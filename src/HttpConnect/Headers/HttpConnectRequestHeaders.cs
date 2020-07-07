namespace HttpConnect.Headers
{
    public class HttpConnectRequestHeaders : HttpConnectHeaders
    {
        public AuthorizationHeader Authorization
        {
            get
            {
                HttpConnectHeader header = GetHeader(KnownHeaders.Authorization);

                if (header == null)
                    return null;

                return new AuthorizationHeader(header.Value);
            }
            set => SetHeader(value);
        }

        public AcceptHeader Accept
        {
            get
            {
                HttpConnectHeader header = GetHeader(KnownHeaders.Accept);

                if (header == null)
                    return null;

                return new AcceptHeader(header.Value);
            }
            set => SetHeader(value);
        }
    }
}
