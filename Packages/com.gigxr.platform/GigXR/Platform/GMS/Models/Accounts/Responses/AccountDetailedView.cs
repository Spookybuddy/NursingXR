namespace GIGXR.GMS.Models.Accounts.Responses
{
    using Classes.Resposnes;
    using System;
    using System.Collections.Generic;

    public class AccountDetailedView
    {
        public Guid AccountId { get; set; }

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public bool EmailVerified { get; set; }

        public AccountRole AccountRole { get; set; }

        public Guid InstitutionId { get; set; }

        public bool IsActive { get; set; }

        public DateTime LastActive { get; set; }

        public RegistrationStatus RegistrationStatus { get; set; }

        public ICollection<Guid> DepartmentIds { get; set; } = null!;

        public ICollection<Guid> ClassIds { get; set; } = null!;

        public ICollection<ClassLeafView> Classes { get; set; } = null!;

        public DateTime CreatedOn { get; set; }

        public Guid CreatedById { get; set; }

        public AccountBasicView CreatedBy { get; set; } = null!;

        public DateTime ModifiedOn { get; set; }

        public Guid ModifiedById { get; set; }

        public AccountBasicView ModifiedBy { get; set; } = null!;
    }
}