using System.Collections.Generic;
using IdentityModel;
using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.LdapExtension.UserStore
{
    public class InMemoryUserStore<TUser> : ILdapUserStore
        where TUser : IAppUser, new()
    {
        private readonly ILdapService<TUser> _authenticationService;
        private readonly Dictionary<string, Dictionary<string, TUser>> _users = new Dictionary<string, Dictionary<string, TUser>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryUserStore{TUser}"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        public InMemoryUserStore(ILdapService<TUser> authenticationService)
        {
            _authenticationService = authenticationService;
        }

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns>
        /// Returns the application user that match that account if the
        /// authentication is successful.
        /// </returns>
        public IAppUser ValidateCredentials(string username, string password)
        {
            try
            {
                var user = _authenticationService.Login(username, password);
                if (user != null)
                {
                    return user;
                }
            }
            catch (LoginFailedException)
            {
                return default(TUser);
            }

            return default(TUser);
        }

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns>
        /// Returns the application user that match that account if the
        /// authentication is successful.
        /// </returns>
        public IAppUser ValidateCredentials(string username, string password, string domain)
        {
            try
            {
                var user = _authenticationService.Login(username, password, domain);
                if (user != null)
                {
                    return user;
                }
            }
            catch (LoginFailedException)
            {
                return default(TUser);
            }

            return default(TUser);
        }

        /// <summary>
        /// Finds the user by subject identifier, but does not add the user to the cache
        /// since he's not logged in, in the current context.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns>The application user.</returns>
        public IAppUser FindBySubjectId(string subjectId)
        {
            // Search in external provider first
            var foundAnything = _users.Select(f => f.Value.FirstOrDefault(g => g.Value.SubjectId == subjectId)).FirstOrDefault();
            if (foundAnything.Value != null)
            {
                return foundAnything.Value;
            }

            // Search in the LDAP
            return _authenticationService.FindUser(subjectId.Replace("ldap_", ""));
        }

        /// <summary>
        /// Finds by username.
        /// </summary>
        /// <param name="username">The username that we are want to find.</param>
        /// <returns>
        /// Returns the application user that match the requested username.
        /// </returns>
        public IAppUser FindByUsername(string username)
        {
            // Check the external data provider
            var foundAnything = _users.Select(f => f.Value.FirstOrDefault(g => g.Value.Username == username)).FirstOrDefault();
            if (foundAnything.Value != null)
            {
                return foundAnything.Value;
            }

            // If nothing found in external, than we look in our current LDAP system. (We want to get always the latest details when we are on the LDAP).
            return _authenticationService.FindUser(username.Replace("ldap_", ""));
        }

        /// <summary>
        /// Finds the by external provider.
        /// </summary>
        /// <param name="provider">The OpenId/specific provider.</param>
        /// <param name="userId">The user identifier to search within the specified provider.</param>
        /// <returns>
        /// Returns the application user that match the requested username and provider.
        /// </returns>
        public IAppUser FindByExternalProvider(string provider, string userId)
        {
            if (_users.TryGetValue(provider, out var providerUsers))
            {
                if (providerUsers.TryGetValue(userId, out var foundUser))
                {
                    return foundUser;
                }
            }

            return default(TUser);
        }

        /// <summary>
        /// Provisions users automatically. By example if login using Google, we want to handle how
        /// we will add it in our LDAP extension. You could add the user to your own LDAP or add it
        /// to a different store (Redis, InMemory, SQL, ...).
        /// </summary>
        /// <param name="provider">The provider that require to provision in the system.</param>
        /// <param name="userId">The user identifier from that provider.</param>
        /// <param name="claims">The claims related to that provider.</param>
        /// <returns>
        /// Returns the application users created.
        /// </returns>
        public IAppUser AutoProvisionUser(string provider, string userId, List<Claim> claims)
        {
            // create a list of claims that we want to transfer into our store
            var filtered = new List<Claim>();

            foreach (var claim in claims)
            {
                // if the external system sends a display name - translate that to the standard OIDC name claim
                if (claim.Type == ClaimTypes.Name)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, claim.Value));
                }
                // if the JWT handler has an outbound mapping to an OIDC claim use that
                else if (JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.ContainsKey(claim.Type))
                {
                    filtered.Add(new Claim(JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap[claim.Type], claim.Value));
                }
                // copy the claim as-is
                else
                {
                    filtered.Add(claim);
                }
            }

            // if no display name was provided, try to construct by first and/or last name
            if (!filtered.Any(x => x.Type == JwtClaimTypes.Name))
            {
                var first = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.GivenName)?.Value;
                var last = filtered.FirstOrDefault(x => x.Type == JwtClaimTypes.FamilyName)?.Value;
                if (first != null && last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first + " " + last));
                }
                else if (first != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, first));
                }
                else if (last != null)
                {
                    filtered.Add(new Claim(JwtClaimTypes.Name, last));
                }
            }

            // create a new unique subject id
            var sub = CryptoRandom.CreateUniqueId();

            // check if a display name is available, otherwise fallback to subject id
            var name = filtered.FirstOrDefault(c => c.Type == JwtClaimTypes.Name)?.Value ?? sub;

            // create new user
            var user = new TUser
            {
                SubjectId = sub,
                Username = name,
                ProviderName = provider,
                ProviderSubjectId = userId,
                Claims = filtered
            };

            // add user to in-memory store
            if (!_users.TryGetValue(provider, out var providerData))
            {
                providerData = new Dictionary<string, TUser>();
                _users.Add(provider, providerData);
            }

            if (!providerData.ContainsKey(user.SubjectId))
            {
                providerData.Add(user.ProviderSubjectId, user);
            }
            else
            {
                // replace the user? (Good idea, I don't know, but why not).
                providerData[user.SubjectId] = user;
            }

            return user;
        }
    }
}
