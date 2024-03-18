using System;

namespace GIGXR.Platform.Downloads.Data
{
    /// <summary>
    /// Represents a download that can be queued, in-progress, or completed as represented via the <c>Progress</c>
    /// property. 
    /// </summary>
    public class DownloadInfo
    {
        public DownloadInfo(
            Guid resourceId,
            long length,
            float progress)
        {
            ResourceId = resourceId;
            Length = length;
            Progress = progress;
        }

        /// <summary>
        /// The resourceId of the download.
        /// </summary>
        public Guid ResourceId { get; }

        /// <summary>
        /// The download file size in bytes.
        /// </summary>
        public long Length { get; }

        /// <summary>
        /// A value 0.0 - 1.0 representing the download percentage.
        /// </summary>
        public float Progress { get; }
    }
}