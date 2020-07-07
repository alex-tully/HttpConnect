using System.Collections.Generic;
using System.Threading;

namespace HttpConnect
{
    public class HttpConnectContext
    {
        public HttpConnectContext(HttpConnectRequest request, CancellationToken requestAborted = default)
        {
            Request = request ?? throw new System.ArgumentNullException(nameof(request));
            RequestAborted = requestAborted;
        }

        public HttpConnectContext()
        {
            // TODO ???? SHOULD I DO THIS?! NO! NEED TO REMOVE
        }

        public HttpConnectRequest Request { get; }

        public HttpConnectResponse Response { get; set; }

        public CancellationToken RequestAborted { get; set; }

        public IDictionary<string, object> Items { get; } = new Dictionary<string, object>();
    }
}
