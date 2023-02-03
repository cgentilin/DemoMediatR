using DemoMediatR.Application.Core;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using DemoMediatR.Application.Domain.Interfaces;
using DemoMediatR.Application.Repositories;
using Microsoft.OpenApi.Models;
using System.IO;

namespace DemoMediatR.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            AddApplicationServices(services);

            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "DemoMediatR.Api");
                c.RoutePrefix = string.Empty;
            });
        }

        private static void AddApplicationServices(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
            AddMediatr(services);

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MediatR - API Exemplo uso MediatR", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
                var xmlPath = Path.Combine(AppContext.BaseDirectory, "DemoMediatR.Api.xml");
                c.IncludeXmlComments(xmlPath);
            });

        }

        private static void AddMediatr(IServiceCollection services)
        {
            const string applicationAssemblyName = "DemoMediatR.Application";
            var assembly = AppDomain.CurrentDomain.Load(applicationAssemblyName);

            AssemblyScanner
                .FindValidatorsInAssembly(assembly)
                .ForEach(result => services.AddScoped(result.InterfaceType, result.ValidatorType));

            services.AddScoped(typeof(IPipelineBehavior<,>), typeof(FailFastRequestBehavior<,>));

            services.AddMediatR();
        }
    }
}
