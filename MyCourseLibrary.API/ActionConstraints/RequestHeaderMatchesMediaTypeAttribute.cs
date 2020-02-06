using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Net.Http.Headers;

namespace CourseLibrary.API.ActionConstraints
{
    [AttributeUsage(AttributeTargets.All,  AllowMultiple = true)]
    public class RequestHeaderMatchesMediaTypeAttribute: Attribute, IActionConstraint
    {
        private readonly string _requestHeaderToMatch;
        private readonly MediaTypeCollection _mediaTypes = new MediaTypeCollection();

        public RequestHeaderMatchesMediaTypeAttribute(string requestHeaderToMatch, string mediaType, params string[] otherMediaTypes)
        {
            _requestHeaderToMatch = requestHeaderToMatch ?? throw new ArgumentNullException(nameof(requestHeaderToMatch));
            var mediaTypes = new List<string>(){mediaType};
            
            foreach (var otherMediaType in otherMediaTypes)
            {
                mediaTypes.Add(otherMediaType);
            }
            SetMediaTypes(mediaTypes);
        }

        public bool Accept(ActionConstraintContext context)
        {
            var headers = context.RouteContext.HttpContext.Request.Headers;
            if (!headers.ContainsKey(_requestHeaderToMatch))
            {
                return false;
            }

            var parsedRequestMediaType = new MediaType(headers[_requestHeaderToMatch]);
            
            return _mediaTypes.Select(mediaType => new MediaType(mediaType)).Contains(parsedRequestMediaType);
        }

        public int Order => HttpMethodActionConstraint.HttpMethodConstraintOrder + 1 ;

        private void SetMediaTypes(IEnumerable<string> mediaTypes)
        {
            foreach (var mediaType in mediaTypes)
            {
                if (MediaTypeHeaderValue.TryParse(mediaType, out var parsedMediaType))
                {
                    _mediaTypes.Add(parsedMediaType);
                }
                else
                {
                    throw new ArgumentException(nameof(mediaType));
                }
            }
        }
    }
}