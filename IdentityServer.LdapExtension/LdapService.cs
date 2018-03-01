using System;
using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// This is the service that is used to contact the Ldap.
    /// </summary>
    public class LdapService<TUser> : ILdapService<TUser>
        where TUser: IAppUser, new()
    {
        private readonly ILogger<LdapService<TUser>> _logger;
        private readonly LdapConfig _config;
        private readonly LdapConnection _ldapConnection;

        public LdapService(IOptions<LdapConfig> config, ILogger<LdapService<TUser>> logger)
        {
            _logger = logger;
            _config = config.Value;

            _ldapConnection = new LdapConnection
            {
                SecureSocketLayer = false
            };
        }

        /// <summary>
        /// Attempt to login through Ldap.
        /// </summary>
        /// <param name="username">Ldap username</param>
        /// <param name="password">Ldap password</param>
        /// <returns>User details.</returns>
        public TUser Login(string username, string password)
        {
            var searchResult = SearchUser(username);
            if (searchResult.hasMore()) {
                try
                {
                    var user = searchResult.next();
                    if (user != null)
                    {
                        _ldapConnection.Bind(user.DN, password);
                        if (_ldapConnection.Bound)
                        {
                            // Here it cause some kind of issue.
                            var appUser = new TUser();
                            appUser.SetBaseDetails(user, "local"); // Could be also ldap.
                            _ldapConnection.Disconnect();

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

            _ldapConnection.Disconnect();

            return default(TUser);
        }

        public TUser FindUser(string username)
        {
            var searchResult = SearchUser(username);

            try
            {
                var user = searchResult.next();
                if (user != null)
                {
                    var appUser = new TUser();
                    appUser.SetBaseDetails(user, "local");

                    _ldapConnection.Disconnect();

                    return appUser;
                }
            }
            catch (Exception e)
            {
                _logger.LogTrace(e.Message);
                _logger.LogTrace(e.StackTrace);
                // Swallow the exception since we don't expect an error from this method.
            }

            _ldapConnection.Disconnect();

            return default(TUser);
        }

        private LdapSearchResults SearchUser(string username)
        {
            var port = _config.Port == 0 ? _config.Ssl ? LdapConnection.DEFAULT_SSL_PORT : LdapConnection.DEFAULT_PORT : _config.Port;

            _ldapConnection.Connect(_config.Url, port);
            _ldapConnection.Bind(_config.BindDn, _config.BindCredentials);
            var attributes = (new TUser()).LdapAttributes; // TODO change this code, this is totally a hack. It's bad coding!
            var searchFilter = string.Format(_config.SearchFilter, username);
            var result = _ldapConnection.Search(
                _config.SearchBase,
                LdapConnection.SCOPE_SUB,
                searchFilter,
                attributes,
                false
            );

            return result;
        }
    }
}
