namespace HttpConnect.Headers
{
    public class AcceptHeader : HttpConnectHeader
    {
        public AcceptHeader(string mediaType)
        {
            if (string.IsNullOrWhiteSpace(mediaType))
            {
                throw new System.ArgumentException("mediaType cannot be null, empty or whitespace", nameof(mediaType));
            }

            Name = KnownHeaders.Accept;
            Value = mediaType;
        }
    }
}
