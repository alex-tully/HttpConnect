namespace HttpConnect.Headers
{
    public class ContentTypeHeader : HttpConnectHeader
    {
        public ContentTypeHeader(string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                throw new System.ArgumentException("mediaType cannot be null, empty or whitespace", nameof(mediaType));
            }

            Name = KnownHeaders.ContentType;
            Value = mediaType;
        }
    }
}
