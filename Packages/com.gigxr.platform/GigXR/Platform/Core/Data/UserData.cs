using System;

namespace GIGXR.Platform.Data
{
    [Serializable]
    public class UserData
    {
        public string jti;
        public string nameid;
        public string unique_name;
        public string role;
        public string institutionId;
        public string departmentIds;
        public string clientAppId;
        public string firstName;
        public string lastName;
        public string nbf;
        public string exp;
        public string iat;
    }
}