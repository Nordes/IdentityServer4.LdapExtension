using System.Collections.Generic;

namespace IdentityServer.LdapExtension
{
    public class ExtensionConfig
    {
        /// <summary>
        /// Gets or sets the redis connection string. It follows the Redis library connection string
        /// format.
        /// </summary>
        /// <example>localhost:32771,ssl=false</example>
        /// <remarks>Optional</remarks>
        public string Redis { get; set; }

        /// <summary>
        /// Gets or sets the LDAP configurations.
        /// </summary>
        public ICollection<LdapConfig> Connections { get; set; }

        /// <summary>
        /// Gets or sets the refresh claims in seconds.
        /// </summary>
        /// <remarks>[Not fully implemented] Optional</remarks>
        public uint? RefreshClaimsInSeconds { get; set; } = null;
    }
}
