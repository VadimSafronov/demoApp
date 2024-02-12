using Application.Domain;

namespace Application.DataAccess
{
    public interface ITransactionScope
    {
        void UpdateAccounts(decimal transferAmount, Account senderAccount, Account receiverAccount);
        void UpdateAccount(decimal amountToWithdraw, Account account);
    }
}