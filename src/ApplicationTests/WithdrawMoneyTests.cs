using Application.DataAccess;
using Application.Domain;
using Application.Domain.Services;
using Application.Features;
using Moq;

namespace ApplicationTests;

public class WithdrawMoneyTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly WithdrawMoney _withdrawMoney;
    private readonly Account _account;
    const decimal WithdrawAmount = 500m;
    
    public WithdrawMoneyTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _withdrawMoney = new WithdrawMoney(_accountRepositoryMock.Object);
        _account = new Account(
            Guid.NewGuid(),
            new User
            {
                Id = Guid.NewGuid(),
                Name = "John Doe",
                Email = "john.doe@example.com"
            },
            2000m,
            _notificationServiceMock.Object);
    }

    [Fact]
    public void WithdrawMoney_ShouldUpdateBalanceAndNotifyFundsLow()
    {
        _accountRepositoryMock.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);
            
        _withdrawMoney.Execute(_account.Id, WithdrawAmount);

        _accountRepositoryMock.Verify(repo => repo.Update(_account), Times.Once);
        _notificationServiceMock.Verify(service => service.NotifyFundsLow(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void WithdrawMoney_BalanceShouldBeCorrectAfterWithdraw()
    {
        var expectedAfterWithdrawBalance = _account.Balance - WithdrawAmount;
        var expectedAfterWithdraw = _account.Withdrawn + WithdrawAmount;
        
        _accountRepositoryMock.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);
            
        _withdrawMoney.Execute(_account.Id, WithdrawAmount);
        
        Assert.Equal(expectedAfterWithdrawBalance, _account.Balance);
        Assert.Equal(expectedAfterWithdraw, _account.Withdrawn);
        _accountRepositoryMock.Verify(repo => repo.Update(_account), Times.Once);
        
    }
    
    [Fact]
    public void WithdrawMoney_InsufficientFunds_ExceptionThrown()
    {
        _accountRepositoryMock.Setup(repo => repo.GetAccountById(_account.Id)).Returns(_account);
        
        InvalidOperationException exception = Assert.Throws<InvalidOperationException>
            (() => _withdrawMoney.Execute(_account.Id, 3000m));
        Assert.Equal(ExceptionMessages.InsufficientFundsToWithdraw, exception.Message);
        _accountRepositoryMock.Verify(repo => repo.Update(_account), Times.Never);
    }
    
    [Fact]
    public void WithdrawMoney_AccountNotFound_ExceptionThrown()
    {
        _accountRepositoryMock.Setup(repo => repo.GetAccountById(_account.Id)).Returns((Account)null);

        ArgumentException exception = Assert.Throws<ArgumentException>
            (() => _withdrawMoney.Execute(_account.Id, WithdrawAmount));
        Assert.Equal(ExceptionMessages.InvalidAccountIds, exception.Message);
    }
}