using System.IO;

namespace GIGXR.Platform.Mobile
{
    public static class FileInfoExtensions
    {
        /// <summary>
        /// Converts a `FileInfo` to a clip name.
        /// </summary>
        /// <param name="file">The file to convert.</param>
        /// <returns>A clip name.</returns>
        public static string ToClipName(this FileInfo file)
        {
            return $"{file.Directory?.Name}/{file.Name}";
        }
    }
}