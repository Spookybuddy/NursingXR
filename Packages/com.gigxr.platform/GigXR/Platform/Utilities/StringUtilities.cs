namespace GIGXR.Platform.Utilities
{
    using System;

    public static class StringUtilities
    {
        /// <summary>
        /// Adds a trailing slash to a string if one is not there. Returns a single "/" for an empty string.
        /// </summary>
        /// <param name="value">The string to add a slash.</param>
        /// <returns>A string with a trailing slash.</returns>
        public static string AddTrailingSlashIfMissing(string value)
        {
            if (value == string.Empty)
                return "/";

            if (value[value.Length - 1] != '/')
                value += '/';

            return value;
        }

        /// <summary>
        /// Removes a leading slash from a string if it is present.
        /// </summary>
        /// <param name="value">The string to remove a slash.</param>
        /// <returns>A string without a leading slash.</returns>
        public static string RemoveLeadingSlashIfPresent(string value)
        {
            if (value == string.Empty)
                return value;

            if (value[0] == '/')
                value = value.Substring(1, value.Length - 1);

            return value;
        }

        public static string FirstLetterToLowerInvariant(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            if (value.Length == 1)
                return value.ToLowerInvariant();

            return Char.ToLowerInvariant(value[0]) + value.Substring(1);
        }
    }
}