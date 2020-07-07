using System;

namespace HttpConnect.Headers
{
    public class HttpConnectHeader
    {
        public HttpConnectHeader(string name, string value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("'name' cannot be null, empty or whitespace", nameof(name));
            }

            Name = name;
            Value = value ?? string.Empty; // allowing this as we don't parse values, so it could be invalid!
        }

        protected HttpConnectHeader()
        {
        }

        public string Name { get; protected set; }

        public string Value { get; protected set; }
    }
}
