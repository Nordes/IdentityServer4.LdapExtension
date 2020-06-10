![License](https://img.shields.io/github/license/mashape/apistatus.svg)
[![Build status](https://ci.appveyor.com/api/projects/status/k26pepb32vka29w2/branch/master?svg=true)](https://ci.appveyor.com/project/Nordes/identityserver4-ldapextension/branch/master)
[![NuGet](https://img.shields.io/nuget/v/IdentityServer.LdapExtension.svg)](https://www.nuget.org/packages/IdentityServer.LdapExtension/)

- [IdentityServer4.LdapExtension](#identityserver4ldapextension)
  - [Installation](#installation)
  - [Configuration for IdentityServer4 Server](#configuration-for-identityserver4-server)
    - [AppSettings Configuration](#appsettings-configuration)
    - [Multiple concurent Ldap (For different DN, or totally different Ldap)](#multiple-concurent-ldap-for-different-dn-or-totally-different-ldap)
      - [Quick and Simple Example of a Configuration](#quick-and-simple-example-of-a-configuration)
  - [You don't have an LDAP for your tests, use a OpenLdap docker image instead!](#you-dont-have-an-ldap-for-your-tests-use-a-openldap-docker-image-instead)
  - [Features in progress](#features-in-progress)
  - [Contributors](#contributors)
    - [Special thanks to](#special-thanks-to)
  - [License](#license)

# IdentityServer4.LdapExtension
IdentityServer4 Ldap Extension ([OpenLdap](https://www.openldap.org/) or [ActiveDirectory](https://en.wikipedia.org/wiki/Active_Directory)).

## Installation
The plugin is easy to install to your solution. Built using **.Net Standard 2.0**. The Nuget package can be installed by either searching the package `IdentityServer.LdapExtension` or by typing the following command in your package console:

```csharp
Install-Package IdentityServer.LdapExtension
```

> Be aware of the dependency with IdentityServer4. The version of the package is visible in your Visual Studio or through Nuget.org.
> - Ldap Extension 2.0.0 goes with IdentityServer 2.2.x 
> - Ldap Extension 2.1.7 goes with IdentityServer 2.3.x 
> - Ldap Extension 2.1.8 goes with IdentityServer 2.4.x

## Configuration for IdentityServer4 Server
An easy extension method have been created in order to add the LDAP as a provider to your IdentityServer. For this you simply have to use the `AddLdapUsers<TApplicationUser>(LdapConfigSection, StoreTypeOrCustomStore)`. The configuration has to be provided or it won't work. The configuration is described [here](#appsettings-configuration).

In the `Startup.cs` under `ConfigureServices` method, you will have something similar to the following by default (Starter pack for IdentityServer). The last line is what you will need to add in order to get started.

```csharp
// ... Code ...
services.AddIdentityServer()
    .AddDeveloperSigningCredential()
    //.AddSigningCredential(...)
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients())
    .AddLdapUsers<OpenLdapAppUser>(Configuration.GetSection("MyConfigurationSection"), UserStore.InMemory);
// ... Code ...
```

**Application User:** `2` (`OpenLdapAppUser`, `ActiveDirectoryAppUser`) have been provided with this extension, but you can use your own as long as you implement the interface `IAppUser`. I encourrage you to provide your own implementation. You might want to have claims/roles based on an active directory group or your attributes within LDAP are not the one I have defined.

**Store types:**
1. `UserStore.InMemory`: Can be used when you test locally. It stores the logged in user in memory in order to avoid querying the LDAP server over and over. It is also used in order to store the external logged in user details (Google, Facebook, etc.).
2. `UserStore.Redis`: Same as in memory, but is persisted and will be ready when you restart.
3. `ILdapUserStore` implementation: Build your own store implementation and pass it as a parameter.

### AppSettings Configuration
The `appsettings.json` will require a configuration for the extension. Here's an example using OpenLdap:

```javascript
{
  "MyConfigurationSection": { // Name can be of your choosing
    "Url": "localhost",
    "Port": 389,
    "BindDn": "cn=ldap-ro,dc=contoso,dc=com",
    "BindCredentials": "P@ss1W0Rd!",
    "SearchBase": "ou=users,DC=contoso,dc=com",
    "SearchFilter": "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))"
    // "Redis": "localhost:32771,ssl=false", // Required if using UserStore.Redis 
  }
}
```

If you want to see a working demo, you can open the implementation available the `sample` folder. It is based on the QuickStart from [IdentityServer4 WebSite](http://docs.identityserver.io/en/release/).

### Multiple concurent Ldap (For different DN, or totally different Ldap)
In the case you would have a need to have multiple configuration to either connect to different LDAP servers or to even connect to different part of the directory (multiple area for the DN), this feature have been requested and it should be able to allow different type of AD to live together. The AAD is of course not part of this. In case you would like to use AAD, there's either other connector or you can also write your own.

The usage of multiple configuration will bring some issues, so here's the rules:
1. Configurations needs to be all the same type, except if you have a custom LDapUser and you're not using the one provided in this extension.
2. Rules for `preFilterRegex` can discriminate in order to not try on all the LDAP server the credential/password for faillure. It also avoid having some kind of DoS on all your server in case of attack.
3. If we have multiple LDAP configuration that are ok with the `preFilterRegex`, then the validation is done async (To be confirmed) and the first server to answer OK will be the one to use in order to get the information. The issue in that case is that it will try to call all your servers and that's probably not something you wish for.
4. If it does not match anything, the extension will send back automatically a user not found.

By default the cache is using InMemory, but you can also use Redis. It needs to be set in the global configuration when multiple Ldap entries. This avoid having custom code for each Ldap.

#### Quick and Simple Example of a Configuration
2 configurations using a `preFilterRegex` for discrimination.
```javascript
  "IdentityServerLdap": {
    // Example: If you use a redis instead of in-memory (See Startup.cs)
    //"redis": "localhost:32771,ssl=false",
    //"RefreshClaimsInSeconds": 3600,
    "Connections": [
      {
        "FriendlyName": "OpenLdap-Users",
        "Url": "localhost",
        "Port": 389,
        "Ssl": false,
        "BindDn": "cn=ldap-ro,dc=contoso,dc=com",
        "BindCredentials": "P@ss1W0Rd!",
        "SearchBase": "ou=users,DC=contoso,dc=com",
        "SearchFilter": "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))",
        "PreFilterRegex": "^(?![a|A]).*$" // not mandatory and will take everything not starting with A
      },
      {
        "FriendlyName": "OpenLdap-BuzzUsers",
        "Url": "localhost",
        "Port": 389,
        "Ssl": false,
        "BindDn": "cn=ldap-ro,dc=contoso,dc=com",
        "BindCredentials": "P@ss1W0Rd!",
        "SearchBase": "ou=users-buzz,DC=contoso,dc=com",
        "SearchFilter": "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))",
        "PreFilterRegex": "^([a|A]).*$" // not mandatory and will take everything not starting with A
      }
    ]
  }
```

In startup, the same as a single configuration. Basically the configuration section and nothing more. If it's a single configuration, it will upgrade the single configuration to act like a multi-configuration. It is recommended from now on to use the multi-configuration style. It's easier to handle the Redis and other new features if any comes.

## You don't have an LDAP for your tests, use a OpenLdap docker image instead!
It's not a big problem. I wrote a small tutorial/article in order to setup an entire OpenLdap server within Docker in order to not pollute your PC and also to avoid relying on network admnistrator. That way you can play with existing users or create your own users directory. The tutorial/article is available at [HoNoSoFt](https://blog.honosoft.com/2018/06/18/ldap-identity-server-series-%E3%83%BC-part-i-%E3%83%BC-openldap-on-docker-container/) website.

## Features in progress
I plan to work on the following:
* Implement the SSL
* Implement a cache invalidation based on time (After x time without being hit, remove from redis or from memory).

## Contributors
**Main contributor**
* @Nordes: The main author of the package (@me)
  
### Special thanks to
* @marianahycit: Contribution
* @uchetfield: Contribution (Issue #10)
* @ttutko
* @chrgraefe 

## License
MIT

> Regarding the IdentityServer4 Sample - Apache 2 (due to original code a bit updated)
