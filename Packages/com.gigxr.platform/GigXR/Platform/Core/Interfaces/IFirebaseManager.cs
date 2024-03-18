namespace GIGXR.Platform.Interfaces
{
    public interface IFirebaseManager
    {
        public delegate void DynamicLinkReceivedEventHandler(string path);
        event DynamicLinkReceivedEventHandler DynamicLinkReceived;

        bool TryEnableMessaging();
        bool TryEnableDynamicLinks();
        // TODO This endpoint doesn't exist in GMS anymore, not sure if we should keep this around
        //void DeleteFirebaseToken();
    }
}