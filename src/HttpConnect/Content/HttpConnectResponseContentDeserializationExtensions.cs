using System.Collections.Generic;
using HttpConnect.Headers;
using Newtonsoft.Json;

namespace HttpConnect.Content
{
    internal static class HttpConnectResponseContentDeserializationExtensions
    {
        private static IReadOnlyDictionary<string, IContentDeserializer> contentTypeDeserializerMap
            = new Dictionary<string, IContentDeserializer>
            {
                [MediaTypes.ApplicationJson] = new JsonDeserializer(),
                [MediaTypes.TextJson] = new JsonDeserializer(),
                [MediaTypes.TextXJson] = new JsonDeserializer(),
                [MediaTypes.TextJavascript] = new JsonDeserializer()
            };

        public static T Deserialize<T>(this HttpConnectResponseContent content)
        {
            string contentType = content.Headers.ContentType.Value;

            int semicolonIndex = contentType.IndexOf(';');

            if (semicolonIndex > -1)
                contentType = contentType.Substring(0, semicolonIndex);

            if (contentTypeDeserializerMap.ContainsKey(contentType))
                return contentTypeDeserializerMap[contentType].Deserialize<T>(content.Content);

            return default;
        }

        private interface IContentDeserializer
        {
            T Deserialize<T>(string content);
        }

        private class JsonDeserializer : IContentDeserializer
        {
            public T Deserialize<T>(string content)
            {
                return JsonConvert.DeserializeObject<T>(content);
            }
        }
    }
}
