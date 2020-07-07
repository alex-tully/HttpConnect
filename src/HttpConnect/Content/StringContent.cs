using System;
using HttpConnect.Headers;

namespace HttpConnect.Content
{
    public class StringContent : HttpConnectRequestContent
    {
        public StringContent(string content, string mediaType)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new ArgumentException("'content' cannot be null empty or whitespace", nameof(content));
            }

            if (string.IsNullOrWhiteSpace(mediaType))
            {
                throw new ArgumentException("'mediaType' cannot be null empty or whitespace", nameof(mediaType));
            }

            Headers.ContentType = new ContentTypeHeader(mediaType);
            Content = content;
        }

        public override object Content { get; }

        public override string Serialize() => (string)Content;
    }
}
