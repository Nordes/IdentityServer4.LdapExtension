using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            builder.Services.Configure<LdapConfig>(configuration);
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
        /// <param name="configuration">The configuration.</param>
        /// <param name="customUserStore">The custom user store (ILdapUserStore).</param>
        /// <returns>
        /// Returns the builder instance
        /// </returns>
        public static IIdentityServerBuilder AddLdapUsers<TUserDetails, TCustomUserStore>(this IIdentityServerBuilder builder, IConfiguration configuration, ILdapUserStore customUserStore)
            where TUserDetails : IAppUser, new()
        {
            builder.Services.Configure<LdapConfig>(configuration.GetSection("ldap"));
            builder.Services.AddSingleton<ILdapService<TUserDetails>, LdapService<TUserDetails>>();

            // For testing purpose we can use the in memory. In reality it's better to have
            // your own implementation. An example with Redis exists in the repository
            builder.Services.AddSingleton(customUserStore);

            builder.AddProfileService<LdapUserProfileService<TUserDetails>>();
            builder.AddResourceOwnerValidator<LdapUserResourceOwnerPasswordValidator<TUserDetails>>();

            return builder;
        }
    }
}