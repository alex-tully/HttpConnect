using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpConnect.Middleware.HttpClient
{
    internal sealed class GZipContent : HttpContent
    {
        private readonly HttpContent _httpContent;

        public GZipContent(HttpContent content)
        {
            if (content == null)
            {
                throw new ArgumentException("'content' cannot be null", nameof(content));
            }

            _httpContent = content;

            foreach (KeyValuePair<string, IEnumerable<string>> header in content.Headers)
                Headers.TryAddWithoutValidation(header.Key, header.Value);

            Headers.ContentEncoding.Add("gzip");
        }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            using (GZipStream gzip = new GZipStream(stream, CompressionMode.Compress, true))
            {
                await _httpContent.CopyToAsync(gzip);
            }
        }

        // Implementing abstract classs of HttpContent, returning false as data is a stream
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }
    }
}