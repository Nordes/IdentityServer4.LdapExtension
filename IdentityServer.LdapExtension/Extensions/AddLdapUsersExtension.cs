using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Linq;

namespace IdentityServer.LdapExtension.Extensions
{
    public static class AddLdapUsersExtension
    {
        /// <summary>
        /// Adds the LDAP users mechanism to IdentityServer.
        /// </summary>
        /// <typeparam name="TUserDetails">The type of the user details.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The configuration.</param>
        /// <param name="userStore">The user store.</param>
        /// <returns>
        /// Returns the builder instance.
        /// </returns>
        public static IIdentityServerBuilder AddLdapUsers<TUserDetails>(this IIdentityServerBuilder builder, IConfiguration configuration, UserStore userStore)
            where TUserDetails : IAppUser, new()
        {
            RegisterLdapConfigurations(builder, configuration);
            builder.Services.AddSingleton<ILdapService<TUserDetails>, LdapService<TUserDetails>>();

            // For testing purpose we can use the in memory. In reality it's better to have
            // your own implementation. An example with Redis exists in the repository
            if (userStore == UserStore.InMemory)
            {
                builder.Services.AddSingleton<ILdapUserStore, InMemoryUserStore<TUserDetails>>();
            }
            else
            {
                builder.Services.AddSingleton<ILdapUserStore, RedisUserStore<TUserDetails>>();
            }

            builder.AddProfileService<LdapUserProfileService<TUserDetails>>(); // Claims? + ApplicationUser should be sent using a parameter
            builder.AddResourceOwnerValidator<LdapUserResourceOwnerPasswordValidator<TUserDetails>>(); // Get user profiles

            return builder;
        }

        /// <summary>
        /// Adds Ldap Users to identity server.
        /// </summary>
        /// <typeparam name="TUserDetails">The type of the user details.</typeparam>
        /// <typeparam name="TCustomUserStore">The type of the custom user store.</typeparam>
        /// <param name="builder">The builder.</param>
        /// <param name="configuration">The ldap configuration.</param>
        /// <param name="customUserStore">The custom user store (ILdapUserStore).</param>
        /// <returns>
        /// Returns the builder instance
        /// </returns>
        public static IIdentityServerBuilder AddLdapUsers<TUserDetails, TCustomUserStore>(this IIdentityServerBuilder builder, IConfiguration configuration)
            where TUserDetails : IAppUser, new()
            where TCustomUserStore : ILdapUserStore
        {
            RegisterLdapConfigurations(builder, configuration);
            builder.Services.AddSingleton<ILdapService<TUserDetails>, LdapService<TUserDetails>>();

            // For testing purpose we can use the in memory. In reality it's better to have
            // your own implementation. An example with Redis exists in the repository
            builder.Services.AddSingleton(typeof(TCustomUserStore));
            builder.Services.AddSingleton(serviceProvider => (ILdapUserStore)serviceProvider.GetService(typeof(TCustomUserStore)));

            builder.AddProfileService<LdapUserProfileService<TUserDetails>>();
            builder.AddResourceOwnerValidator<LdapUserResourceOwnerPasswordValidator<TUserDetails>>();

            return builder;
        }

        private static void RegisterLdapConfigurations(IIdentityServerBuilder builder, IConfiguration configuration)
        {
            // Consider multiple configuration as a default way of working
            var configs = (ExtensionConfig)configuration.Get(typeof(ExtensionConfig));

            // Fallback to one configuration in case the collection was containing 0.
            if (configs.Connections?.Count == null)
            {
                var config = (LdapConfig)configuration.Get(typeof(LdapConfig));

                configs.Redis = config.Redis;
                configs.RefreshClaimsInSeconds = config.RefreshClaimsInSeconds;
                configs.Connections = new List<LdapConfig> { config };
            }

            // Enforce a name for each.
            var configIndex = 0;
            configs.Connections.ToList().ForEach(f =>
            {
                configIndex++;
                f.FriendlyName = !string.IsNullOrEmpty(f.FriendlyName) ? f.FriendlyName : $"Config #{configIndex}";
            });

            builder.Services.AddSingleton(configs);
        }
    }
}