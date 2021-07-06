using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
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
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }
        private static List<ApiVersionDescription> _apiVersions { get; set; }
        private const string ApiName = "Social Care Case Viewer API";

        // This method gets called by the runtime. Use this method to add services to the container.
        public static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddFluentValidation();
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
                foreach (var apiVersion in _apiVersions)
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


            services.AddHttpClient<ISocialCarePlatformAPIGateway, SocialCarePlatformAPIGateway>(client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_URL"));
            });

            services.AddTransient<IValidator<CreateAllocationRequest>, CreateAllocationRequestValidator>();
            services.AddTransient<IValidator<UpdateAllocationRequest>, UpdateAllocationRequestValidator>();
            services.AddTransient<IValidator<CreateWorkerRequest>, CreateWorkerRequestValidator>();
            services.AddTransient<IValidator<PatchWarningNoteRequest>, PatchWarningNoteRequestValidator>();
            services.AddTransient<IValidator<CreateTeamRequest>, CreateTeamRequestValidator>();
            services.AddTransient<IValidator<GetTeamsRequest>, GetTeamsRequestValidator>();
            services.AddTransient<IValidator<CreateCaseSubmissionRequest>, CreateCaseSubmissionRequestValidator>();
            services.AddTransient<IValidator<UpdateCaseSubmissionRequest>, UpdateCaseSubmissionRequestValidator>();
            services
                .AddTransient<IValidator<UpdateFormSubmissionAnswersRequest>, UpdateFormSubmissionAnswersValidator>();
            services.AddTransient<IValidator<CreatePersonalRelationshipRequest>, CreatePersonalRelationshipRequestValidator>();

            services.AddScoped<ISystemTime, SystemTime>();
        }

        private static void ConfigureDbContext(IServiceCollection services)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING");

            services.AddDbContext<DatabaseContext>(
                opt => opt.UseNpgsql(connectionString));

            services.AddSingleton<ISccvDbContext, SccvDbContext>();
        }

        private static void RegisterGateways(IServiceCollection services)
        {
            services.AddScoped<IDatabaseGateway, DatabaseGateway>();
            services.AddScoped<IProcessDataGateway, ProcessDataGateway>();
            services.AddScoped<IMosaicAPIGateway, MosaicAPIGateway>();
            services.AddScoped<ISocialCarePlatformAPIGateway, SocialCarePlatformAPIGateway>();
            services.AddScoped<IMongoGateway, MongoGateway>();
        }

        private static void RegisterUseCases(IServiceCollection services)
        {
            services.AddScoped<IGetAllUseCase, GetAllUseCase>();
            services.AddScoped<IAddNewResidentUseCase, AddNewResidentUseCase>();
            services.AddScoped<ICaseRecordsUseCase, CaseRecordsUseCase>();
            services.AddScoped<IAllocationsUseCase, AllocationsUseCase>();
            services.AddScoped<ITeamsUseCase, TeamsUseCase>();
            services.AddScoped<ICaseNotesUseCase, CaseNotesUseCase>();
            services.AddScoped<IVisitsUseCase, VisitsUseCase>();
            services.AddScoped<IWarningNoteUseCase, WarningNoteUseCase>();
            services.AddScoped<IGetVisitByVisitIdUseCase, GetVisitByVisitIdUseCase>();
            services.AddScoped<IWorkersUseCase, WorkersUseCase>();
            services.AddScoped<IPersonUseCase, PersonUseCase>();
            services.AddScoped<IFormSubmissionsUseCase, FormSubmissionsUseCase>();
            services.AddScoped<IRelationshipsUseCase, RelationshipsUseCase>();
            services.AddScoped<IPersonalRelationshipsUseCase, PersonalRelationshipsUseCase>();
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
            _apiVersions = api.ApiVersionDescriptions.ToList();

            //Swagger ui to view the swagger.json file
            app.UseSwaggerUI(c =>
            {
                foreach (var apiVersionDescription in _apiVersions)
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
