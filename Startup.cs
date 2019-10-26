using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LandonApi.Filters;
using LandonApi.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using NSwag.AspNetCore;
using LandonApi.Services;
using AutoMapper;
using LandonApi.Infrastructure;
using Newtonsoft.Json;

namespace LandonApi
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
            services.Configure<HotelInfo>(Configuration.GetSection("Info"));
            services.Configure<HotelOptions>(Configuration);

            services.AddScoped<IRoomService, DefaultRoomService>();
            services.AddScoped<IOpeningService, DefaultOpeningService>();
            services.AddScoped<IBookingService, DefaultBookingService>();
            services.AddScoped<IDateLogicService, DefaultDateLogicService>();

            // Use in-memory database for quick dev and testing
            // TODO: Swap out for a real database in production
            services.AddDbContext<HotelApiDbContext>(
                options => options.UseInMemoryDatabase("landondb"));

            services
                .AddMvc(options =>
                {
                    options.Filters.Add<JsonExceptionFilter>();
                    options.Filters
                        .Add<RequireHttpsOrCloseAttribute>();
                    options.Filters.Add<LinkRewritingFilter>();
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(options =>
                {
                    // These should be the defaults, but we can be explicit:
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatHandling = DateFormatHandling.IsoDateFormat;
                    options.SerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;

                });

            services
                .AddRouting(options => options.LowercaseUrls = true);

            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.ApiVersionReader
                    = new MediaTypeApiVersionReader();
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionSelector
                     = new CurrentImplementationApiVersionSelector(options);
            });

            services.AddAutoMapper(
                options => options.AddProfile<MappingProfile>());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                app.UseSwaggerUi3WithApiExplorer(options =>
                {
                    options.GeneratorSettings
                        .DefaultPropertyNameHandling
                    = NJsonSchema.PropertyNameHandling.CamelCase;
                });
            }
            else
            {
                app.UseHsts();
            }

            app.UseMvc();
        }
    }
}
