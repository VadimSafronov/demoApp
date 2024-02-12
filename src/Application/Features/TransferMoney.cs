using Application.Domain;
using Application.DataAccess;
using System;

namespace Application.Features
{
    public class TransferMoney
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionScope _transactionScope;
        public TransferMoney(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            _transactionScope = new Transaction(_accountRepository);
        }

        public void Execute(Guid senderAccountId, Guid receiverAccountId, decimal transferAmount)
        {
            var senderAccount = _accountRepository.GetAccountById(senderAccountId);
            var receiverAccount = _accountRepository.GetAccountById(receiverAccountId);

            if (senderAccount is null || receiverAccount is null)
            {
                throw new ArgumentException(ExceptionMessages.InvalidAccountIds);
            }

            _transactionScope.UpdateAccounts(transferAmount, senderAccount, receiverAccount);
        }
    }
}
