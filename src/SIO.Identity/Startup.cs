﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SIO.Domain.Extensions;
using SIO.Domain.Users.Events;
using SIO.Infrastructure.Azure.ServiceBus.Extensions;
using SIO.Infrastructure.EntityFrameworkCore.SqlServer.Extensions;
using SIO.Infrastructure.Extensions;
using SIO.Infrastructure.Serialization.Json.Extensions;
using SIO.Infrastructure.Serialization.MessagePack.Extensions;

namespace SIO.Identity
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }        

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore()
                    .AddApiExplorer();

            services.AddMvc()
                .AddRazorRuntimeCompilation()
                .SetCompatibilityVersion(CompatibilityVersion.Latest);

            services.AddSIOInfrastructure()
                .AddEntityFrameworkCoreSqlServer(options => {
                    options.AddStore(_configuration.GetConnectionString("Store"), o => o.MigrationsAssembly($"{nameof(SIO)}.{nameof(Migrations)}"));
                })
                .AddAzureServiceBus(options =>
                {
                    options.UseConnection(_configuration.GetConnectionString("AzureServiceBus"))
                    .UseTopic(e =>
                    {
                        e.WithName(_configuration.GetValue<string>("Azure:ServiceBus:Topic"));
                    });
                })
                .AddCommands()
                .AddEvents(options =>
                {
                    options.Register<UserEmailChanged>();
                    options.Register<UserLoggedIn>();
                    options.Register<UserLoggedOut>();
                    options.Register<UserPasswordTokenGenerated>();
                    options.Register<UserPurchasedCharacterTokens>();
                    options.Register<UserRegistered>();
                    options.Register<UserVerified>();
                })
                .AddQueries()
                .AddJsonSerializers();
            
            services.AddSIOIdentity()
                .AddDomain();


            services.Configure<RazorViewEngineOptions>(o =>
            {
                o.ViewLocationFormats.Add($"/{{1}}/Views/{{0}}{RazorViewEngine.ViewExtension}");
            });

            services.AddCors(options =>
                     options.AddPolicy("cors", builder =>
                     {
                         builder.WithOrigins(_configuration.GetValue<string>("DefaultAppUrl"))
                                .AllowAnyMethod()
                                .AllowCredentials()
                                .AllowAnyHeader();
                     }));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseCors("cors");
            app.UseRouting();
            app.UseIdentityServer();

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
