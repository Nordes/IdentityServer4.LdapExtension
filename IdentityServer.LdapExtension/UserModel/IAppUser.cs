using System.Collections.Generic;
using System.Security.Claims;
using Novell.Directory.Ldap;

namespace IdentityServer.LdapExtension.UserModel
{
    public interface IAppUser
    {
        // Mandatory
        string SubjectId { get; set; }
        string Username { get; set; }
        string ProviderSubjectId { get; set; }
        string ProviderName { get; set; }
        bool IsActive { get; set; }
        string DisplayName { get; set; }

        ICollection<Claim> Claims { get; set; }

        /// <summary>
        /// Define the Ldap attributes that will be map on the user.
        /// </summary>
        string[] LdapAttributes { get; }

        /// <summary>
        /// This will set the base details such as:
        /// - DisplayName
        /// - Username
        /// - ProviderName
        /// - SubjectId
        /// - ProviderSubjectId
        /// - Fill the claims
        /// </summary>
        /// <param name="ldapEntry">Ldap Entry</param>
        /// <param name="providerName">Specific provider such as Google, Facebook, etc.</param>
        void SetBaseDetails(LdapEntry ldapEntry, string providerName, IEnumerable<string> extraFields = null);
    }
}
