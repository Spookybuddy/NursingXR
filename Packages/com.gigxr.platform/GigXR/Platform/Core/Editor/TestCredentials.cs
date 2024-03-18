using UnityEngine;

namespace GIGXR.Platform.Core
{
    /// <summary>
    /// <c>TestCredentials</c> is TODO 
    /// </summary>
    /// <remarks>
    /// Please store credentials in a gitignored directory.  
    /// </remarks>
    [CreateAssetMenu(menuName = "GIGXR/ScriptableObjects/New Test Credentials")]
    public class TestCredentials : ScriptableObject
    {
        /// <summary>
        /// Email used to authenticate the user.
        /// </summary>
        public string Email;
        
        /// <summary>
        /// Password used to authenticate the user.
        /// </summary>
        public string Password;
    }
}
