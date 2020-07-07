using System;

namespace HttpConnect.Headers
{
    public class AuthorizationHeader : HttpConnectHeader
    {
        public AuthorizationHeader(string scheme)
            : this()
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException("Scheme cannot be null, empty or whitespace", nameof(scheme));
            }

            Value = scheme;
        }

        public AuthorizationHeader(string scheme, string parameter)
            : this()
        {
            if (string.IsNullOrWhiteSpace(scheme))
            {
                throw new ArgumentException("Scheme cannot be null, empty or whitespace", nameof(scheme));
            }

            if (string.IsNullOrWhiteSpace(parameter))
            {
                throw new ArgumentException("parameter cannot be null, empty or whitespace", nameof(parameter));
            }
            
            Value = $"{scheme} {parameter}";
        }

        protected AuthorizationHeader()
        {
            Name = KnownHeaders.Authorization;
        }
    }
}
