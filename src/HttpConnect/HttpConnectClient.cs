using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using HttpConnect.Content;
using HttpConnect.Middleware.HttpClient;

namespace HttpConnect
{
    public class HttpConnectClient : IHttpConnectClient
    {
        private readonly Uri _baseUri = null;
        private readonly HttpConnectRequestDelegate _pipeline;

        // empty -> defaults to http client
        public HttpConnectClient()
            : this(null, null)
        {
        }

        public HttpConnectClient(Action<HttpConnectPipelineBuilder> builderAction)
            : this(null, builderAction)
        {
        }

        // empty -> defaults to http client
        public HttpConnectClient(Uri baseUri)
            : this(baseUri, null)
        {
        }

        public HttpConnectClient(Uri baseUri, Action<HttpConnectPipelineBuilder> builderAction)
        {
            if (baseUri != null && !baseUri.IsAbsoluteUri)
            {
                throw new ArgumentException("When supplied base uri must be absolute");
            }

            _baseUri = baseUri;
            _pipeline = ConfigurePipeline(builderAction);
        }

        private static HttpConnectRequestDelegate ConfigurePipeline(Action<HttpConnectPipelineBuilder> builderAction = null)
        {
            // if this comes through as null then set it to default
            builderAction = builderAction ?? ((builder) => builder.UseHttpClient());

            HttpConnectPipelineBuilder pipelineBuilder = new HttpConnectPipelineBuilder();
            builderAction.Invoke(pipelineBuilder);
            return pipelineBuilder.Build();
        }

        public Task<HttpConnectResponse> GetAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            return SendAsync(new HttpConnectRequest(HttpMethod.Get, requestUri), cancellationToken);
        }

        public Task<HttpConnectResponse<T>> GetAsync<T>(Uri requestUri, CancellationToken cancellationToken)
        {
            return SendAsync<T>(new HttpConnectRequest(HttpMethod.Get, requestUri), cancellationToken);
        }

        public async Task<HttpConnectResponse> SendAsync(HttpConnectRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            cancellationToken.ThrowIfCancellationRequested();
            PrepareRequest(request);

            HttpConnectContext context = new HttpConnectContext(request, cancellationToken);

            try
            {
                await _pipeline.Invoke(context).ConfigureAwait(false);
                return context.Response;
            }
            catch (Exception ex)
            {
                // if no response value the create one
                if (context.Response == null) 
                {
                    context.Response = new HttpConnectResponse();
                }

                context.Response.SetError(ex);
            }

            return context.Response;
        }

        public async Task<HttpConnectResponse<T>> SendAsync<T>(HttpConnectRequest request, CancellationToken cancellationToken)
        {
            HttpConnectResponse response = await SendAsync(request, cancellationToken);

            try
            {
                if (response.IsSuccess)
                {
                    var deserializedResponse = new HttpConnectResponse<T>(response);
                    deserializedResponse.Data = deserializedResponse.Content.Deserialize<T>();
                    return deserializedResponse;
                }

                return new HttpConnectResponse<T>(response);
            }
            catch (Exception exception)
            {
                HttpConnectResponse<T> errorResponse = new HttpConnectResponse<T>(response);
                errorResponse.SetError(exception);
                return errorResponse;
            }
        }

        private void PrepareRequest(HttpConnectRequest request)
        {
            if (_baseUri == null && !request.RequestUri.IsAbsoluteUri)
                throw new InvalidOperationException("An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set.");

            if (!request.RequestUri.IsAbsoluteUri)
            {
                request.RequestUri = new Uri(_baseUri, request.RequestUri);
            }
        }
    }
}
