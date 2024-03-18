using System;

namespace GIGXR.Platform.Utilities
{
    public static class UriExtensions
    {
        /// <summary>
        /// Returns a new Uri with a path appended.
        ///
        /// Note: this treats input Uris without trailing slashes as directories.
        ///
        /// E.g., AppendPath(new Uri("http://localhost/foo"), "bar"); // http://localhost/foo/bar
        /// </summary>
        /// <param name="uri">The base Uri.</param>
        /// <param name="path">The path to append.</param>
        /// <returns>A new Uri with the path appended.</returns>
        public static Uri AppendPath(this Uri uri, string path)
        {
            var builder = new UriBuilder(uri);
            builder.Path = StringUtilities.AddTrailingSlashIfMissing(builder.Path);
            builder.Path += StringUtilities.RemoveLeadingSlashIfPresent(path);

            return builder.Uri;
        }

        /// <summary>
        /// Returns a new Uri with a query string appended.
        ///
        /// Note: will accept a queryString value with or without a leading separator.
        ///
        /// <code>
        /// AppendQueryString(uri, "foo=bar"); // Valid
        /// AppendQueryString(uri, "?foo=bar"); // Valid
        /// AppendQueryString(uri, "&foo=bar"); // Valid
        /// </code>
        /// </summary>
        /// <param name="uri">The base Uri.</param>
        /// <param name="queryString">The query string to append.</param>
        /// <returns>A new Uri with the query string appended.</returns>
        public static Uri AppendQueryString(this Uri uri, string queryString)
        {
            if (queryString == string.Empty)
                return uri;

            if (queryString[0] == '&' || queryString[0] == '?')
                queryString = queryString.Substring(1, queryString.Length - 1);

            var builder = new UriBuilder(uri);
            if (builder.Query.Length > 1)
                builder.Query = builder.Query.Substring(1) + "&" + queryString;
            else
                builder.Query = queryString;

            return builder.Uri;
        }
    }
}