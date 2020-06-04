using Novell.Directory.Ldap;
using System.Text.RegularExpressions;
/// <summary>
/// Configuration section that can be serialized from the AppSettings configuration.
/// </summary>
namespace IdentityServer.LdapExtension
{
    public class LdapConfig
    {
        private Regex _preFilterRegex = null;
        private string _preFilterRegexString = null;

        /// <summary>
        /// Gets or sets the name of the friendly name. This is used only for debug or personal information.
        /// In the future it might be used within the logs in case you need to understand what's happening.
        /// </summary>
        /// <example>OpenLdap Server-A</example>
        /// <remarks>Optional</remarks>
        public string FriendlyName { get; set; }

        /// <summary>
        /// Gets or sets the URL in order to access LDAP server.
        /// </summary>
        /// <example>localhost</example>
        /// <remarks>Required</remarks>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the port in order to connect to the LDAP server.
        /// </summary>
        /// <example>389</example>
        /// <remarks>Required</remarks>
        public int Port { get; set; } = LdapConnection.DefaultPort;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="LdapConfig" /> is SSL.
        /// </summary>
        /// <value>
        ///   <c>true</c> if SSL; otherwise, <c>false</c>.
        /// </value>
        /// <example>false</example>
        /// <remarks>Not yet being used. Some issue while using the library with it.</remarks>
        public bool Ssl { get; set; }

        /// <summary>
        /// Gets or sets the DN. This actually sets the base for your LDAP configuration.
        /// </summary>
        /// <example>cn=ldap-ro,dc=contoso,dc=com</example>
        /// <remarks>Required</remarks>
        public string BindDn { get; set; }

        /// <summary>
        /// Gets or sets the bind credentials (password).
        /// </summary>
        /// <example>P@ss1W0Rd!</example>
        /// <remarks>Required</remarks>
        public string BindCredentials { get; set; }

        /// <summary>
        /// Gets or sets the search base for your users. If you don't specify, we can't search
        /// and validate your users credentials.
        /// </summary>
        /// <example>ou=users,DC=contoso,dc=com</example>
        public string SearchBase { get; set; }

        /// <summary>
        /// Gets or sets the search filter in order to find your user. The parameter {0} is used in order to
        /// fill the current user trying to connect.
        /// </summary>
        /// <example>(&(objectClass=posixAccount)(objectClass=person)(uid={0}))</example>
        /// <remarks>Required</remarks>
        public string SearchFilter { get; set; }

        /// <summary>
        /// Gets or sets the redis connection string. It follows the Redis library connection string
        /// format.
        /// </summary>
        /// <example>localhost:32771,ssl=false</example>
        /// <remarks>Optional, when multiple configuration it is being ignored and use the global configuration.</remarks>
        public string Redis { get; set; }

        /// <summary>
        /// Gets or sets the pre filter regex for discrimination. It is not yet supporting the /regex/ig format.
        /// </summary>
        /// <example>^(?![a|A]).*$</example>
        /// <remarks>Optional</remarks>
        public string PreFilterRegex
        {
            // TODO Could use something like https://stackoverflow.com/questions/12075927/serialization-of-regexp solution for regex
            get { return _preFilterRegexString; }
            set
            {
                // Compiled since it might be used quite a lot over time
                _preFilterRegex = new Regex(value, RegexOptions.Compiled);
                _preFilterRegexString = value;
            }
        }

        /// <summary>
        /// Gets or sets the refresh claims in seconds.
        /// </summary>
        /// <remarks>[Not fully implemented] Optional and if multiple take the general configuration (like redis)</remarks>
        public uint? RefreshClaimsInSeconds { get; set; }

        /// <summary>
        /// Gets or sets the extra attributes. These extra attributes
        /// are the one from LDAP not part of the enum <see cref="LdapAttributes"/>.
        /// </summary>
        /// <remarks>Not being used in current implementation.</remarks>
        public string[] ExtraAttributes { get; set; }

        internal bool IsConcerned(string username)
        {
            if (_preFilterRegex == null)
            {
                return true;
            }

            return _preFilterRegex.IsMatch(username);
        }

        internal int FinalLdapConnectionPort
        {
            get
            {
                if (Port == 0)
                {
                    return Ssl ? LdapConnection.DefaultSslPort : LdapConnection.DefaultPort;
                }

                return Port;
            }
        }
    }
}
