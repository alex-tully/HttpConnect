namespace HttpConnect.Content
{
    public abstract class HttpConnectRequestContent : HttpConnectContent
    {
        public abstract object Content { get; }

        public abstract string Serialize();
    }
}
