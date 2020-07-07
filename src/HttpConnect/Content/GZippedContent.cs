using System;
using System.Collections.Generic;
using System.Linq;
using HttpConnect.Headers;
using Newtonsoft.Json;

namespace HttpConnect.Content
{
    public class GZippedContent : HttpConnectRequestContent
    {
        public GZippedContent(HttpConnectRequestContent httpConnectRequestContent)
        {
            if (httpConnectRequestContent == null)
                throw new ArgumentNullException("httpConnectRequestContent cannot be null");

            // Map the entire HttpConnectRequestContent object accross to the content of this object
            Content = httpConnectRequestContent;

            CopyHeaders(httpConnectRequestContent.Headers);
            Headers.ContentEncoding = new ContentEncodingHeader("gzip");
        }

        public override object Content { get; }

        // Serialize the Entire HttpConnectRequestContent object, JsonContent/StringContent
        public override string Serialize()
        {
            return JsonConvert.SerializeObject(Content);
        }

        private void CopyHeaders(HttpConnectHeaders headers)
        {
            foreach (var header in headers)
            {
                if (header.Name == KnownHeaders.ContentEncoding)
                    throw new ArgumentException("Content-Encoding header is already on request.", header.Name);

                Headers.Add(header.Name, header.Value);
            }
        }
    }
}