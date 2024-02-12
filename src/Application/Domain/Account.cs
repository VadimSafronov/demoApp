using System;
using Application.Domain.Services;

namespace Application.Domain
{
    public class Account
    {
        private readonly INotificationService _notificationService;
        public Guid Id { get; private set; }

        private User User { get; set; }

        public decimal Balance { get; private set; }

        public decimal Withdrawn { get; private set; }
        public decimal PaidIn { get; private set; }

        private const decimal PayInLimit = 4000m;
        private const decimal LowFundsMark = 500m;

        public Account(Guid id, User user, decimal initialBalance, INotificationService notificationService)
        {
            if (initialBalance < 0)
                throw new ArgumentException(ExceptionMessages.NegativeInitialBalance, nameof(initialBalance));

            Id = id;
            User = user;
            Balance = initialBalance;
            _notificationService = notificationService;
            Withdrawn = 0;
        }

        public void PayIn(decimal amount)
        {
            var paidIn = PaidIn + amount;
            if (paidIn > PayInLimit)
                throw new InvalidOperationException(ExceptionMessages.PayInLimitExceeded);

            if (PayInLimit - paidIn < LowFundsMark)
                _notificationService.NotifyApproachingPayInLimit(User.Email);

            PaidIn += amount;
            Balance += amount;
        }

        public void Withdraw(decimal amount)
        {
            var balanceAfterWithdraw = Balance - amount;
            if (balanceAfterWithdraw < 0)
                throw new InvalidOperationException(ExceptionMessages.InsufficientFundsToWithdraw);

            if (balanceAfterWithdraw < LowFundsMark)
                _notificationService.NotifyFundsLow(User.Email);

            Withdrawn += amount;
            Balance -= amount;
        }

        public void PayOut(decimal amount)
        {
            var newBalance = Balance - amount;
            if (newBalance < 0)
            {
                throw new InvalidOperationException(ExceptionMessages.InsufficientFunds);
            }

            if (newBalance < LowFundsMark)
            {
                _notificationService.NotifyFundsLow(User.Email);  
            }
            Balance -= amount;
            Withdrawn -= amount;
        }
    }
}
