using System;

namespace IdentityServer.LdapExtension.Exceptions
{
    internal class LoginFailedException : Exception
    {
        public LoginFailedException(string message)
            : base(message)
        {
        }

        public LoginFailedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
