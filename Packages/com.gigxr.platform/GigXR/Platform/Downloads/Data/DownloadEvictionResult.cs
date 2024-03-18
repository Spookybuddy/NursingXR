/*
 * THIS CLASS IS NOT IN USE.
 * 
 * It is a part of old download management utilities, and has been left
 * as a reference for upcoming Content Management efforts - CU-1x0q7ce
 */

/*
using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using GIGXR.Platform.Utilities;

namespace GIGXR.Platform.Downloads.Data
{
    /// <summary>
    /// Represents the result of running a <c>DownloadEvictionRequest</c>.
    /// </summary>
    public class DownloadEvictionResult
    {
        public DownloadEvictionRequest Request { get; }

        public bool Success { get; }

        public long BytesEvicted { get; }

        public List<DownloadEviction> EvictedDownloads = new List<DownloadEviction>();

        public List<Guid> UnneededResourceIds = new List<Guid>();

        public List<Guid> NeededResourceIds = new List<Guid>();

        [CanBeNull] public string ErrorMessage { get; }

        public DownloadEvictionResult(
            DownloadEvictionRequest request,
            bool success,
            long bytesEvicted,
            List<DownloadEviction> evictedDownloads,
            List<Guid> unneededResourceIds,
            List<Guid> neededResourceIds,
            [CanBeNull] string errorMessage = null)
        {
            Request = request;
            Success = success;
            ErrorMessage = errorMessage;
            BytesEvicted = bytesEvicted;
            EvictedDownloads = evictedDownloads;
            UnneededResourceIds = unneededResourceIds;
            NeededResourceIds = neededResourceIds;
        }

        public DownloadEvictionResult(
            DownloadEvictionRequest request,
            bool success,
            string errorMessage)
        {
            Request = request;
            Success = success;
            ErrorMessage = errorMessage;
        }

        public DownloadEvictionResult(
            DownloadEvictionRequest request,
            bool success)
        {
            Request = request;
            Success = success;
        }

        public override string ToString()
        {
            var output = new StringBuilder();
            output.AppendLine();
            output.AppendLine("============================= Download Eviction Result =============================");
            output.AppendLine();

            output.AppendLine("DownloadEvictionRequest:");
            output.AppendLine($"Bytes to free: {NumberUtils.GetBytesReadable(Request.BytesToFree)}.");
            output.AppendLine($"Locked resource IDs: {string.Join(", ", Request.LockedResourceIds)}");
            output.AppendLine($"Can evict needed resources: {Request.CanEvictNeededResources}.");
            output.AppendLine();


            output.AppendLine("Summary:");
            output.AppendLine(Success ? "Eviction successful!" : $"Eviction failed! Error: {ErrorMessage}");

            output.AppendLine();
            output.Append($"{EvictedDownloads.Count} download{(EvictedDownloads.Count == 1 ? "" : "s")} evicted to ");
            output.AppendLine($"free {NumberUtils.GetBytesReadable(BytesEvicted)}.");
            output.AppendLine();


            output.AppendLine("Downloads evicted:");
            if (EvictedDownloads.Count > 0)
            {
                EvictedDownloads.ForEach(download =>
                {
                    output.AppendLine($" - {download.ResourceId}: {NumberUtils.GetBytesReadable(download.Length)}");
                });
            }
            else
            {
                output.AppendLine(" - No downloads evicted.");
            }

            output.AppendLine();

            output.AppendLine("On device unneeded downloads:");
            output.AppendLine(UnneededResourceIds.Count > 0 ? $"{string.Join(", ", UnneededResourceIds)}" : "-");

            output.AppendLine();

            output.AppendLine("Downloads needed for the future:");
            output.AppendLine(NeededResourceIds.Count > 0 ? $"{string.Join(", ", NeededResourceIds)}" : "-");

            output.AppendLine();

            output.AppendLine("================================================================================");

            return output.ToString();
        }
    }
}
*/
