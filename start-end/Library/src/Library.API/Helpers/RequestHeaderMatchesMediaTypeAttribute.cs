// ------------------------------------------------------------------------------
//     <copyright file="RequestHeaderMatchesMediaTypeAttribute.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace Library.API.Helpers
{
    [AttributeUsage(AttributeTargets.All, Inherited = true, AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute : Attribute, IActionConstraint
    {
        private readonly string[] c_mediaTypes;

        private readonly string c_requestHeaderToMatch;

        public int Order => 0;

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string[] mediaTypes)
        {
            c_requestHeaderToMatch = requestHeaderToMatch;
            c_mediaTypes = mediaTypes;
        }

        public bool Accept(ActionConstraintContext context)
        {
            IHeaderDictionary requestHeaders = context.RouteContext.HttpContext.Request.Headers;

            if (!requestHeaders.ContainsKey(c_requestHeaderToMatch))
            {
                return false;
            }

            // if one of the media types matches, return true
            foreach (string mediaType in c_mediaTypes)
            {
                bool mediaTypeMatches = string.Equals(requestHeaders[c_requestHeaderToMatch].ToString(), mediaType, StringComparison.OrdinalIgnoreCase);

                if (mediaTypeMatches)
                {
                    return true;
                }
            }

            return false;
        }
    }
}