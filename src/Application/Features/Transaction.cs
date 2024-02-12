using Application.DataAccess;
using Application.Domain;

namespace Application.Features
{
    public class Transaction : ITransactionScope
    {
        private readonly IAccountRepository _accountRepository;

        public Transaction(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
        }

        public void UpdateAccounts(decimal transferAmount, Account senderAccount, Account receiverAccount)
        {
            UpdateSenderBalance(transferAmount, senderAccount);
            UpdateReceiverBalance(transferAmount, receiverAccount);
        }
        private void UpdateReceiverBalance(decimal transferAmount, Account receiverAccount)
        {
            receiverAccount.PayIn(transferAmount);
            _accountRepository.Update(receiverAccount);
        }

        private void UpdateSenderBalance(decimal transferAmount, Account senderAccount)
        {
            senderAccount.PayOut(transferAmount);
            _accountRepository.Update(senderAccount);
        }

        public void UpdateAccount(decimal amountToWithdraw, Account account)
        {
            account.Withdraw(amountToWithdraw);
            _accountRepository.Update(account);
        }
    }
}