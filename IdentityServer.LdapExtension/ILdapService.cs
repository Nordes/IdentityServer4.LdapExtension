using IdentityServer.LdapExtension.UserModel;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// Maybe not mandatory, to see.
    /// </summary>
    public interface ILdapService<out TUser>
        where TUser: IAppUser, new()
    {
        /// <summary>
        /// Logins using the specified credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>Returns the logged in user.</returns>
        TUser Login(string username, string password);

        /// <summary>
        /// Logins using the specified credentials and domain name as specified in the host config
        /// </summary>
        /// <param name="domain">The name of the domain as specified in the host config</param>
        /// <param name="username">The username</param>
        /// <param name="password">The password</param>
        /// <returns>Returns the logged in user.</returns>
        TUser Login(string domain, string username, string password);

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>Returns the user when it exists.</returns>
        TUser FindUser(string username);

        /// <summary>
        /// Finds user by username and domain
        /// </summary>
        /// <param name="domain">The name of the domain as specified in the host config</param>
        /// <param name="username">The username</param>
        /// <returns>Returns the user when it exists.</returns>
        TUser FindUser(string domain, string username);
    }
}
