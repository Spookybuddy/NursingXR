namespace GIGXR.GMS.Models.Sessions.Requests
{
    using Newtonsoft.Json;

    public class UpdateSessionLockRequest
    {
        [JsonRequired] public bool Locked { get; set; }
    }

    public class UpdateSessionPermissionRequest
    {
        [JsonRequired] public SessionPermission Permission { get; set; }
    }
}