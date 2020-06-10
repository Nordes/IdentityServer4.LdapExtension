using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Linq;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// This is an implementation of the service that is used to contact Ldap.
    /// </summary>
    public class LdapService<TUser> : ILdapService<TUser>
        where TUser : IAppUser, new()
    {
        private readonly ILogger<LdapService<TUser>> _logger;
        private readonly LdapConfig[] _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapService{TUser}"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public LdapService(ExtensionConfig config, ILogger<LdapService<TUser>> logger)
        {
            _logger = logger;
            _config = config.Connections.ToArray();
        }

        /// <summary>
        /// Logins using the specified credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// Returns the logged in user.
        /// </returns>
        /// <exception cref="LoginFailedException">Login failed.</exception>
        public TUser Login(string username, string password)
        {
            return Login(username, password, null);
        }

        /// <summary>
        /// Logins using the specified credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>
        /// Returns the logged in user.
        /// </returns>
        /// <exception cref="LoginFailedException">Login failed.</exception>
        public TUser Login(string username, string password, string domain)
        {
            var searchResult = SearchUser(username, domain);

            if (searchResult.Results.HasMore())
            {
                try
                {
                    var user = searchResult.Results.Next();
                    if (user != null)
                    {
                        searchResult.LdapConnection.Bind(user.Dn, password);
                        if (searchResult.LdapConnection.Bound)
                        {
                            //could change to ldap or change to configurable option
                            var provider = !string.IsNullOrEmpty(domain) ? domain : "local";
                            var appUser = new TUser();
                            appUser.SetBaseDetails(user, provider); // Should we change to LDAP.
                            searchResult.LdapConnection.Disconnect();

                            return appUser;
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogTrace(e.Message);
                    _logger.LogTrace(e.StackTrace);
                    throw new LoginFailedException("Login failed.", e);
                }
            }

            searchResult.LdapConnection.Disconnect();

            return default(TUser);
        }

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>
        /// Returns the user when it exists.
        /// </returns>
        public TUser FindUser(string username)
        {
            return FindUser(username, null);
        }

        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>
        /// Returns the user when it exists.
        /// </returns>
        public TUser FindUser(string username, string domain)
        {
            var searchResult = SearchUser(username, domain);

            try
            {
                var user = searchResult.Results.Next();
                if (user != null)
                {
                    //could change to ldap or change to configurable option
                    var provider = !string.IsNullOrEmpty(domain) ? domain : "local";
                    var appUser = new TUser();
                    appUser.SetBaseDetails(user, provider);

                    searchResult.LdapConnection.Disconnect();

                    return appUser;
                }
            }
            catch (Exception e)
            {
                _logger.LogTrace(default(EventId), e, e.Message);
                // Swallow the exception since we don't expect an error from this method.
            }

            searchResult.LdapConnection.Disconnect();

            return default(TUser);
        }

        private (LdapSearchResults Results, LdapConnection LdapConnection) SearchUser(string username, string domain)
        {
            var allSearcheable = _config.Where(f => f.IsConcerned(username)).ToList();
            if (!string.IsNullOrEmpty(domain))
            {
                allSearcheable = allSearcheable.Where(e => e.FriendlyName.Equals(domain)).ToList();
            }

            if (allSearcheable == null || allSearcheable.Count() == 0)
            {
                throw new LoginFailedException(
                    "Login failed.",
                    new NoLdapSearchableException("No searchable LDAP"));
            }

            // Could become async
            foreach (var matchConfig in allSearcheable)
            {
                using(var ldapConnection = new LdapConnection {
                    SecureSocketLayer = matchConfig.Ssl
                })
                {
                    ldapConnection.Connect(matchConfig.Url, matchConfig.FinalLdapConnectionPort);
                    ldapConnection.Bind(matchConfig.BindDn, matchConfig.BindCredentials);
                    var attributes = new TUser().LdapAttributes;
                    var searchFilter = string.Format(matchConfig.SearchFilter, username);
                    var result = ldapConnection.Search(
                        matchConfig.SearchBase,
                        LdapConnection.ScopeSub,
                        searchFilter,
                        attributes,
                        false
                    );

                    if (result.HasMore()) // Count is async (not waiting). The hasMore() always works.
                    {
                        return (Results: result as LdapSearchResults, LdapConnection: ldapConnection);
                    }
                }
            }

            throw new LoginFailedException(
                    "Login failed.",
                    new UserNotFoundException("User not found in any LDAP."));
        }
    }
}
