
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Infrastructure;
using Core.Interfaces;
using System.Threading.Tasks;
using Infrastructure.Repositories;
using Microsoft.OpenApi.Models;
using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using DissertationMSSQLEF.Controllers;
using Prometheus;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace DissertationMSSQLEF
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
            #region Authentication
            services.AddAuthentication(o =>
            {
                o.DefaultScheme = SchemesNamesConst.TokenAuthenticationDefaultScheme;
            })
            .AddScheme<TokenAuthenticationOptions, TokenAuthenticationHandler>(SchemesNamesConst.TokenAuthenticationDefaultScheme, 
            o => {
            }
            );
            #endregion

            services.AddAuthorization(options =>
            {
                options.AddPolicy("ValidateBearerToken", policy =>
                {
                    policy.Requirements.Add(new ValidateBearerToken());
                });
            });
            services.AddSingleton<IAuthorizationHandler, ValidateTokenHandler>();
            var options = new TokenAuthenticationOptions()
            {
                Instance = Configuration.GetSection("AzureAd:Instance").Value,
                TenantId = Configuration.GetSection("AzureAd:TenantId").Value,
                ClientId = Configuration.GetSection("AzureAd:ClientId").Value
            };
            var azureSection = Configuration.GetSection("Azure");

            services.AddOptions<TokenAuthenticationOptions>("AzureAd").Bind(azureSection);
            services.Configure<TokenAuthenticationOptions>(Configuration.GetSection("Azure"));
            services.AddHttpContextAccessor();
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddDbContext<CourseContext>(opt =>
                         opt.UseSqlServer(Configuration.GetConnectionString("DissertationDatabase"),
                         sqlServerOptionsAction: sqlOptions =>
                         {
                             sqlOptions.EnableRetryOnFailure();
                         }));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ITaskRepository, TaskRepository>();

            services.AddCors(options =>
            {
                options.AddPolicy("CorsApi",
                    builder => builder.AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services.AddControllers();

            var portUsed = Environment.GetEnvironmentVariable("ASPNETCORE_URLS");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1",
                    new OpenApiInfo
                    {
                        Title = "Dissertation Secure Web API",
                        Version = "v1",
                        Description = "Running on Port: " + portUsed,
                    });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = @"JWT Authorization header using the Bearer scheme. 
                      Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer '",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                    }
                });

            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseMetricServer("/metrics");
            app.UseHttpMetrics();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseCors("CorsApi");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseMiddleware<MyErrorHandling>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
    public class MyErrorHandling
    {
        private readonly RequestDelegate _next;

        public MyErrorHandling(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next.Invoke(context);
            }
            catch (Exception e)
            {
                // Do stuff?
                await context.Response.WriteAsync("it broke. :(");
            }
        }
    }
    public static class SchemesNamesConst
    {
        public const string TokenAuthenticationDefaultScheme = "TokenAuthenticationScheme";
    }
}
