﻿// ------------------------------------------------------------------------------
//     <copyright file="Startup.cs" company="BlackLine">
//         Copyright (C) BlackLine. All rights reserved.
//     </copyright>
// ------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using System.Net;

using AspNetCoreRateLimit;

using Library.API.Entities;
using Library.API.Enums;
using Library.API.Helpers;
using Library.API.Models;
using Library.API.Services;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json.Serialization;

using NLog.Extensions.Logging;

namespace Library.API
{
    public class Startup
    {
        public static IConfiguration Configuration;

        public Startup(IConfiguration configuration)
        {
            Startup.Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, LibraryContext libraryContext)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddDebug(LogLevel.Information);

            //loggerFactory.AddProvider(new NLogLoggerProvider());

            loggerFactory.AddNLog();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler(
                    appBuilder =>
                    {
                        appBuilder.Run(
                            async context =>
                            {
                                var exceptionHandlerFeature = context.Features.Get<IExceptionHandlerFeature>();

                                if (exceptionHandlerFeature != null)
                                {
                                    ILogger logger = loggerFactory.CreateLogger("Global exception logger");
                                    logger.LogError((int)HttpStatusCode.InternalServerError, exceptionHandlerFeature.Error, exceptionHandlerFeature.Error.Message);
                                }

                                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                                await context.Response.WriteAsync("An unexpected fault happened. Try again later.");
                            });
                    });
            }

            AutoMapper.Mapper.Initialize(
                cfg =>
                {
                    cfg.CreateMap<Author, AuthorDto>().ForMember(dest => dest.Name, opt => opt.MapFrom(src => $"{src.FirstName} {src.LastName}"))
                       .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.GetCurrentAge(src.DateOfDeath)));

                    cfg.CreateMap<Book, BookDto>();

                    cfg.CreateMap<AuthorForCreationDto, Author>();
                    cfg.CreateMap<AuthorForCreationWithDateOfDeathDto, Author>();

                    cfg.CreateMap<BookForCreationDto, Book>();

                    cfg.CreateMap<BookForUpdateDto, Book>();
                    cfg.CreateMap<Book, BookForUpdateDto>();
                });

            libraryContext.EnsureSeedDataForContext();

            //app.UseIpRateLimiting();

            app.UseResponseCaching();

            app.UseHttpCacheHeaders();

            app.UseMvc();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(
                setupAction =>
                { 
                    setupAction.ReturnHttpNotAcceptable = true;
                    setupAction.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter());

                    var xmlDataContractSerializerInputFormatter = new XmlDataContractSerializerInputFormatter();
                    xmlDataContractSerializerInputFormatter.SupportedMediaTypes.Add(AcceptMediaTypes.MarvinHateoasWithAuthorFullPlusXml);
                    setupAction.InputFormatters.Add(xmlDataContractSerializerInputFormatter);

                    JsonOutputFormatter jsonOutputFormatter = setupAction.OutputFormatters.OfType<JsonOutputFormatter>().FirstOrDefault();

                    if (jsonOutputFormatter != null)
                    {
                        jsonOutputFormatter.SupportedMediaTypes.Add(AcceptMediaTypes.MarvinHateoasPlusJson);
                        jsonOutputFormatter.SupportedMediaTypes.Add(AcceptMediaTypes.MarvinHateoasWithAuthorFullPlusJson);
                        jsonOutputFormatter.SupportedMediaTypes.Add(AcceptMediaTypes.MarvinHateoasWithDateOfDeathPlusJson);
                    }

                }).AddJsonOptions(
                options =>
                {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                });

            // register the DbContext on the container, getting the connection string from
            // appSettings (note: use this during development; in a production environment,
            // it's better to store the connection string in an environment variable)
            string connectionString = Startup.Configuration["connectionStrings:libraryDBConnectionString"];
            services.AddDbContext<LibraryContext>(o => o.UseSqlServer(connectionString));

            // register the repository
            services.AddScoped<ILibraryRepository, LibraryRepository>();

            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            services.AddScoped<IUrlHelper, UrlHelper>(
                implementationFactory =>
                {
                    ActionContext actionContext = implementationFactory.GetService<IActionContextAccessor>().ActionContext;
                    return new UrlHelper(actionContext);
                });

            services.AddTransient<IPropertyMappingService, PropertyMappingService>();
            services.AddTransient<ITypeHelperService, TypeHelperService>();

            services.AddHttpCacheHeaders(
                (expirationModelOptions) =>
                {
                    expirationModelOptions.MaxAge = 600;
                },
                (validationModelOptions) =>
                {
                    validationModelOptions.MustRevalidate = true;
                });

            services.AddResponseCaching();

            services.AddMemoryCache();

            services.Configure<IpRateLimitOptions>(
                (options) =>
                {
                    options.GeneralRules = new List<RateLimitRule>
                    {
                        new RateLimitRule
                        {
                            Endpoint = "*",
                            Limit = 3,
                            Period = "5m"
                        }
                    };
                });

            services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
            services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
        }
    }
}