namespace Application.Domain
{
    public static class ExceptionMessages
    {
        public const string InvalidAccountIds = "Invalid account IDs.";
        public const string InsufficientFunds = "Insufficient funds to make transfer.";
        public const string PayInLimitExceeded = "Account pay in limit reached.";
        public const string InsufficientFundsToWithdraw = "Insufficient funds to make withdrawal";
        public const string NegativeInitialBalance = "Initial balance cannot be negative.";
    }
}