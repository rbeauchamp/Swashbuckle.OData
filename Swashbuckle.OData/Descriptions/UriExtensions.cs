using System;
using System.Globalization;

namespace Swashbuckle.OData.Descriptions
{
    internal static class UriExtensions
    {
        /// <summary>
        /// Appends a segment to provided URI
        /// </summary>
        /// <param name="uri">URI to append a segment to</param>
        /// <param name="segment">A segment to append</param>
        /// <returns>Adjusted URI</returns>
        public static string AppendUriSegment(this string uri, string segment)
        {
            if (segment == null) 
                throw new ArgumentNullException(nameof(segment));

            // Disassemble provided URI into major parts
            var parts = uri.Split('?');
            string uriQueryAndFragment = (parts.Length == 2) ? parts[1] : "";
            string uriPath = parts[0];

            // define URL separator character
            char pathSeparator = '/';
            
            // make sure the segment value gets properly encoded
            var encodedSegment = EncodeUri(segment.Trim(pathSeparator).ToString(CultureInfo.InvariantCulture)).Replace("?", "%3F");

            // add segment to the base URI path
            uriPath = $"{uriPath}{pathSeparator}{encodedSegment}";

            return ConstructUri(uriPath, uriQueryAndFragment);
        }

        private static string EncodeUri(string urlPart)
        {
            var unescaped = Uri.UnescapeDataString(urlPart);
            return Uri.EscapeUriString(unescaped);
        }

        /// <summary>
        /// Constructs URI from provided parts
        /// </summary>
        /// <param name="path">URI path</param>
        /// <param name="query">URI query string</param>
        /// <param name="fragment">URI fragment</param>
        /// <returns>URI value</returns>
        private static string ConstructUri(string path, string query)
        {
            var sb = new System.Text.StringBuilder(path);
            if (!string.IsNullOrWhiteSpace(query))
                sb.Append("?").Append(query);
            return sb.ToString();
        }

    }
}
