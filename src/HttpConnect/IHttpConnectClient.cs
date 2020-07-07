using System;
using System.Threading;
using System.Threading.Tasks;

namespace HttpConnect
{
    public interface IHttpConnectClient
    {
        Task<HttpConnectResponse> GetAsync(Uri requestUri, CancellationToken cancellationToken);

        Task<HttpConnectResponse<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken);

        Task<HttpConnectResponse> SendAsync(HttpConnectRequest request, CancellationToken cancellationToken);

        Task<HttpConnectResponse<T>> SendAsync<T>(HttpConnectRequest request, CancellationToken cancellationToken);
    }
}
