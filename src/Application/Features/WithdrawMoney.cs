using Application.DataAccess;
using System;
using Application.Domain;

namespace Application.Features
{
    public class WithdrawMoney
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionScope _transactionScope;

        public WithdrawMoney(IAccountRepository accountRepository)
        {
            _accountRepository = accountRepository;
            _transactionScope = new Transaction(_accountRepository);
        }

        public void Execute(Guid withdrawFromAccountId, decimal amountToWithdraw)
        {
            var account = _accountRepository.GetAccountById(withdrawFromAccountId);
            
            if (account is null)
            {
                throw new ArgumentException(ExceptionMessages.InvalidAccountIds);
            }
            
            _transactionScope.UpdateAccount(amountToWithdraw, account);
        }
    }
}
