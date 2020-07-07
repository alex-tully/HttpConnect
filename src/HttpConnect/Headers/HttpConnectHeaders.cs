using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpConnect.Headers
{
    public class HttpConnectHeaders : IEnumerable<HttpConnectHeader>
    {
        private readonly IDictionary<string, HttpConnectHeader> _headerStore = new Dictionary<string, HttpConnectHeader>();

        public void Add(string name, string value)
        {
            SetHeader(new HttpConnectHeader(name, value));
        }

        public bool TryGetValue(string name, out string value)
        {
            value = default;

            if (_headerStore.TryGetValue(name, out HttpConnectHeader header))
            {
                value = header.Value;
                return true;
            }

            return false;
        }

        protected void SetHeader(HttpConnectHeader header)
        {
            if (header == null)
            {
                throw new ArgumentNullException(nameof(header));
            }

            _headerStore[header.Name] = header;
        }

        protected HttpConnectHeader GetHeader(string name)
        {
            if (_headerStore.ContainsKey(name))
                return _headerStore[name];

            return null;
        }

        public IEnumerator<HttpConnectHeader> GetEnumerator()
        {
            return _headerStore.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _headerStore.Values.GetEnumerator();
        }
    }
}
