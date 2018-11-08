using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using Microsoft.Extensions.Logging;
using Novell.Directory.Ldap;
using System;
using System.Collections.Generic;
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
        private readonly ICollection<LdapConfig> _config;
        private readonly Dictionary<string, LdapConnection> _ldapConnections = new Dictionary<string, LdapConnection>();

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapService{TUser}"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public LdapService(ExtensionConfig config, ILogger<LdapService<TUser>> logger)
        {
            _logger = logger;
            _config = config.Connections;

            _config.ToList().ForEach(f => _ldapConnections.Add(f.FriendlyName, new LdapConnection
            {
                SecureSocketLayer = f.Ssl
            }));
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
            var searchResult = SearchUser(username);

            if (searchResult.Results.hasMore())
            {
                try
                {
                    var user = searchResult.Results.next();
                    if (user != null)
                    {
                        searchResult.LdapConnection.Bind(user.DN, password);
                        if (searchResult.LdapConnection.Bound)
                        {
                            var appUser = new TUser();
                            appUser.SetBaseDetails(user, "local"); // Should we change to LDAP.
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
        /// <returns>
        /// Returns the user when it exists.
        /// </returns>
        public TUser FindUser(string username)
        {
            var searchResult = SearchUser(username);

            try
            {
                var user = searchResult.Results.next();
                if (user != null)
                {
                    var appUser = new TUser();
                    appUser.SetBaseDetails(user, "local");

                    searchResult.LdapConnection.Disconnect();

                    return appUser;
                }
            }
            catch (Exception e)
            {
                _logger.LogTrace(e.Message);
                _logger.LogTrace(e.StackTrace);
                // Swallow the exception since we don't expect an error from this method.
            }

            searchResult.LdapConnection.Disconnect();

            return default(TUser);
        }

        private (LdapSearchResults Results, LdapConnection LdapConnection) SearchUser(string username)
        {
            var allSearcheable = _config.Where(f => f.IsConcerned(username)).ToList();

            if (allSearcheable == null || allSearcheable.Count() == 0)
            {
                throw new LoginFailedException(
                    "Login failed.",
                    new NoLdapSearchableException("No searchable LDAP"));
            }

            // Could become async
            foreach (var matchConfig in allSearcheable)
            {
                var ldapConnection = _ldapConnections[matchConfig.FriendlyName];

                ldapConnection.Connect(matchConfig.Url, matchConfig.FinalLdapConnectionPort);
                ldapConnection.Bind(matchConfig.BindDn, matchConfig.BindCredentials);
                var attributes = new TUser().LdapAttributes;
                var searchFilter = string.Format(matchConfig.SearchFilter, username);
                var result = ldapConnection.Search(
                    matchConfig.SearchBase,
                    LdapConnection.SCOPE_SUB,
                    searchFilter,
                    attributes,
                    false
                );

                if (result.hasMore()) // Count is async (not waiting). The hasMore() always works.
                {
                    return (Results: result, LdapConnection: ldapConnection);
                }
            }

            throw new LoginFailedException(
                    "Login failed.",
                    new UserNotFoundException("User not found in any LDAP."));
        }
    }
}
