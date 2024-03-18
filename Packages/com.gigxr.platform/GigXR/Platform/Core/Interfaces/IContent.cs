namespace GIGXR.Platform.Interfaces
{
    public interface IContent
    {
        void ContentFound();

        void DownloadProgressUpdated(int progress, string url);

        void ContentMissing();

        void ContentDownloaded();
    }
}