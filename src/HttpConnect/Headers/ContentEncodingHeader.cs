namespace HttpConnect.Headers
{
    public class ContentEncodingHeader : HttpConnectHeader
    {
        public ContentEncodingHeader(string encoding)
        {
            if (string.IsNullOrWhiteSpace(encoding))
            {
                throw new System.ArgumentException("encoding cannot be null, empty or whitespace");
            }

            if (encoding != ContentEncodings.GZip)
            {
                throw new System.ArgumentException($"{encoding} not supported. Only gzip currently supported by HttpConnect", encoding);
            }

            Name = KnownHeaders.ContentEncoding;
            Value = ContentEncodings.GZip;
        }
    }
}