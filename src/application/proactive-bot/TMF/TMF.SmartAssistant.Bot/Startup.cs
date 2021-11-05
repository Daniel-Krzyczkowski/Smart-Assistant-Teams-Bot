// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EmptyBot v4.14.0

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Collections.Concurrent;
using TMF.SmartAssistant.Bot.Bots;
using TMF.SmartAssistant.Bot.Configuration;
using TMF.SmartAssistant.Bot.Core.DependencyInjection;

namespace TMF.SmartAssistant.Bot
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<CosmosDbConfiguration>(Configuration.GetSection("CosmosDbConfiguration"));

            services.AddDatabaseServices();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer("ClientCredentialsScheme", jwtOptions =>
                        {
                            jwtOptions.MetadataAddress = Configuration["AzureAD:MetadataAddress"];
                            jwtOptions.Authority = Configuration["AzureAD:Authority"];
                            jwtOptions.Audience = Configuration["AzureAD:ClientId"];
                        });

            services.AddAuthorization(options =>
            {
                var authorizationPolicy = new AuthorizationPolicyBuilder()
                                              .RequireRole("access:bot")
                                              .AddAuthenticationSchemes("ClientCredentialsScheme")
                                              .Build();
                options.AddPolicy("BotAccessAuthorizationPolicy", authorizationPolicy);
            });

            services.AddSingleton<ConcurrentDictionary<string, ConversationReference>>();

            services.AddHttpClient().AddControllers().AddNewtonsoftJson();

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, ProactiveBot>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseDefaultFiles()
                .UseStaticFiles()
                .UseWebSockets()
                .UseRouting()
                .UseAuthentication()
                .UseAuthorization()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapControllers();
                });

            // app.UseHttpsRedirection();
        }
    }
}
