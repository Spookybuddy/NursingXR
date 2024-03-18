using System;

namespace GIGXR.GMS.Models.Accounts
{
    public class AccountBasicView
    {
        public AccountBasicView(
            Guid accountId,
            string firstName,
            string lastName)
        {
            AccountId = accountId;
            FirstName = firstName;
            LastName = lastName;
        }

        public Guid AccountId { get; }

        public string FirstName { get; }

        public string LastName { get; }
    }
}