﻿using AppText.AdminApp.Controllers;
using AppText.Configuration;
using AppText.Shared.Infrastructure.Mvc;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System;
using System.Reflection;

namespace AppText.AdminApp.Configuration
{
    public static class AppTextBuilderExtensions
    {
        public static AppTextBuilder AddAdmin(this AppTextBuilder appTextBuilder, Action<AppTextAdminConfigurationOptions> setupAction = null)
        {
            var mvcBuilder = appTextBuilder.Services.AddMvcCore();
            var assembly = typeof(AdminController).Assembly;
            mvcBuilder.AddApplicationPart(assembly);

            var options = GetOptions(appTextBuilder.Services, setupAction);

            ConfigureServices(appTextBuilder.Services, assembly, options);

            mvcBuilder.AddMvcOptions(mvcOptions =>
            {
                mvcOptions.Conventions.Insert(0, new AppTextRouteConvention(options.RoutePrefix, assembly));
                mvcOptions.Conventions.Add(new AppTextAuthorizationConvention(
                    options.RequireAuthenticatedUser.HasValue ? options.RequireAuthenticatedUser.Value : false,
                    options.RequiredAuthorizationPolicy,
                    assembly));
            });

            return appTextBuilder;
        }

        private static AppTextAdminConfigurationOptions GetOptions(
            IServiceCollection services,
            Action<AppTextAdminConfigurationOptions> setupAction = null
        )
        {
            var enrichOptions = setupAction ?? delegate { };
            var options = new AppTextAdminConfigurationOptions();

            enrichOptions(options);

            // Try to set empty options from AppText API configuration
            var serviceProvider = services.BuildServiceProvider();
            var appTextConfiguration = serviceProvider.GetService<AppTextPublicConfiguration>();
            if (appTextConfiguration != null)
            {
                if (String.IsNullOrEmpty(options.RoutePrefix))
                {
                    options.RoutePrefix = appTextConfiguration.RoutePrefix;
                }
                if (!options.RequireAuthenticatedUser.HasValue)
                {
                    options.RequireAuthenticatedUser = appTextConfiguration.RequireAuthenticatedUser;
                }
                if (options.RequiredAuthorizationPolicy == null)
                {
                    options.RequiredAuthorizationPolicy = appTextConfiguration.RequiredAuthorizationPolicy;
                }
            }

            // Register options as singleton
            services.AddSingleton(options);

            return options;
        }

        private static void ConfigureServices(IServiceCollection services, Assembly assembly, AppTextAdminConfigurationOptions options)
        {
            // Register EmbeddedFileProvider for views
            services.Configure<MvcRazorRuntimeCompilationOptions>(razorOptions =>
            {
                razorOptions.FileProviders.Add(new EmbeddedFileProvider(assembly));
            });

            // Register Embedded Static Files provider
            services.ConfigureOptions(typeof(EmbeddedStaticFilesOptions));
        }
    }
}