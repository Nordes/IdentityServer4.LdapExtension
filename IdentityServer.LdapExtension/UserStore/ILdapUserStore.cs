using System.Collections.Generic;
using System.Security.Claims;
using IdentityServer.LdapExtension.UserModel;

namespace IdentityServer.LdapExtension.UserStore
{
    public interface ILdapUserStore
    {
        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// Returns the application user that match that account if the 
        /// authentication is successful.
        /// </returns>
        IAppUser ValidateCredentials(string username, string password);

        /// <summary>
        /// Validates the credentials by domain.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>
        /// Returns the application user that match that account if the 
        /// authentication is successful.
        /// </returns>
        IAppUser ValidateCredentials(string username, string password, string domain);

        /// <summary>
        /// Finds the by subject identifier.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns>Returns the application user that match that unique subject id.</returns>
        IAppUser FindBySubjectId(string subjectId);

        /// <summary>
        /// Finds by username.
        /// </summary>
        /// <param name="username">The username that we are want to find.</param>
        /// <returns>Returns the application user that match the requested username.</returns>
        IAppUser FindByUsername(string username);

        /// <summary>
        /// Finds the by external provider.
        /// </summary>
        /// <param name="provider">The OpenId/specific provider.</param>
        /// <param name="userId">The user identifier to search within the specified provider.</param>
        /// <returns>Returns the application user that match the requested username and provider.</returns>
        IAppUser FindByExternalProvider(string provider, string userId);

        /// <summary>
        /// Provisions users automatically. By example if login using Google, we want to handle how
        /// we will add it in our LDAP extension. You could add the user to your own LDAP or add it
        /// to a different store (Redis, InMemory, SQL, ...).
        /// </summary>
        /// <param name="provider">The provider that require to provision in the system.</param>
        /// <param name="userId">The user identifier from that provider.</param>
        /// <param name="claims">The claims related to that provider.</param>
        /// <returns>Returns the application users created.</returns>
        IAppUser AutoProvisionUser(string provider, string userId, List<Claim> claims);
    }
}
