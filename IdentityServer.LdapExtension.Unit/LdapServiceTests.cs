using IdentityServer.LdapExtension.UserModel;
using Moq;
using Xunit;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using IdentityServer.LdapExtension.DirectoryServices;
using System.Collections.Generic;

namespace IdentityServer.LdapExtension.Sample
{
    public class LdapServiceTests
    {
        /// <summary>
        /// todo Should be a fixture
        /// </summary>
        private readonly LdapConfig _ldapConfig = new LdapConfig
        {
            Hosts = new HashSet<LdapHost>()
            {
                new LdapHost {
                    Name= "default",
                    Url = "localhost",
                    BindDn = "cn=ldap-ro,dc=contoso,dc=com",
                    BindCredentials = "P@ss1W0Rd!",
                    SearchBase = "ou=users,DC=contoso,dc=com",
                    SearchFilter = "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))",
                    Ssl = false,
                    Port = 389
                }
            }
        };

        private readonly Mock<IOptions<LdapConfig>> _ldapConfigMock;
        private readonly Mock<ILdapConnection> _ldapConnectionMock;
        private readonly ILogger<LdapService<OpenLdapAppUser>> _logger;

        public LdapServiceTests()
        {
            _ldapConfigMock = new Mock<IOptions<LdapConfig>>(MockBehavior.Strict);
            _ldapConnectionMock = new Mock<ILdapConnection>(MockBehavior.Strict);
            _logger = Mock.Of<ILogger<LdapService<OpenLdapAppUser>>>();
        }

        //[Fact]
        //public void FindUser_WhenUsernameEmptyOrNotFound_ShouldReturnDefaultUserClass()
        //{
        //    _ldapConfigMock.SetupGet(m => m.Value).Returns(_ldapConfig);
        //    _ldapConnectionMock.Setup(m => m.Connect(_ldapConfig.Url, It.IsAny<int>()));
        //    _ldapConnectionMock.Setup(m => m.Disconnect());
        //    _ldapConnectionMock.Setup(m => m.Bind(_ldapConfig.BindDn, _ldapConfig.BindCredentials));
        //    _ldapConnectionMock.Setup(m => m.Search(_ldapConfig.SearchBase, LdapConnection.SCOPE_SUB, It.IsAny<string>(), It.IsAny<string[]>(), false))
        //        .Returns(It.IsAny<Novell.Directory.Ldap.LdapSearchResults>());

        //    var ldapService = new LdapService<OpenLdapAppUser>(_ldapConfigMock.Object, _ldapConnectionMock.Object, _logger);
        //    var result = ldapService.FindUser(string.Empty);

        //    Assert.Equal(default(OpenLdapAppUser), result);
        //    _ldapConfigMock.VerifyAll();
        //    _ldapConnectionMock.VerifyAll();
        //}
    }
}
