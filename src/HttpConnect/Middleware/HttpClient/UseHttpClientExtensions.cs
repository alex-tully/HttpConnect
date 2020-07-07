namespace HttpConnect.Middleware.HttpClient
{
    using HttpClient = System.Net.Http.HttpClient;

    public static class UseHttpClientExtensions
    {
        private static readonly HttpClient s_httpClient = new HttpClient();

        public static HttpConnectPipelineBuilder UseHttpClient(this HttpConnectPipelineBuilder builder)
        {
            return builder.UseHttpClient(s_httpClient);
        }

        public static HttpConnectPipelineBuilder UseHttpClient(
            this HttpConnectPipelineBuilder builder, 
            HttpClient httpClient)
        {
            HttpClientMiddleware httpClientMiddleware = new HttpClientMiddleware(httpClient);

            builder.Use((next) => (ctx) => httpClientMiddleware.Invoke(ctx));

            return builder;
        }
    }
}
