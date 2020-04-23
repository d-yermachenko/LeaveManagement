using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement {
    public static class GlobalizationStartup {

        static GlobalizationStartup() {
            ClassRessourceMappers = new ConcurrentBag<Func<Type, Tuple<string, string>>>();
            InitThisAssemblyDataAnnotationRessources();
        }

        public static ConcurrentBag<Func<Type, Tuple<string, string>>> ClassRessourceMappers { get; private set; }

        /// <summary>
        /// Gets the IStringLocalizer for given type, looking also in custom ClassRessourceMappers
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IStringLocalizer MapRessourceToType(IStringLocalizerFactory factory, Type type) {
            IStringLocalizer result = null;
            Tuple<string, string> knownMapping = null;
            Parallel.For(0, ClassRessourceMappers.Count, (element, state) => {
                try {
                    var mapping = ClassRessourceMappers.ElementAt(element).Invoke(type);
                    if (mapping != null) {
                        knownMapping = mapping;
                        state.Break();
                    }

                }
                catch {
                    ;
                }
            });
            if (knownMapping != null)
                result = factory.Create(knownMapping.Item1, knownMapping.Item2);
            else
                result = factory.Create(type);
            return result;
        }

        private static void InitThisAssemblyDataAnnotationRessources() {

            ClassRessourceMappers.Add(LeaveManagementViewModelDAResourcesConventionMapper);
            ClassRessourceMappers.Add(LeaveManagementControllerResourcesConventionMapper);

        }
        /// <summary>
        /// Gets resource type and assembly name for DataAnnoration translation for type, if type follows the convention of 
        /// ViewModels resources and type is from this assembly.
        /// </summary>
        /// <param name="expectedType">Type to get its resource name</param>
        /// <remarks>
        /// Convention for localizing DataAnnotations for ViewModels of this project is next:
        /// ViewModels' full type name conform to template LeaveManagement.ViewModels.Model.SpecificViewModel.
        /// for exemple, for ViewModel of type LeaveTypeNavigationViewModel, namespace will be next: LeaveManagement.ViewModels.LeaveType;
        /// ViewModels' ressources must be located in path [ResourcesFolder]/ViewModels/Model/ andhave the name Model.[Culture].resx.
        /// for exemple, for DataAnnotations for same LeaveManagement.ViewModels.LeaveType.LeaveTypeNavigationViewModel path 
        /// for resources must be: [ResourcesFolder]\ViewModels\LeaveType\LeaveType.??.resx
        /// </remarks>
        /// <returns>If type is one of conventional ViewModels type, returns Tuple(resourceName, assembmyName). Otherwise, null</returns>
        private static Tuple<string, string> LeaveManagementViewModelDAResourcesConventionMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.Namespace?.Split(new char[] { '.' });
            string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
            bool isTypeCorrespondsToLeaveTypeViewModel = expectedType.Assembly.GetManifestResourceNames().Any(x => x.Contains(resourceName));
            if (isTypeCorrespondsToLeaveTypeViewModel)
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            else
                return null;
        }

        private static Tuple<string, string> LeaveManagementControllerResourcesConventionMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.FullName?.Split(new char[] { '.' });
            string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
            bool isTypeCorrespondsToLeaveTypeViewModel = expectedType.Assembly.GetManifestResourceNames().Any(x => x.Contains(resourceName));
            if (isTypeCorrespondsToLeaveTypeViewModel)
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            else
                return null;
        }


        #region Startup methods
        public static void ConfigureServices(IServiceCollection services) {
            services.AddLocalization(options => {
                options.ResourcesPath = "Resources";
            });
            services.AddMvc()
                .AddViewLocalization(LanguageViewLocationExpanderFormat.SubFolder)
                .AddDataAnnotationsLocalization(options => {
                    options.DataAnnotationLocalizerProvider = (type, factory) => MapRessourceToType(factory, type);
                });
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
