namespace GIGXR.GMS.Models.Accounts
{
    using System;

    public class GmsAccount
    {
        public GmsAccount(Guid accountId, Guid institutionId)
        {
            AccountId = accountId;
            InstitutionId = institutionId;
        }

        public Guid AccountId { get; }
        public Guid InstitutionId { get; }
    }
}