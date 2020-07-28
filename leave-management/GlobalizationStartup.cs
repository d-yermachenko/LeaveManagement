﻿using LeaveManagement.Code.CustomLocalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Linq;

namespace LeaveManagement {
    public static class GlobalizationStartup {

        #region Startup methods
        public static void ConfigureServices(IServiceCollection services) {
            services.AddLocalization(options => {
                options.ResourcesPath = "Resources";
            });
            services.AddTransient<ILeaveManagementCustomLocalizerFactory, LeaveManagementCustomLocalizerFactory>();
            services.AddMvc()
            .AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder)
            .AddDataAnnotationsLocalization();
        }



        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            CultureInfo[] supportedCultures = new CultureInfo[] {
                new CultureInfo("en"),
                new CultureInfo("en-US"),
                new CultureInfo("ru"),
                new CultureInfo("fr")
            };

            var localizationService = app.ApplicationServices.GetRequiredService<ILeaveManagementCustomLocalizerFactory>();
            var options1 = app.ApplicationServices.GetService<IOptions<MvcDataAnnotationsLocalizationOptions>>();


            options1.Value.DataAnnotationLocalizerProvider = (type, function) => {
                return localizationService.CreateStringLocalizer(type);
            };

            IRequestCultureProvider[] cultureProviders = new IRequestCultureProvider[] {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };

            app.UseRequestLocalization(new RequestLocalizationOptions {
                DefaultRequestCulture = new RequestCulture("en-US"),
                RequestCultureProviders = cultureProviders.ToList(),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            }); ;

        }
        #endregion
    }
}
