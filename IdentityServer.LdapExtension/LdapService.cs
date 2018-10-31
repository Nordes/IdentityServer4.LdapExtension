using System;
using System.Linq;
using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Novell.Directory.Ldap;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// This is an implementation of the service that is used to contact Ldap.
    /// </summary>
    public class LdapService<TUser> : ILdapService<TUser>
        where TUser : IAppUser, new()
    {
        private readonly ILogger<LdapService<TUser>> _logger;
        private readonly LdapConfig _config;
        private readonly LdapConnection _ldapConnection;

        /// <summary>
        /// Initializes a new instance of the <see cref="LdapService{TUser}"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="logger">The logger.</param>
        public LdapService(IOptions<LdapConfig> config, ILogger<LdapService<TUser>> logger)
        {
            _logger = logger;
            _config = config.Value;
            _ldapConnection = new LdapConnection();

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

            if (searchResult.hasMore())
            {
                try
                {
                    var user = searchResult.next();
                    if (user != null)
                    {
                        _ldapConnection.Bind(user.DN, password);
                        if (_ldapConnection.Bound)
                        {
                            var appUser = new TUser();
                            appUser.SetBaseDetails(user, "local"); // Should we change to LDAP.
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
        /// <summary>
        /// Logins using the specified credentials.
        /// </summary>
        /// <param name="domain">The domain name to authenticate against</param>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// Returns the logged in user.
        /// </returns>
        /// <exception cref="LoginFailedException">Login failed.</exception>
        public TUser Login(string domain, string username, string password)
        {
            var searchResult = SearchUser(domain, username);

            if (searchResult.hasMore())
            {
                try
                {
                    var user = searchResult.next();
                    if (user != null)
                    {
                        _ldapConnection.Bind(user.DN, password);
                        if (_ldapConnection.Bound)
                        {
                            var appUser = new TUser();
                            appUser.SetBaseDetails(user, domain);
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
        /// <summary>
        /// Finds user by username.
        /// </summary>
        /// <param name="domain">The domain name to authenticate against</param>
        /// <param name="username">The username.</param>
        /// <returns>
        /// Returns the user when it exists.
        /// </returns>
        public TUser FindUser(string domain, string username)
        {
            
            var searchResult = SearchUser(domain, username);

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
            //grab first host
            var host = _config.Hosts.First();
            var result = SearchUser(host, username);

            return result;
        }
        private LdapSearchResults SearchUser(string domain, string username)
        {
            //grab first host
            var host = _config.Hosts.First(f => f.Name.Equals(domain));
            var result = SearchUser(host, username);

            return result;
        }
        private LdapSearchResults SearchUser(LdapHost host, string username)
        {
            _ldapConnection.SecureSocketLayer = host.Ssl;
            _ldapConnection.Connect(host.Url, host.Port);
            _ldapConnection.Bind(host.BindDn, host.BindCredentials);
            var attributes = (new TUser()).LdapAttributes;
            var searchFilter = string.Format(host.SearchFilter, username);
            var result = _ldapConnection.Search(
                host.SearchBase,
                LdapConnection.SCOPE_SUB,
                searchFilter,
                attributes,
                false
            );

            return result;
        }
    }
}
