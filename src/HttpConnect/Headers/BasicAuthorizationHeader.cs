using System;
using System.Text;

namespace HttpConnect.Headers
{
    public class BasicAuthorizationHeader : AuthorizationHeader
    {
        private static readonly string s_basicAuthScheme = "Basic";

        public BasicAuthorizationHeader(string username, string password)
            : base(s_basicAuthScheme, CreateBasicToken(username, password))
        {
        }

        private static string CreateBasicToken(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("'username' cannot be null, empty or whitespace", nameof(username));
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                throw new ArgumentException("'password' cannot be null, empty or whitespace", nameof(password));
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
        }
    }
}
