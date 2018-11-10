using IdentityModel;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer4.Stores.Serialization;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace IdentityServer.LdapExtension.UserStore
{
    /// <summary>
    /// Redis user store (persistent)
    /// </summary>
    /// <typeparam name="TUser">The type of the user.</typeparam>
    /// <remarks>
    /// Redis keys:
    ///     /IdentityServer/OpenId/providers/[Provider]/subjectId/[subjectId] &lt;== Key with data
    ///     /IdentityServer/OpenId/users/[username] &lt;== Key forwarding to the provider+subjectId
    /// </remarks>
    public class RedisUserStore<TUser> : ILdapUserStore
        where TUser : IAppUser, new()
    {
        private readonly ILdapService<TUser> _authenticationService;
        private readonly ILogger<RedisUserStore<TUser>> _logger;
        private IConnectionMultiplexer _redis;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings
        {
            Converters = new List<JsonConverter> { new ClaimConverter() },
            Formatting = Formatting.Indented
        };

        private TimeSpan _dataExpireIn;

        public RedisUserStore(
            ILdapService<TUser> authenticationService,
            ExtensionConfig ldapConfigurations,
            ILogger<RedisUserStore<TUser>> logger)
        {
            _authenticationService = authenticationService;
            _logger = logger;

            InitializeRedis(ldapConfigurations);
        }

        private void InitializeRedis(ExtensionConfig config)
        {
            if (string.IsNullOrEmpty(config.Redis))
            {
                throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "LDAP configuration is configured to use Redis, but there's no configuration. Please fix your injection in Startup.cs");
            }

            _redis = ConnectionMultiplexer.Connect(config.Redis);
            if (_redis.IsConnected)
            {
                _logger.LogDebug($"LDAP {GetType().Name}: Connected to redis \\o/");
            }
            else
            {
                _logger.LogError($"LDAP {GetType().Name}: Not able to connect to redis :(");
            }

            _dataExpireIn = TimeSpan.FromSeconds(config.RefreshClaimsInSeconds ?? (double)-1);
        }

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <returns></returns>
        public IAppUser ValidateCredentials(string username, string password)
        {
            try
            {
                var user = _authenticationService.Login(username, password);
                if (user != null)
                {
                    SetRedisData(user);
                    return user;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Login failed.")
                {
                    return default(TUser);
                }

                throw;
            }

            return default(TUser);
        }

        /// <summary>
        /// Validates the credentials.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="domain">The domain friendly name.</param>
        /// <returns></returns>
        public IAppUser ValidateCredentials(string username, string password, string domain)
        {
            try
            {
                var user = _authenticationService.Login(username, password, domain);
                if (user != null)
                {
                    SetRedisData(user);
                    return user;
                }
            }
            catch (Exception e)
            {
                if (e.Message == "Login failed.")
                {
                    return default(TUser);
                }

                throw;
            }

            return default(TUser);
        }

        /// <summary>
        /// Finds the user by subject identifier.
        /// </summary>
        /// <param name="subjectId">The subject identifier.</param>
        /// <returns></returns>
        public IAppUser FindBySubjectId(string subjectId)
        {
            // /IdentityServer/OpenId/providers/[Provider]/userId/[userId] &lt;== Key with data
            // /IdentityServer/OpenId/users/[username] &lt;== Key forwarding to the provider+subjectId
            const string keyFormat = "IdentityServer/OpenId/subjectId/{0}";
            var rdb = _redis.GetDatabase();
            var result = rdb.StringGet(string.Format(keyFormat, subjectId));

            if (result.HasValue)
            {
                // IMPORTANT! This line might throw an exception if we change the format/version
                IAppUser foundSubjectId = JsonConvert.DeserializeObject<TUser>(result.ToString(), _jsonSerializerSettings);

                return foundSubjectId;
            }

            // Search in the LDAP
            if (subjectId.Contains("ldap_"))
            {
                var found = _authenticationService.FindUser(subjectId.Replace("ldap_", "")); // As of now, subjectId is the same as the username

                if (found != null)
                {
                    SetRedisData(found);
                    return found;
                }
            }

            // Not found at all
            return null;
        }

        public IAppUser FindByUsername(string username)
        {
            const string keyFormat = "IdentityServer/OpenId/username/{0}";
            var rdb = _redis.GetDatabase();
            var result = rdb.StringGet(string.Format(keyFormat, username));

            if (result.HasValue)
            {
                string foundSubjectIdKey = result.ToString();
                var subject = rdb.StringGet(foundSubjectIdKey);

                if (subject.HasValue)
                {
                    // IMPORTANT! This line might throw an exception if we change the format/version
                    IAppUser foundSubjectId = JsonConvert.DeserializeObject<TUser>(subject.ToString(), _jsonSerializerSettings);

                    return foundSubjectId;
                }

                _logger.LogWarning($"The key {foundSubjectIdKey} should not be existing or data is corrupted!");
            }

            // If nothing found in external, than we look in our current LDAP system. (We want to get always the latest details when we are on the LDAP).
            var ldapUser = _authenticationService.FindUser(username);
            if (ldapUser != null)
            {
                SetRedisData(ldapUser);
            }

            // Not found at all
            return ldapUser;
        }

        /// <summary>
        /// Finds the user by external provider.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns></returns>
        public IAppUser FindByExternalProvider(string provider, string userId)
        {
            const string keyFormat = "IdentityServer/OpenId/provider/{0}/userId/{1}";
            var rdb = _redis.GetDatabase();
            var result = rdb.StringGet(string.Format(keyFormat, provider, userId));

            if (result.HasValue)
            {
                string foundSubjectIdKey = result.ToString();
                var subject = rdb.StringGet(foundSubjectIdKey);

                if (subject.HasValue)
                {
                    // IMPORTANT! This line might throw an exception if we change the format/version
                    IAppUser foundSubjectId = JsonConvert.DeserializeObject<TUser>(subject.ToString(), _jsonSerializerSettings);

                    return foundSubjectId;
                }

                _logger.LogWarning($"The key {foundSubjectIdKey} should not be existing or data is corrupted!");
            }

            // Nothing found... just get out and give an error.
            return default(IAppUser);
        }

        /// <summary>
        /// Automatically provisions a user.
        /// </summary>
        /// <param name="provider">The provider.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="claims">The claims.</param>
        /// <returns></returns>
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

            SetRedisData(user);

            return user;
        }

        private void SetRedisData(IAppUser user)
        {
            const string keyBySubjectId = "IdentityServer/OpenId/subjectId/{0}"; // <== contains the full data
            const string keyByUsername = "IdentityServer/OpenId/username/{0}"; // <== contains a link to the SubjectId
            const string keyByProviderAndUserid = "IdentityServer/OpenId/provider/{0}/userId/{1}"; // <== contains a link to the SubjectId

            var userStr = JsonConvert.SerializeObject(user, _jsonSerializerSettings);
            var subjectIdStorageKey = string.Format(keyBySubjectId, user.SubjectId);

            // add user to Redis store
            var rdb = _redis.GetDatabase();
            var foundUser = rdb.StringGet(string.Format(keyByProviderAndUserid, user.ProviderName, user.ProviderSubjectId));
            if (foundUser.HasValue)
            {
                _logger.LogWarning($"This data should not be already in redis. {string.Format(keyByProviderAndUserid, user.ProviderName, user.ProviderSubjectId)}");
            }

            // Add the parameter , _dataExpireIn if we want to expire the data. I don't know the impact if we do it.
            // Documentation is not clear about how this code is called. Probably it would be better to have a job running to update the claims in redis.
            rdb.StringSet(subjectIdStorageKey, userStr);
            rdb.StringSet(string.Format(keyByUsername, user.Username), subjectIdStorageKey); // Might cause issue... or hack...
            rdb.StringSet(string.Format(keyByProviderAndUserid, user.ProviderName, user.ProviderSubjectId), subjectIdStorageKey);
        }
    }
}
