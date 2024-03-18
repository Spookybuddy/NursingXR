using System;

namespace GIGXR.Platform.Data
{
    /// <summary>
    /// Contains the basic information for a user account
    /// </summary>
    [Serializable]
    public class AccountLeafView
    {
        public string AccountId;
        public string FirstName;
        public string LastName;
        public string Email;
    }
}