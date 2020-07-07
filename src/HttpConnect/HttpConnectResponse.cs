using System;
using System.Net;
using HttpConnect.Content;
using HttpConnect.Headers;

namespace HttpConnect
{
    public class HttpConnectResponse
    {
        public HttpConnectResponse()
        {
        }

        public HttpConnectResponse(HttpStatusCode statusCode)
        {
            StatusCode = statusCode;
            Status = HttpConnectResponseStatus.Completed;
        }

        public HttpConnectResponse(Exception exception)
        {
            SetError(exception);
        }

        public HttpConnectResponse(HttpConnectResponse response)
        {
            StatusCode = response.StatusCode;
            Headers = response.Headers;
            Request = response.Request;
            Content = response.Content;
            Status = response.Status;
            Exception = response.Exception;
        }

        public HttpStatusCode StatusCode { get; set; }

        public bool IsSuccess => (int)StatusCode >= 200 && 
                                 (int)StatusCode <= 299 &&
                                 Status == HttpConnectResponseStatus.Completed;

        public HttpConnectResponseHeaders Headers { get; set; } = new HttpConnectResponseHeaders();

        public HttpConnectRequest Request { get; set; }

        public HttpConnectResponseContent Content { get; set; }

        public HttpConnectResponseStatus Status { get; set; }

        public Exception Exception { get; set; }

        internal void SetError(Exception exception)
        {
            Exception = exception ?? throw new ArgumentNullException(nameof(exception));
            Status = HttpConnectResponseStatus.Error;
        }
    }

    public class HttpConnectResponse<T> : HttpConnectResponse
    {
        public HttpConnectResponse(HttpStatusCode statusCode)
            : base(statusCode)
        {
        }

        public HttpConnectResponse(HttpConnectResponse response)
            : base(response)
        {
        }


        /// <summary>
        /// Deserialized content response
        /// </summary>
        public T Data { get; set; } = default;
    }
}
