using IdentityServer.LdapExtension.Exceptions;
using IdentityServer.LdapExtension.UserModel;
using IdentityServer.LdapExtension.UserStore;
using Moq;
using Novell.Directory.Ldap;
using System;
using System.Linq;
using Xunit;

namespace IdentityServer.LdapExtension.Unit.UserStores
{
    public class InMemoryUserStoreTests
    {
        private readonly Mock<ILdapService<OpenLdapAppUser>> _authenticationService;
        private readonly InMemoryUserStore<OpenLdapAppUser> _inMemoryUserStore;

        public InMemoryUserStoreTests()
        {
            _authenticationService = new Mock<ILdapService<OpenLdapAppUser>>();
            _inMemoryUserStore = new InMemoryUserStore<OpenLdapAppUser>(_authenticationService.Object);
        }

        [Fact]
        public void ValidateCredential_WhenLoginReturnNull_ExpectDefaultUser()
        {
            _authenticationService.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(default(OpenLdapAppUser));

            var result = _inMemoryUserStore.ValidateCredentials("username", "password");

            Assert.Equal(default(OpenLdapAppUser), result);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void ValidateCredential_WhenLoginThrowsLoginFailedException_ExpectDefaultUser()
        {
            _authenticationService.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new LoginFailedException("Login failed."));

            var result = _inMemoryUserStore.ValidateCredentials("username", "password");

            Assert.Equal(default(OpenLdapAppUser), result);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void ValidateCredential_WhenLoginThrowsUnhandledException_ExpectSameExceptionToBeThrown()
        {
            var exceptionMessage = "An exception thrown.";
            _authenticationService.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Throws(new Exception(exceptionMessage));

            var exception = Assert.Throws<Exception>(() => { _inMemoryUserStore.ValidateCredentials("username", "password"); });

            Assert.Equal(exceptionMessage, exception.Message);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void ValidateCredential_WhenLoginIsValid_ExpectUserDetails()
        {
            _authenticationService.Setup(m => m.Login(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new OpenLdapAppUser { Username = "username" });

            var user = _inMemoryUserStore.ValidateCredentials("username", "password");

            Assert.Equal("username", user.Username);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void FindBySubjectId_WhenSubjectIdNotCached_ExpectToSearchLdapAndFindUser()
        {
            _authenticationService.Setup(m => m.FindUser(It.IsAny<string>()))
                .Returns(new OpenLdapAppUser { Username = "ldap_username" });

            var user = _inMemoryUserStore.FindBySubjectId("ldap_username");

            Assert.Equal("ldap_username", user.Username);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void FindBySubjectId_WhenSubjectIdNotCached_ExpectToSearchLdapAndNotFoundReturnDefaultUser()
        {
            _authenticationService.Setup(m => m.FindUser(It.IsAny<string>()))
                .Returns(default(OpenLdapAppUser));

            var user = _inMemoryUserStore.FindBySubjectId("ldap_username");

            Assert.Equal(default(OpenLdapAppUser), user);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void FindByUsername_WhenUsernameNotCached_ExpectToSearchLdapAndFindUser()
        {
            _authenticationService.Setup(m => m.FindUser(It.IsAny<string>()))
                .Returns(new OpenLdapAppUser { Username = "username" });

            var user = _inMemoryUserStore.FindByUsername("username");

            Assert.Equal("username", user.Username);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void FindByUsername_WhenUsernameNotCached_ExpectToSearchLdapAndNotFoundReturnDefaultUser()
        {
            _authenticationService.Setup(m => m.FindUser(It.IsAny<string>()))
                .Returns(default(OpenLdapAppUser));

            var user = _inMemoryUserStore.FindByUsername("username");

            Assert.Equal(default(OpenLdapAppUser), user);
            _authenticationService.VerifyAll();
        }

        [Fact]
        public void ActiveDirectoryAttributeDisplayNameIsNull_SetsAppUserDisplayNameNull()
        {
            var ldapAttributeSet = new LdapAttributeSet();
            ldapAttributeSet.Add(new LdapAttribute("distinguishedName", "cn=testuser,cn=users,dc=example,dc=com"));
            ldapAttributeSet.Add(new LdapAttribute("cn", "testuser"));
            ldapAttributeSet.Add(new LdapAttribute("givenName", "Test"));
            ldapAttributeSet.Add(new LdapAttribute("name", "testuser"));
            ldapAttributeSet.Add(new LdapAttribute("userPrincipalName", "testuser@example.com"));
            ldapAttributeSet.Add(new LdapAttribute("sAMAccountName", "testuser")); 

            var ldapEntry = new LdapEntry("cn=testuser,cn=users,dc=example,dc=com", ldapAttributeSet);

            var appUser = new ActiveDirectoryAppUser();
            appUser.SetBaseDetails(ldapEntry, "local");

            Assert.Null(appUser.DisplayName);
        }

        [Fact]
        public void ActiveDirectoryAttributeWithExtrafield()
        {
            var ldapAttributeSet = new LdapAttributeSet();
            ldapAttributeSet.Add(new LdapAttribute("distinguishedName", "cn=testuser,cn=users,dc=example,dc=com"));
            ldapAttributeSet.Add(new LdapAttribute("cn", "testuser"));
            ldapAttributeSet.Add(new LdapAttribute("givenName", "Test"));
            ldapAttributeSet.Add(new LdapAttribute("name", "testuser"));
            ldapAttributeSet.Add(new LdapAttribute("userPrincipalName", "testuser@example.com"));
            ldapAttributeSet.Add(new LdapAttribute("sAMAccountName", "testuser"));
            ldapAttributeSet.Add(new LdapAttribute("displayName", "TestUser"));
            ldapAttributeSet.Add(new LdapAttribute("testfield", "extrafield"));

            var ldapEntry = new LdapEntry("cn=testuser,cn=users,dc=example,dc=com", ldapAttributeSet);

            var appUser = new ActiveDirectoryAppUser();
            appUser.SetBaseDetails(ldapEntry, "local", new string[] { "testfield" });

            Assert.NotNull(appUser.DisplayName);

            Assert.Equal("extrafield", appUser.Claims.FirstOrDefault(x => x.Type.Equals("testfield"))?.Value);
        }
    }
}
