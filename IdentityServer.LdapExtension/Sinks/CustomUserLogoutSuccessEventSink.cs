using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;

namespace IdentityServer.LdapExtension
{
    /// <summary>
    /// Should be an event when the user sign out. This way we could push details to the
    /// redis cache or other source.
    /// </summary>
    public class CustomUserLogoutSuccessEventSink : IEventSink
    {
        private readonly ILogger<CustomUserLogoutSuccessEventSink> _log;

        public CustomUserLogoutSuccessEventSink(ILogger<CustomUserLogoutSuccessEventSink> logger)
        {
            _log = logger;
        }

        public Task PersistAsync(Event evt)
        {
            if (evt == null) throw new ArgumentNullException(nameof(evt));

            _log.LogInformation("Event details: {@Event}", evt);

            return Task.CompletedTask;
        }
    }
}
