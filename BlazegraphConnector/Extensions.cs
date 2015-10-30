using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Storage
{
    public static class Extensions
    {
        /// <summary>
        /// Gets either the String representation of the URI or the Empty String if the URI is null
        /// </summary>
        /// <param name="u">URI</param>
        /// <returns></returns>
        internal static string ToSafeString(this Uri u)
        {
            return (u == null) ? string.Empty : u.AbsoluteUri;
        }

        /// <summary>
        /// Turns a string into a safe URI
        /// </summary>
        /// <param name="str">String</param>
        /// <returns>Either null if the string is null/empty or a URI otherwise</returns>
        internal static Uri ToSafeUri(this string str)
        {
            return (string.IsNullOrEmpty(str) ? null : UriFactory.Create(str));
        }
    }
}
