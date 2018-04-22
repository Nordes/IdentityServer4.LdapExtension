[![Build status](https://ci.appveyor.com/api/projects/status/k26pepb32vka29w2/branch/master?svg=true)](https://ci.appveyor.com/project/Nordes/identityserver4-ldapextension/branch/master)


# IdentityServer4.LdapExtension
IdentityServer4 Ldap Extension (OpenLdap or ActiveDirectory).

## Table of content
* [Nuget Package Installation](#installation)
* [Configuration on IdentityServer4 server](#IS.BaseConfiguration)
  * [AppSettings Configuration](#IS.AppSettings)
* [You don't have a LDAP for your tests?](#Ldap.Test)
* [Features in progress](#NewFeature)
* [License](#license)

## Installation
<a name="installation"/>

The plugin is easy to install to your solution. Built using **.Net Standard 2.0**. The Nuget package can be installed by either searching the package `IdentityServer.LdapExtension` or by typing the following command in your package console:

```csharp
Install-Package IdentityServer.LdapExtension
```

## Configuration on IdentityServer4 server
<a name="IS.BaseConfiguration"/>

An easy extension method have been created in order to add the LDAP as a provider to your IdentityServer. For this you simply have to use the `AddLdapUsers<TApplicationUser>(LdapConfigSection, StoreTypeOrCustomStore)`. The configuration has to be provided or it won't work. The configuration is described [here](#IS.AppSettings).

In the `Startup.cs` under `ConfigureServices` method, you will have something similar to the following by default (Starter pack for IdentityServer). The last line is what you will need to add in order to get started.

```csharp
// ... Code ...
services.AddIdentityServer()
    .AddDeveloperSigningCredential()
    //.AddSigningCredential(...)
    .AddInMemoryIdentityResources(Config.GetIdentityResources())
    .AddInMemoryApiResources(Config.GetApiResources())
    .AddInMemoryClients(Config.GetClients())
    .AddLdapUsers<OpenLdapAppUser>(Configuration.GetSection("ldap"), UserStore.InMemory);
// ... Code ...
```

**Application User:** `2` (`OpenLdapAppUser`, `ActiveDirectoryAppUser`) have been provided with this extension, but you can use your own as long as you implement the interface `IAppUser`. I encourrage you to provide your own implementation. You might want to have claims/roles based on an active directory group or your attributes within LDAP are not the one I have defined.

**Store types:**
1. `UserStore.InMemory`: Can be used when you test locally. It stores the logged in user in memory in order to avoid querying the LDAP server over and over. It is also used in order to store the external logged in user details (Google, Facebook, etc.).
2. `UserStore.Redis`: Same as in memory, but is persisted and will be ready when you restart.
3. `ILdapUserStore` implementation: Build your own store implementation and pass it as a parameter.

### AppSettings Configuration
<a name="IS.AppSettings"/> 

The `appsettings.json` will require a configuration for the extension. Here's an example using OpenLdap:

```json
{
  "ldap": { // Name can be of your choosing
    "url": "localhost",
    "port": 389,
    "bindDn": "cn=ldap-ro,dc=contoso,dc=com",
    "bindCredentials": "P@ss1W0Rd!",
    "searchBase": "ou=users,DC=contoso,dc=com",
    "searchFilter": "(&(objectClass=posixAccount)(objectClass=person)(uid={0}))"
    // "redis": "localhost:32771,ssl=false", // Required if using UserStore.Redis 
  }
}
```

If you want to see a working demo, you can open the implementation available the `sample` folder. It is based on the QuickStart from [IdentityServer4 WebSite](http://docs.identityserver.io/en/release/).

## You don't have a LDAP for your tests?
<a name="Ldap.Test"/>

It's not a big problem. I wrote a small tutorial/article in order to setup an entire OpenLdap server within Docker in order to not pollute your PC and also to avoid relying on network admnistrator. That way you can play with existing users or create your own users directory. The tutorial/article is available at https://nordes.github.io/#/Articles/howto-openldap-with-contoso-users.

## Features in progress
<a name="NewFeature"/>

I plan to work on the following:
* Create a demo page using VueJS + Dotnet instead of Angular demo.
* Implement the SSL
* Implement a cache invalidation based on time (After x time without being hit, remove from redis or from memory).

## Contributors
* Me, the author of the package (@Nordes)

### Thanks to:
* @marianahycit

## Licenses
<a name="license"/>

MIT

> Regarding the IdentityServer4 Sample - Apache 2 (due to original code a bit updated)
