namespace GIGXR.Platform.Utilities
{
    public static class NumberUtils
    {
        /// <summary>
        /// Converts gigabytes to bytes.
        /// </summary>
        /// <param name="gigabytes">The number of gigabytes to convert.</param>
        /// <returns>The converted amount in bytes.</returns>
        public static long GigabytesToBytes(long gigabytes)
        {
            return gigabytes * 1000 * 1000 * 1000;
        }

        /// <inheritdoc cref="GigabytesToBytes(long)"/>
        public static long GigabytesToBytes(int gigabytes) => GigabytesToBytes((long)gigabytes);

        /// <summary>
        /// Converts megabytes to bytes.
        /// </summary>
        /// <param name="megabytes">The number of megabytes to convert.</param>
        /// <returns>The converted amount in bytes.</returns>
        public static long MegabytesToBytes(long megabytes)
        {
            return megabytes * 1000 * 1000;
        }

        /// <inheritdoc cref="MegabytesToBytes(long)"/>
        public static long MegabytesToBytes(int megabytes) => MegabytesToBytes((long)megabytes);

        /// <summary>
        /// Converts bytes to a human readable format.
        ///
        /// Uses SI units.
        ///
        /// Source: https://www.somacon.com/p576.php
        /// </summary>
        /// <param name="bytes">The number of bytes to convert.</param>
        /// <returns>A human readable representation of bytes.</returns>
        public static string GetBytesReadable(long bytes)
        {
            // Get absolute value
            var absoluteValue = bytes < 0 ? -bytes : bytes;

            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absoluteValue >= 1e18) // Exabyte
            {
                suffix = "EB";
                readable = bytes / 1000000000000000L;
            }
            else if (absoluteValue >= 1000000000000000L) // Petabyte
            {
                suffix = "PB";
                readable = bytes / 1000000000000L;
            }
            else if (absoluteValue >= 1000000000000L) // Terabyte
            {
                suffix = "TB";
                readable = bytes / 1000000000L;
            }
            else if (absoluteValue >= 1000000000L) // Gigabyte
            {
                suffix = "GB";
                readable = bytes / 1000000L;
            }
            else if (absoluteValue >= 1000000L) // Megabyte
            {
                suffix = "MB";
                readable = bytes / 1000L;
            }
            else if (absoluteValue >= 1000L) // Kilobyte
            {
                suffix = "KB";
                readable = bytes;
            }
            else
            {
                return bytes.ToString("0 B"); // Byte
            }

            // Divide by 1000 to get fractional value
            readable /= 1000L;

            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}