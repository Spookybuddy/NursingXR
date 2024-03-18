namespace GigXR.Platform.GMS.Models.Accounts.Responses
{
    using System;
    using System.Collections.Generic;

    public class InstitutionDetailedView
    {
        public Guid InstitutionId { get; set; }
        public string InstitutionName { get; set; }
        public Guid ContactId { get; set; }
        public ContactView Contact;
        public Guid AddressId { get; set; }
        public AddressView Address { get; set; }
        public Guid PhoneNumberId { get; set; }
        public PhoneNumberView PhoneNumber { get; set; }
        public bool IsDemoAccount { get; set; }
        public bool CanMobileCreateSessions { get; set; }
        public int AccountCount { get; set; }
        public int SessionsInProgressCount { get; set; }
        public int AccountsInSessionsCount { get; set; }
        public int TotalSavedSessions { get; set; }
        public int TotalSessionsRun { get; set; }
        public InstitutionConfigView InstitutionConfig { get; set; }
        public List<InstitutionAdminView> InstitutionAdmins { get; set; }
        public List<Guid> LicenseIds { get; set; }
        public DateTime CreatedOn { get; set; }
        public Guid CreatedById { get; set; }
        public CreatorView CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid ModifiedById { get; set; }
        public CreatorView ModifiedBy { get; set; }
    }

    public class ContactView
    {
        public Guid ContactId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
    }

    public class AddressView
    {
        public Guid AddressId { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string Address3 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
    }

    public class PhoneNumberView
    {
        public Guid PhoneNumberId { get; set; }
        public string Number { get; set; }
    }

    public class InstitutionConfigView
    {
        public Guid InstitutionId { get; set; }
        public bool AllowGigXRAccess { get; set; }
    }

    public class InstitutionAdminView
    {
        public Guid AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public bool EmailVerified { get; set; }
        public string AccountRole { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastActive { get; set; }
        public string RegistrationStatus { get; set; }
    }

    public class CreatorView
    {
        public Guid AccountId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}