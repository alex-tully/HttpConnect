using System;
using HttpConnect.Headers;
using Newtonsoft.Json;

namespace HttpConnect.Content
{
    public class JsonContent : HttpConnectRequestContent
    {
        public JsonContent(object content)
        {
            Headers.ContentType = new ContentTypeHeader(MediaTypes.ApplicationJson);
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override object Content { get; }

        public override string Serialize()
        {
            return JsonConvert.SerializeObject(Content);
        }
    }
}
