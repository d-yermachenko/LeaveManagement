using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace LeaveManagement {
    public static class GlobalizationStartup {

        #region Startup methods
        public static void ConfigureServices(IServiceCollection services) {
            services.AddLocalization(options => {
                options.ResourcesPath = "Resources";
                });
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder)
                //.AddDataAnnotationsLocalization();
                .AddDataAnnotationsLocalization(options=> {
                    options.DataAnnotationLocalizerProvider = (type, factory) => {
                        var localizer = factory.Create("ViewModels.LeaveType.LeaveTypeNavigationViewModel", 
                            typeof(ViewModels.LeaveType.LeaveTypeNavigationViewModel).Assembly.FullName);
                        return localizer;
                    };
                });
            /*.AddDataAnnotationsLocalization(options=> {
                //var dataAnnotationsRessourcesType = typeof(SharedResources);
                //var assemblyName = new AssemblyName(dataAnnotationsRessourcesType.Assembly.FullName);
                //var factory = services.BuildServiceProvider().GetService<IStringLocalizerFactory>();
                //var dataAnnotationLocalizer = factory.Create("SharedResources", assemblyName.Name);
                options.DataAnnotationLocalizerProvider = (type, factory) => {
                    System.Diagnostics.Trace.WriteLine("Loading resources for: " + type.FullName);
                    System.Diagnostics.Trace.WriteLine("Factory type: " + factory.GetType().FullName);
                    var stringLocalizer = factory.Create(type);
                    //var stringLocalizer = factory.Create("Resources.SharedResources", location: null);
                    var resourceFiles = typeof(ViewModels.LeaveType.LeaveTypeNavigationViewModel).Assembly.GetManifestResourceNames();
                    foreach (var resFile in resourceFiles)
                        System.Diagnostics.Trace.WriteLine(resFile.ToString());

                    return stringLocalizer;
                };
            });*/
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            CultureInfo[] supportedCultures = new CultureInfo[] {
                new CultureInfo("en"),
                new CultureInfo("en-US"),
                new CultureInfo("ru"),
                new CultureInfo("fr")
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
