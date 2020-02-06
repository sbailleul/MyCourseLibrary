using System;
using System.Linq;
using AutoMapper;
using CourseLibrary.API.DbContexts;
using CourseLibrary.API.Services;
using CourseLibrary.API.Services.Interfaces;
using CourseLibrary.API.Services.PropertyMapping;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;

namespace CourseLibrary.API
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
            services.AddHttpCacheHeaders(expirationOptions =>
            {
                expirationOptions.MaxAge = 60;
                expirationOptions.CacheLocation = CacheLocation.Private;
            }, validationOptions => validationOptions.MustRevalidate = true);
            services.AddResponseCaching();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddControllers(action =>
                {
                    action.ReturnHttpNotAcceptable = true;
                    action.CacheProfiles.Add("240SecondCachedProfile", new CacheProfile(){Duration = 240});
                    // action.OutputFormatters.Add(new XmlSerializerOutputFormatter());
                }).AddNewtonsoftJson(action =>
                    action.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver())
                .AddXmlDataContractSerializerFormatters().ConfigureApiBehaviorOptions(
                    action => action.InvalidModelStateResponseFactory = context =>
                    {
                        var problemDetails = new ValidationProblemDetails(context.ModelState)
                        {
                            Type = "https://coureslibrary.com/modelvalidationproblem",
                            Title = "One or more validation error occured",
                            Status = StatusCodes.Status422UnprocessableEntity,
                            Detail = "See the error properties for details",
                            Instance = context.HttpContext.Request.Path
                        };

                        problemDetails.Extensions.Add("traceId", context.HttpContext.TraceIdentifier);
                        return new UnprocessableEntityObjectResult(problemDetails)
                            {ContentTypes = {"application/problem+json"}};
                    });

            services.Configure<MvcOptions>(config =>
            {
                var newtonJsonOutputFormatter =
                    config.OutputFormatters.OfType<NewtonsoftJsonOutputFormatter>()?.FirstOrDefault();
                newtonJsonOutputFormatter?.SupportedMediaTypes.Add("application/vnd.marvin.hateoas+json");
            });
            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<IPropertyCheckerService, PropertyCheckerService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddScoped<ICourseLibraryRepository, CourseLibraryRepository>();

            services.AddDbContext<CourseLibraryContext>(options =>
            {
                options.UseSqlServer(
                    "Data Source=LFR036403\\SQLEXPRESS; Initial Catalog=CourseLibraryDB; Integrated Security=True");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
                app.UseDeveloperExceptionPage();
            // app.UseExceptionHandler(appBuilder =>
            // {
            //     appBuilder.Run(async context =>
            //     {
            //         context.Response.StatusCode = 500;
            //         await context.Response.WriteAsync("An unexpected fault happened. Try again later");
            //     });
            // });

            // app.UseResponseCaching        ();
            app.UseHttpCacheHeaders();
            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}