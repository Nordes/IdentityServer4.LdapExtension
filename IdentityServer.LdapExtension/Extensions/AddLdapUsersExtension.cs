using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityServer.LdapExtension.Extensions
{
    public static class AddLdapUsersExtension
    {
        /// <summary>
        /// <returns>The builder instance</returns>
        /// </summary>
        /// <typeparam name="TUserDetails"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="userStore"></param>
        /// <returns></returns>
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
        /// <typeparam name="TUserDetails"></typeparam>
        /// <typeparam name="TCustomUserStore"></typeparam>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <param name="customUserStore"></param>
        /// <returns>The builder instance</returns>
        public static IIdentityServerBuilder AddLdapUsers<TUserDetails, TCustomUserStore>(this IIdentityServerBuilder builder, IConfiguration configuration, ILdapUserStore customUserStore)
            where TUserDetails : IAppUser, new()
        {
            builder.Services.Configure<LdapConfig>(configuration.GetSection("ldap"));
            builder.Services.AddSingleton<ILdapService<TUserDetails>, LdapService<TUserDetails>>();

            // For testing purpose we can use the in memory. In reality it's better to have
            // your own implementation. An example with Redis exists in the repository
            builder.Services.AddSingleton(customUserStore);

            builder.AddProfileService<LdapUserProfileService<TUserDetails>>(); // Claims? + ApplicationUser should be sent using a parameter
            builder.AddResourceOwnerValidator<LdapUserResourceOwnerPasswordValidator<TUserDetails>>(); // Get user profiles

            return builder;
        }
    }
}