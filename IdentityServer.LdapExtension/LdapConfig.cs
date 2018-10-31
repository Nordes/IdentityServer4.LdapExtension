using Novell.Directory.Ldap;
using System.Collections.Generic;
/// <summary>
/// Configuration section that can be serialized from the AppSettings configuration.
/// </summary>
namespace IdentityServer.LdapExtension
{
    public class LdapConfig
    {
        public ICollection<LdapHost> Hosts { get; set; }
    }
    public class LdapHost
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public int Port { get; set; } = LdapConnection.DEFAULT_PORT;
        public bool Ssl { get; set; }
        public string BindDn { get; set; }
        public string BindCredentials { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
        public string Redis { get; set; }
        public int RefreshClaimsInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the extra attributes. These extra attributes
        /// are the one from LDAP not part of the enum <see cref="LdapAttributes"/>.
        /// </summary>
        /// <remarks>Not being used in current implementation.</remarks>
        public string[] ExtraAttributes { get; set; }

    }
}
