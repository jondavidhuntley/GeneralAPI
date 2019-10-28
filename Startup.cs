using AutoMapper;
using GeneralAPI.Services;
using GeneralAPI.Services.DataServices;
using GeneralAPI.Services.DataServices.DAL;
using GeneralAPI.Services.DataServices.DAL.Dapper;
using GeneralAPI.Services.Messaging;
using GeneralAPI.Services.OAuth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerUI;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GeneralAPI
{
    /// <summary>
    /// The Startup for the project
    /// </summary>
    public class Startup
    {
        private AssemblyName _assemblyName = Assembly.GetEntryAssembly().GetName();

        /// <summary>
        /// Initializes a new instance of the <see cref="Startup"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Gets the configuration.
        /// </summary>
        /// <value>
        /// The configuration.
        /// </value>
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.

        /// <summary>
        /// Configures the services.
        /// </summary>
        /// <param name="services">The services.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            var gcpAudience = Configuration.GetValue<string>("GcpAudience");
            var taaAudience = Configuration.GetValue<string>("TaaDocumentAPIAudience");
            var taaTokenServiceUrl = Configuration.GetValue<string>("TaaTokenServiceUrl");
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = taaTokenServiceUrl;
                options.Audience = taaAudience;
            });
            // Add Authorization Options to Read and Save (Read/Store/Delete) Report Documents
            services.AddAuthorization(options =>
            {
                options.AddPolicy("read:documents", policy => policy.Requirements.Add(new HasScopeRequirement("read:documents", taaTokenServiceUrl)));
                options.AddPolicy("store:documents", policy => policy.Requirements.Add(new HasScopeRequirement("store:documents", taaTokenServiceUrl)));
                options.AddPolicy("delete:documents", policy => policy.Requirements.Add(new HasScopeRequirement("delete:documents", taaTokenServiceUrl)));
            });
            // Add Authorization Handler
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            var dbConnection = Configuration.GetConnectionString("DocumentDatabase");
            services.AddSingleton<IReportDAL>((svc) =>
            {
                var logger = svc.GetService<ILogger<ReportDAL>>();
                return new ReportDAL(dbConnection, logger);
            });
            services.AddSingleton<IReportDataService, ReportDataService>();
            services.AddSingleton<IClientTokenHandler>((svc) =>
            {
                var logger = svc.GetService<ILogger<ClientTokenHandler>>();
                var tokenServiceUrl = Configuration.GetValue<string>("GcpTokenServiceUrl");
                var clientId = Configuration.GetValue<string>("GcpClientId");
                var clientSecret = Configuration.GetValue<string>("GcpClientSecret");
                var grantType = Configuration.GetValue<string>("GrantType");
                return new ClientTokenHandler(tokenServiceUrl, clientId, clientSecret, gcpAudience, grantType, logger);
            });

            // Add Message Publisher
            services.AddSingleton<IMessagePublisher>((svc) =>
            {
                var sbusConnection = Configuration.GetConnectionString("ServiceBusTopicalNotifications");
                var logger = svc.GetService<ILogger<MessagePublisher>>();

                return new MessagePublisher(sbusConnection, false, logger);
            });

            // Add Notification Service
            services.AddSingleton<INotificationService>((svc) =>
            {
                var dataService = svc.GetService<IReportDataService>();
                var messageService = svc.GetService<IMessagePublisher>();
                var secondaryReportNotificationTopic = Configuration.GetValue<string>("TopicSecondaryReportNotification");
                var logger = svc.GetService<ILogger<NotificationService>>();

                return new NotificationService(dataService, messageService, secondaryReportNotificationTopic, logger);
            });

            var documentAPIURL = Configuration.GetValue<string>("DocumentAPIUrl");
            services.AddSingleton<IDocumentDeletionProcessor>((svc) =>
            {
                var logger = svc.GetService<ILogger<DocumentDeletionProcessor>>();
                return new DocumentDeletionProcessor(documentAPIURL, logger);
            });

            services.AddSingleton<IHistoricDataDeletionProcessor, HistoricDataDeletionProcessor>();
            services.AddSingleton<IDocumentUpsertProcessor, DocumentUpsertProcessor>();            

            services.AddSingleton<IDocumentHandler<EBITDARCostsAnalysis>, DocumentHandler<EBITDARCostsAnalysis>>();
            services.AddSingleton<IDocumentHandler<TotalOpCostsAnalysis>, DocumentHandler<TotalOpCostsAnalysis>>();
            services.AddSingleton<IDocumentHandler<SupplementaryInfoRaw>, DocumentHandler<SupplementaryInfoRaw>>();
            services.AddSingleton<IDocumentHandler<SupplementaryInfo>, DocumentHandler<SupplementaryInfo>>();
            services.AddSingleton<IDocumentHandler<RevenueAnalysis>, DocumentHandler<RevenueAnalysis>>();
            services.AddSingleton<IDocumentHandler<RevenueAnalysisRaw>, DocumentHandler<RevenueAnalysisRaw>>();

            services.AddSingleton<IDocumentStoreSettings>(new DocumentStoreSettings(Configuration.GetValue<string>("DocumentAPIUrl")));

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(
                    $"v{_assemblyName.Version.ToString()}",
                    new Info
                    {
                        Title = $"{_assemblyName.Name}",
                        Version = $"{_assemblyName.Version.ToString()}"
                    });
                c.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    In = "header",
                    Description = "Add the word 'Bearer' followed by space & JWT",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Array.Empty<string>() },
                });
            });

#pragma warning disable CS0618 // Type or member is obsolete
            Mapper.Initialize(cfg =>
            {
                cfg.AddProfile<ReportProfile>();
            });
#pragma warning restore CS0618 // Type or member is obsolete
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        /// <summary>
        /// Configures the specified application.
        /// </summary>
        /// <param name="app">The application.</param>
        /// <param name="env">The env.</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v{_assemblyName.Version.ToString()}/swagger.json", $"{_assemblyName.Name} v{_assemblyName.Version.ToString()}");
                c.DocumentTitle = $"{_assemblyName.Name} v{_assemblyName.Version.ToString()}";
                c.DocExpansion(DocExpansion.None);
            });
            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseMvc();
        }
    }
}