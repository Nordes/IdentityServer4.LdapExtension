using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Duende.IdentityServer.Models;
using IdentityServer.LdapExtension;
using IdentityServer.LdapExtension.UserStore;
using Microsoft.Extensions.Logging;

namespace IdentityServerHost.Extensions
{
    public class HostProfileService : LdapUserProfileService
    {
        public HostProfileService(ILdapUserStore users, ILogger<LdapUserProfileService> logger) : base(users, logger)
        {
        }

        public override async Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            await base.GetProfileDataAsync(context);

            var transaction = context.RequestedResources.ParsedScopes.FirstOrDefault(x => x.ParsedName == "transaction");
            if (transaction?.ParsedParameter != null)
            {
                context.IssuedClaims.Add(new Claim("transaction_id", transaction.ParsedParameter));
            }
        }
    }
}