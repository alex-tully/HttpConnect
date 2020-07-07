using System;
using System.Collections;
using System.Collections.Generic;

namespace HttpConnect.Headers
{
    public class HttpConnectContentHeaders : HttpConnectHeaders
    {
        public ContentTypeHeader ContentType { get; set; }
        public ContentEncodingHeader ContentEncoding { get; set; }
    }
}
