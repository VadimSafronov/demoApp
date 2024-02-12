using Application.DataAccess;
using Application.Domain;
using Application.Domain.Services;
using Application.Features;
using Moq;

namespace ApplicationTests
{
    public class TransferMoneyTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly TransferMoney _transferMoney;

        public TransferMoneyTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _transferMoney = new TransferMoney(_accountRepositoryMock.Object);
        }

        [Fact]
        public void TransferMoney_SuccessfullyUpdatedBothAccounts_NoWarnings()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                2000m,
                _notificationServiceMock.Object);
            senderAccount.PayIn(3000m);
            senderAccount.Withdraw(3000m);

            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Jane Doe",
                    Email = "jane.doe@example.com"
                },
                0m,
                _notificationServiceMock.Object);

            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);
            
            _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, 600m);

            _accountRepositoryMock.Verify(repo => repo.Update(senderAccount), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.Update(receiverAccount), Times.Once);
            _notificationServiceMock.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationServiceMock.Verify(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Never);
        }
        
        [Fact]
        public void TransferMoney_ReceivedTransfer_BalanceAndPaidInUpdated_Success()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                2000m,
                _notificationServiceMock.Object);
            
            senderAccount.PayIn(3000m);
            senderAccount.Withdraw(3000m);
            
            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Jane Doe",
                    Email = "jane.doe@example.com"
                },
                0m,
                _notificationServiceMock.Object);
            
            
            
            const decimal transferAmount = 500m;
            var expectedReceiverBalance = receiverAccount.Balance + transferAmount;
            var expectedReceiverPaidIn = receiverAccount.PaidIn + transferAmount;
            
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);
            
            _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, transferAmount);
            
            Assert.Equal(expectedReceiverBalance, receiverAccount.Balance);
            Assert.Equal(expectedReceiverPaidIn, receiverAccount.PaidIn);
        }
        
        [Fact]
        public void TransferMoney_SenderBalanceUpdated_Success()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                2000m,
                _notificationServiceMock.Object);
            senderAccount.PayIn(3000m);
            senderAccount.Withdraw(3000m);

            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "Jane Doe",
                    Email = "jane.doe@example.com"
                },
                0m,
                _notificationServiceMock.Object);
            
            const decimal transferAmount = 500m;
            var expectedSenderBalance = senderAccount.Balance - transferAmount;
            var expectedSenderWithdrawn = senderAccount.Withdrawn - transferAmount;
            
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);
            
            _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, transferAmount);
            
            Assert.Equal(expectedSenderBalance, senderAccount.Balance);
            Assert.Equal(expectedSenderWithdrawn, senderAccount.Withdrawn);
        }
        
        
        [Fact]
        public void TransferMoney_NotificationApproachingPayInLimit_Thrown()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                2000m,
                _notificationServiceMock.Object);
            
            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                1000m,
                _notificationServiceMock.Object);
            receiverAccount.PayIn(3500m);
            receiverAccount.Withdraw(3500m);
            
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);
            
            _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, 500m);
            
            _notificationServiceMock.Verify(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Exactly(1));
            _notificationServiceMock.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            
            _accountRepositoryMock.Verify(repo => repo.Update(senderAccount), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.Update(receiverAccount), Times.Once);
        }

        [Fact]
        public void TransferMoney_NotificationPayInLimitAndLowFunds_Thrown()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                600m,
                _notificationServiceMock.Object);
            
            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                1000m,
                _notificationServiceMock.Object);
            
                receiverAccount.PayIn(3500m);
                receiverAccount.Withdraw(3500m);
                
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);
            
            _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, 500m);
            
            _notificationServiceMock.Verify(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
            _notificationServiceMock.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Once);
            
            _accountRepositoryMock.Verify(repo => repo.Update(senderAccount), Times.Once);
            _accountRepositoryMock.Verify(repo => repo.Update(receiverAccount), Times.Once);
        }
        
        [Fact]
        public void TransferMoney_InsufficientFunds_Exception_Thrown()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                200m,
                _notificationServiceMock.Object);

            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>
                (() => _transferMoney.Execute(senderAccount.Id, senderAccount.Id, 500m));
            Assert.Equal(ExceptionMessages.InsufficientFunds, exception.Message);
        }
        
        [Fact]
        public void TransferMoney_ObjectsAreNull_Exception_Thrown()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                200m,
                _notificationServiceMock.Object);
            
            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                200m,
                _notificationServiceMock.Object);

            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns((Account)null);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns((Account)null);

            ArgumentException exception = Assert.Throws<ArgumentException>
                (() => _transferMoney.Execute(senderAccount.Id, senderAccount.Id, 500m));
            Assert.Equal(ExceptionMessages.InvalidAccountIds, exception.Message);
        }
        
        
        [Fact]
        public void TransferMoney_PayInLimitException_Thrown()
        {
            var senderAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                1000m,
                _notificationServiceMock.Object);

            var receiverAccount = new Account(
                Guid.NewGuid(),
                new User
                {
                    Id = Guid.NewGuid(),
                    Name = "John Doe",
                    Email = "john.doe@example.com"
                },
                0m,
                _notificationServiceMock.Object);
            receiverAccount.PayIn(3501m);

            _accountRepositoryMock.Setup(repo => repo.GetAccountById(receiverAccount.Id)).Returns(receiverAccount);
            _accountRepositoryMock.Setup(repo => repo.GetAccountById(senderAccount.Id)).Returns(senderAccount);

            InvalidOperationException exception =
                Assert.Throws<InvalidOperationException>(() =>
                    _transferMoney.Execute(senderAccount.Id, receiverAccount.Id, 500m));
            Assert.Equal(ExceptionMessages.PayInLimitExceeded, exception.Message);
            
            _notificationServiceMock.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Never);
            _notificationServiceMock.Verify(service => service.NotifyApproachingPayInLimit(It.IsAny<string>()), Times.Once);
        }
    }
}