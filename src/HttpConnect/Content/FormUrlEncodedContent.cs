using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HttpConnect.Headers;

namespace HttpConnect.Content
{
    public class FormUrlEncodedContent : HttpConnectRequestContent
    {
        // if we want to make this additive then switch to a list
        // do not use a dictionary as you can repeat parameters
        private readonly IEnumerable<KeyValuePair<string, string>> _content;

        public FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> content)
        {
            Headers.ContentType = new ContentTypeHeader(MediaTypes.FormUrlEncoded);
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public override object Content => _content;

        public override string Serialize()
        {
            if (!_content.Any())
                return string.Empty;

            // Encode and concatenate data
            StringBuilder sb = new StringBuilder();
            foreach (KeyValuePair<string, string> kvp in _content)
            {
                if (sb.Length > 0)
                {
                    sb.Append('&');
                }

                sb.Append(Encode(kvp.Key));
                sb.Append('=');
                sb.Append(Encode(kvp.Value));
            }

            return sb.ToString();
        }

        // https://source.dot.net/#System.Net.Http/System/Net/Http/FormUrlEncodedContent.cs,2fc6e070df4aa9c3
        private static string Encode(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                return string.Empty;
            }

            // Escape spaces as '+'.
            return Uri.EscapeDataString(data).Replace("%20", "+");
        }
    }
}
