// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
#if ASPNETWEBAPI
using ErrorResources = Swashbuckle.OData.ApiExplorer.SRResources;
using TParsedRoute = Swashbuckle.OData.ApiExplorer.HttpParsedRoute;

#else
using ErrorResources = System.Web.Mvc.Properties.MvcResources;
using TParsedRoute = System.Web.Mvc.Routing.ParsedRoute;
#endif

#if ASPNETWEBAPI

namespace Swashbuckle.OData.ApiExplorer
#else
namespace System.Web.Mvc.Routing
#endif
{
    // in the MVC case, route parsing is done for AttributeRouting's sake, so that
    // it could order the discovered routes before pushing them into the routeCollection,
    // where, unfortunately, they would be parsed again.
    internal static class RouteParser
    {
        private static string GetLiteral(string segmentLiteral)
        {
            // Scan for errant single { and } and convert double {{ to { and double }} to }

            // First we eliminate all escaped braces and then check if any other braces are remaining
            var newLiteral = segmentLiteral.Replace("{{", string.Empty).Replace("}}", string.Empty);
            if (newLiteral.Contains("{") || newLiteral.Contains("}"))
            {
                return null;
            }

            // If it's a valid format, we unescape the braces
            return segmentLiteral.Replace("{{", "{").Replace("}}", "}");
        }

        private static int IndexOfFirstOpenParameter(string segment, int startIndex)
        {
            // Find the first unescaped open brace
            while (true)
            {
                startIndex = segment.IndexOf('{', startIndex);
                if (startIndex == -1)
                {
                    // If there are no more open braces, stop
                    return -1;
                }
                if ((startIndex + 1 == segment.Length) || ((startIndex + 1 < segment.Length) && (segment[startIndex + 1] != '{')))
                {
                    // If we found an open brace that is followed by a non-open brace, it's
                    // a parameter delimiter.
                    // It's also a delimiter if the open brace is the last character - though
                    // it ends up being being called out as invalid later on.
                    return startIndex;
                }
                // Increment by two since we want to skip both the open brace that
                // we're on as well as the subsequent character since we know for
                // sure that it is part of an escape sequence.
                startIndex += 2;
            }
        }

        internal static bool IsSeparator(string s)
        {
            return string.Equals(s, "/", StringComparison.Ordinal);
        }

        private static bool IsValidParameterName(string parameterName)
        {
            if (parameterName.Length == 0)
            {
                return false;
            }

            for (var i = 0; i < parameterName.Length; i++)
            {
                var c = parameterName[i];
                if (c == '/' || c == '{' || c == '}')
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsInvalidRouteTemplate(string routeTemplate)
        {
            return routeTemplate.StartsWith("~", StringComparison.Ordinal) || routeTemplate.StartsWith("/", StringComparison.Ordinal) || (routeTemplate.IndexOf('?') != -1);
        }

        public static TParsedRoute Parse(string routeTemplate)
        {
            if (routeTemplate == null)
            {
                routeTemplate = string.Empty;
            }

            if (IsInvalidRouteTemplate(routeTemplate))
            {
                throw Error.Argument("routeTemplate", ErrorResources.Route_InvalidRouteTemplate);
            }

            var uriParts = SplitUriToPathSegmentStrings(routeTemplate);
            var ex = ValidateUriParts(uriParts);
            if (ex != null)
            {
                throw ex;
            }

            var pathSegments = SplitUriToPathSegments(uriParts);

            Contract.Assert(uriParts.Count == pathSegments.Count, "The number of string segments should be the same as the number of path segments");

            return new TParsedRoute(pathSegments);
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exceptions are just constructed here, but they are thrown from a method that does have those parameter names.")]
        private static List<PathSubsegment> ParseUriSegment(string segment, out Exception exception)
        {
            var startIndex = 0;

            var pathSubsegments = new List<PathSubsegment>();

            while (startIndex < segment.Length)
            {
                var nextParameterStart = IndexOfFirstOpenParameter(segment, startIndex);
                if (nextParameterStart == -1)
                {
                    // If there are no more parameters in the segment, capture the remainder as a literal and stop
                    var lastLiteralPart = GetLiteral(segment.Substring(startIndex));
                    if (lastLiteralPart == null)
                    {
                        exception = Error.Argument("routeTemplate", ErrorResources.Route_MismatchedParameter, segment);
                        return null;
                    }

                    if (lastLiteralPart.Length > 0)
                    {
                        pathSubsegments.Add(new PathLiteralSubsegment(lastLiteralPart));
                    }
                    break;
                }

                var nextParameterEnd = segment.IndexOf('}', nextParameterStart + 1);
                if (nextParameterEnd == -1)
                {
                    exception = Error.Argument("routeTemplate", ErrorResources.Route_MismatchedParameter, segment);
                    return null;
                }

                var literalPart = GetLiteral(segment.Substring(startIndex, nextParameterStart - startIndex));
                if (literalPart == null)
                {
                    exception = Error.Argument("routeTemplate", ErrorResources.Route_MismatchedParameter, segment);
                    return null;
                }

                if (literalPart.Length > 0)
                {
                    pathSubsegments.Add(new PathLiteralSubsegment(literalPart));
                }

                var parameterName = segment.Substring(nextParameterStart + 1, nextParameterEnd - nextParameterStart - 1);
                pathSubsegments.Add(new PathParameterSubsegment(parameterName));

                startIndex = nextParameterEnd + 1;
            }

            exception = null;
            return pathSubsegments;
        }

        private static List<PathSegment> SplitUriToPathSegments(List<string> uriParts)
        {
            var pathSegments = new List<PathSegment>();

            foreach (var pathSegment in uriParts)
            {
                var isCurrentPartSeparator = IsSeparator(pathSegment);
                if (isCurrentPartSeparator)
                {
                    pathSegments.Add(new PathSeparatorSegment());
                }
                else
                {
                    Exception exception;
                    var subsegments = ParseUriSegment(pathSegment, out exception);
                    Contract.Assert(exception == null, "This only gets called after the path has been validated, so there should never be an exception here");
                    pathSegments.Add(new PathContentSegment(subsegments));
                }
            }
            return pathSegments;
        }

        internal static List<string> SplitUriToPathSegmentStrings(string uri)
        {
            var parts = new List<string>();

            if (string.IsNullOrEmpty(uri))
            {
                return parts;
            }

            var currentIndex = 0;

            // Split the incoming URI into individual parts
            while (currentIndex < uri.Length)
            {
                var indexOfNextSeparator = uri.IndexOf('/', currentIndex);
                if (indexOfNextSeparator == -1)
                {
                    // If there are no more separators, the rest of the string is the last part
                    var finalPart = uri.Substring(currentIndex);
                    if (finalPart.Length > 0)
                    {
                        parts.Add(finalPart);
                    }
                    break;
                }

                var nextPart = uri.Substring(currentIndex, indexOfNextSeparator - currentIndex);
                if (nextPart.Length > 0)
                {
                    parts.Add(nextPart);
                }

                Contract.Assert(uri[indexOfNextSeparator] == '/', "The separator char itself should always be a '/'.");
                parts.Add("/");
                currentIndex = indexOfNextSeparator + 1;
            }

            return parts;
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily", Justification = "Not changing original algorithm")]
        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exceptions are just constructed here, but they are thrown from a method that does have those parameter names.")]
        private static Exception ValidateUriParts(List<string> pathSegments)
        {
            Contract.Assert(pathSegments != null, "The value should always come from SplitUri(), and that function should never return null.");

            var usedParameterNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            bool? isPreviousPartSeparator = null;

            var foundCatchAllParameter = false;

            foreach (var pathSegment in pathSegments)
            {
                if (foundCatchAllParameter)
                {
                    // If we ever start an iteration of the loop and we've already found a
                    // catchall parameter then we have an invalid URI format.
                    return Error.Argument("routeTemplate", ErrorResources.Route_CatchAllMustBeLast, "routeTemplate");
                }

                bool isCurrentPartSeparator;
                if (isPreviousPartSeparator == null)
                {
                    // Prime the loop with the first value
                    isPreviousPartSeparator = IsSeparator(pathSegment);
                    isCurrentPartSeparator = isPreviousPartSeparator.Value;
                }
                else
                {
                    isCurrentPartSeparator = IsSeparator(pathSegment);

                    // If both the previous part and the current part are separators, it's invalid
                    if (isCurrentPartSeparator && isPreviousPartSeparator.Value)
                    {
                        return Error.Argument("routeTemplate", ErrorResources.Route_CannotHaveConsecutiveSeparators);
                    }

                    Contract.Assert(isCurrentPartSeparator != isPreviousPartSeparator.Value, "This assert should only happen if both the current and previous parts are non-separators. This should never happen because consecutive non-separators are always parsed as a single part.");
                    isPreviousPartSeparator = isCurrentPartSeparator;
                }

                // If it's not a separator, parse the segment for parameters and validate it
                if (!isCurrentPartSeparator)
                {
                    Exception exception;
                    var subsegments = ParseUriSegment(pathSegment, out exception);
                    if (exception != null)
                    {
                        return exception;
                    }

                    exception = ValidateUriSegment(subsegments, usedParameterNames);
                    if (exception != null)
                    {
                        return exception;
                    }

                    foundCatchAllParameter = subsegments.Any(seg => seg is PathParameterSubsegment && ((PathParameterSubsegment) seg).IsCatchAll);
                }
            }
            return null;
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "The exceptions are just constructed here, but they are thrown from a method that does have those parameter names.")]
        private static Exception ValidateUriSegment(List<PathSubsegment> pathSubsegments, HashSet<string> usedParameterNames)
        {
            var segmentContainsCatchAll = false;

            Type previousSegmentType = null;

            foreach (var subsegment in pathSubsegments)
            {
                if (previousSegmentType != null)
                {
                    if (previousSegmentType == subsegment.GetType())
                    {
                        return Error.Argument("routeTemplate", ErrorResources.Route_CannotHaveConsecutiveParameters);
                    }
                }
                previousSegmentType = subsegment.GetType();

                var literalSubsegment = subsegment as PathLiteralSubsegment;
                if (literalSubsegment != null)
                {
                    // Nothing to validate for literals - everything is valid
                }
                else
                {
                    var parameterSubsegment = subsegment as PathParameterSubsegment;
                    if (parameterSubsegment != null)
                    {
                        var parameterName = parameterSubsegment.ParameterName;

                        if (parameterSubsegment.IsCatchAll)
                        {
                            segmentContainsCatchAll = true;
                        }

                        // Check for valid characters in the parameter name
                        if (!IsValidParameterName(parameterName))
                        {
                            return Error.Argument("routeTemplate", ErrorResources.Route_InvalidParameterName, parameterName);
                        }

                        if (usedParameterNames.Contains(parameterName))
                        {
                            return Error.Argument("routeTemplate", ErrorResources.Route_RepeatedParameter, parameterName);
                        }
                        usedParameterNames.Add(parameterName);
                    }
                    else
                    {
                        Contract.Assert(false, "Invalid path subsegment type");
                    }
                }
            }

            if (segmentContainsCatchAll && (pathSubsegments.Count != 1))
            {
                return Error.Argument("routeTemplate", ErrorResources.Route_CannotHaveCatchAllInMultiSegment);
            }

            return null;
        }
    }
}