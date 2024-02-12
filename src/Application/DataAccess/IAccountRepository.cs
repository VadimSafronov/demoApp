using System;
using Application.Domain;

namespace Application.DataAccess
{
    public interface IAccountRepository
    {
        Account GetAccountById(Guid accountId);

        void Update(Account account);
    }
}
