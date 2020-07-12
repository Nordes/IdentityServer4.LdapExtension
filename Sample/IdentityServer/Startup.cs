using IdentityServer.LdapExtension.Extensions;
using IdentityServer.LdapExtension.UserModel;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QuickstartIdentityServer
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env, IConfiguration configuration)
        {
            Env = env;
            Configuration = configuration;
        }

        private IWebHostEnvironment Env { get; }
        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            // configure identity server with in-memory stores, keys, clients and scopes
            services.AddIdentityServer()
                .AddDeveloperSigningCredential()
                ////.AddSigningCredential(...) // Strongly recommended, if you want something more secure than developer signing (Read The Manual since it's highly recommended)
                .AddInMemoryIdentityResources(InMemoryInitConfig.GetIdentityResources())
                .AddInMemoryApiResources(InMemoryInitConfig.GetApiResources())
                .AddInMemoryClients(InMemoryInitConfig.GetClients())
                .AddLdapUsers<OpenLdapAppUser>(Configuration.GetSection("IdentityServerLdap"), UserStore.InMemory);
        }

        public void Configure(IApplicationBuilder app)
        {
            if (Env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(c => c.MapDefaultControllerRoute());
        }
    }
}