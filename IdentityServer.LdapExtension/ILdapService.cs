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
        /// Logins using the specified credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>Returns the logged in user.</returns>
        TUser Login(string username, string password, string domain);

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <returns>Returns the user when it exists.</returns>
        TUser FindUser(string username);

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>Returns the user when it exists.</returns>
        TUser FindUser(string username, string domain);
    }
}
