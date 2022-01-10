using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.Versioning;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SocialCareCaseViewerApi
{
    public class Startup
    {
        private static List<ApiVersionDescription> APIVersions { get; set; }
        private const string ApiName = "Social Care Case Viewer API";

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddFluentValidation()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.IgnoreNullValues = true;
                });
            services.AddApiVersioning(o =>
            {
                o.DefaultApiVersion = new ApiVersion(1, 0);
                o.AssumeDefaultVersionWhenUnspecified = true; // assume that the caller wants the default version if they don't specify
                o.ApiVersionReader = new UrlSegmentApiVersionReader(); // read the version number from the url segment header)
            });

            services.AddSingleton<IApiVersionDescriptionProvider, DefaultApiVersionDescriptionProvider>();

            services.AddSwaggerGen(c =>
            {
                c.AddSecurityDefinition("Token",
                    new OpenApiSecurityScheme
                    {
                        In = ParameterLocation.Header,
                        Description = "Your Hackney API Key",
                        Name = "X-Api-Key",
                        Type = SecuritySchemeType.ApiKey
                    });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Token" }
                        },
                        new List<string>()
                    }
                });

                //Looks at the APIVersionAttribute [ApiVersion("x")] on controllers and decides whether or not
                //to include it in that version of the swagger document
                //Controllers must have this [ApiVersion("x")] to be included in swagger documentation!!
                c.DocInclusionPredicate((docName, apiDesc) =>
                {
                    apiDesc.TryGetMethodInfo(out var methodInfo);

                    var versions = methodInfo?
                        .DeclaringType?.GetCustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions).ToList();

                    return versions?.Any(v => $"{v.GetFormattedApiVersion()}" == docName) ?? false;
                });

                //Get every ApiVersion attribute specified and create swagger docs for them
                foreach (var apiVersion in APIVersions)
                {
                    var version = $"v{apiVersion.ApiVersion.ToString()}";
                    c.SwaggerDoc(version, new OpenApiInfo
                    {
                        Title = $"{ApiName}-api {version}",
                        Version = version,
                        Description = $"{ApiName} version {version}. Please check older versions for depreciated endpoints."
                    });
                }

                c.CustomSchemaIds(x => x.FullName);
                // Set the comments path for the Swagger JSON and UI.
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });
            ConfigureDbContext(services);
            RegisterGateways(services);
            RegisterUseCases(services);

            services.AddScoped<ISystemTime, SystemTime>();
        }

        private static void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<DatabaseContext>(
                opt => opt.UseNpgsql(connectionString ?? throw new InvalidOperationException("Must provide CONNECTION_STRING environment variable")));

            services.AddSingleton<ISccvDbContext, SccvDbContext>();

            //TODO: migrate historical data to service database 
            var historicalDataConnectionString = Environment.GetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING") ?? "Host=;Database=;";

            services.AddDbContext<HistoricalDataContext>(options => options
                .UseNpgsql(historicalDataConnectionString ?? throw new InvalidOperationException("Must provide HISTORICAL_DATA_CONNECTION_STRING environment variable"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking) //read only db for now, no need for tracking
            );
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<IDatabaseGateway, DatabaseGateway>();
            services.AddScoped<IProcessDataGateway, ProcessDataGateway>();
            services.AddScoped<IMongoGateway, MongoGateway>();
            services.AddScoped<ITeamGateway, TeamGateway>();
            services.AddScoped<ICaseStatusGateway, CaseStatusGateway>();
            services.AddScoped<IWorkerGateway, WorkerGateway>();
            services.AddScoped<IMashReferralGateway, MashReferralGateway>();
            services.AddScoped<IHistoricalDataGateway, HistoricalDataGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<ICaseRecordsUseCase, CaseRecordsUseCase>();
            services.AddScoped<IAllocationsUseCase, AllocationsUseCase>();
            services.AddScoped<ITeamsUseCase, TeamsUseCase>();
            services.AddScoped<ICaseNotesUseCase, CaseNotesUseCase>();
            services.AddScoped<IVisitsUseCase, VisitsUseCase>();
            services.AddScoped<IWarningNoteUseCase, WarningNoteUseCase>();
            services.AddScoped<IGetVisitByVisitIdUseCase, GetVisitByVisitIdUseCase>();
            services.AddScoped<IWorkersUseCase, WorkersUseCase>();
            services.AddScoped<IFormSubmissionsUseCase, FormSubmissionsUseCase>();
            services.AddScoped<IRelationshipsUseCase, RelationshipsUseCase>();
            services.AddScoped<IPersonalRelationshipsUseCase, PersonalRelationshipsUseCase>();
            services.AddScoped<ICaseStatusesUseCase, CaseStatusesUseCase>();
            services.AddScoped<ICreateRequestAuditUseCase, CreateRequestAuditUseCase>();
            services.AddScoped<IResidentUseCase, ResidentUseCase>();
            services.AddScoped<IMashReferralUseCase, MashReferralUseCase>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            //Get All ApiVersions,
            var api = app.ApplicationServices.GetService<IApiVersionDescriptionProvider>();
            APIVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in APIVersions)
                {
                    //Create a swagger endpoint for each swagger version
                    c.SwaggerEndpoint($"{apiVersionDescription.GetFormattedApiVersion()}/swagger.json",
                        $"{ApiName}-api {apiVersionDescription.GetFormattedApiVersion()}");
                }
            });
            app.UseSwagger();
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                // SwaggerGen won't find controllers that are routed via this technique.
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
