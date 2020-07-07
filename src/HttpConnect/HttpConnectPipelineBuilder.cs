using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace HttpConnect
{
    public class HttpConnectPipelineBuilder
    {
        private readonly IList<Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate>> _components 
            = new List<Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate>>();

        public HttpConnectPipelineBuilder Use(Func<HttpConnectRequestDelegate, HttpConnectRequestDelegate> middleware)
        {
            _components.Add(middleware);
            return this;
        }

        public HttpConnectRequestDelegate Build()
        {
            HttpConnectRequestDelegate pipeline = ctx =>
            {
                ctx.Response = new HttpConnectResponse(HttpStatusCode.NotFound);
                return Task.CompletedTask;
            };

            foreach (var component in _components.Reverse())
            {
                pipeline = component(pipeline);
            }

            return pipeline;
        }
    }
}
