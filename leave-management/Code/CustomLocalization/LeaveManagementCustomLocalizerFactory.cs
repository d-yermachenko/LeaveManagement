using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;
using LeaveManagement.Data.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using ResourceAutoCompleter;
using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace LeaveManagement.Code.CustomLocalization {
    public class LeaveManagementCustomLocalizerFactory : ILeaveManagementCustomLocalizerFactory {

        #region Root namespaces for different parts of applications
        public const string ViewModelsRootNamespace = "ViewModels";
        public const string ControllersRootNameSpace = "Controllers";

        #endregion

        #region Private fields and constructor
        private readonly IStringLocalizerFactory StringLocalizerFactory;

        private readonly IHtmlLocalizerFactory HtmlLocalizerFactory; //For possible future ViewLocalizer
        private readonly IWebHostEnvironment WebHostEnvironment; //For possible future ViewLocalizer
        private readonly Microsoft.Extensions.Configuration.IConfiguration _Configuration;
        private readonly ILogger<LeaveManagementCustomLocalizerFactory> _Logger;

        public LeaveManagementCustomLocalizerFactory(
            IStringLocalizerFactory stringLocalizerFactory,
            IHtmlLocalizerFactory htmlLocalizerFactory,
            Microsoft.Extensions.Configuration.IConfiguration configuration,
            ILogger<LeaveManagementCustomLocalizerFactory> logger,
            IWebHostEnvironment hostEnvironement = null
        ) {
            StringLocalizerFactory = stringLocalizerFactory;
            HtmlLocalizerFactory = htmlLocalizerFactory;
            WebHostEnvironment = hostEnvironement;
            _Configuration = configuration;
            _Logger = logger;
            InitMappers();
        }
        #endregion

        #region Code for initialize rules


        /// <summary>
        /// List of mappers from type to corresponded resource name.
        /// Mapper gets the type and returns 
        /// </summary>
        public ConcurrentBag<Func<Type, Tuple<string, string>>> ConventionalResourceMappers { get; private set; }

        /// <summary>
        /// Gets the IStringLocalizer for given type, looking also in custom ClassRessourceMappers
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        protected IStringLocalizer MapRessourceToType(IStringLocalizerFactory factory, Type type) {
            IStringLocalizer result = null;
            Tuple<string, string> knownMapping = null;
            Parallel.For(0, ConventionalResourceMappers.Count, (element, state) => {
                try {
                    var mapping = ConventionalResourceMappers.ElementAt(element).Invoke(type);
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
                result = CreateLocalizer(knownMapping.Item1, knownMapping.Item2, factory);
            else
                result = factory.Create(type);
            return result;
        }


        protected IStringLocalizer CreateLocalizer(string baseName, string assembly, IStringLocalizerFactory factory) {
            IStringLocalizer realLocalizer = factory.Create(baseName, assembly);
            if (!(Environment.GetEnvironmentVariable("ResourcesDebug")?.Equals("ResourcesDebug") ??false))
                return realLocalizer;
            AutocompleteResourceConfiguration configuration = _Configuration.GetSection(nameof(AutocompleteResourceConfiguration)).Get<AutocompleteResourceConfiguration>();
            IStringLocalizer debugLocalizer = new AutoCompleterStringLocalizer(realLocalizer, CultureInfo.CurrentCulture,
                baseName, configuration, _Logger);
            return debugLocalizer;
        }

        protected IStringLocalizer CreateLocalizer(Type type, IStringLocalizerFactory factory) {
            IStringLocalizer realLocalizer = factory.Create(type);
            if (!(Environment.GetEnvironmentVariable("ResourcesDebug")?.Equals("ResourcesDebug") ?? false))
                return realLocalizer;
            AutocompleteResourceConfiguration configuration = _Configuration.GetSection(nameof(AutocompleteResourceConfiguration)).Get<AutocompleteResourceConfiguration>();
            IStringLocalizer debugLocalizer = new AutoCompleterStringLocalizer(realLocalizer, CultureInfo.CurrentCulture,
                type.FullName, configuration, _Logger);
            return debugLocalizer;
        }

        private void InitMappers() {
            if (ConventionalResourceMappers == null) {
                ConventionalResourceMappers = new ConcurrentBag<Func<Type, Tuple<string, string>>> {
                    LeaveManagementViewModelDataAnnotationsMapper,
                    LeaveManagementControllerMapper,
                    LeaveManagementIdentityMapper
                };
            }
        }


        #endregion

        #region Type to resource mappers

        /// <summary>
        /// Gets resource type and assembly name for DataAnnoration translation for type, if type follows the convention of 
        /// ViewModels resources and type is from this assembly.
        /// </summary>
        /// <param name="expectedType">Type to get its resource name</param>
        /// <remarks>
        /// Convention for localizing DataAnnotations for ViewModels of this project is next:
        /// ViewModels' full type name conform to template LeaveManagement.ViewModels.Model.SpecificViewModel.
        /// for exemple, for ViewModel of type LeaveTypeNavigationViewModel, namespace will be next: LeaveManagement.ViewModels.LeaveType;
        /// ViewModels' ressources must be located in path [ResourcesFolder]/ViewModels/Model/ and have the name Model-Name.[Culture].resx.
        /// for exemple, for DataAnnotations for same LeaveManagement.ViewModels.LeaveType.LeaveTypeNavigationViewModel path 
        /// for resources must be: [ResourcesFolder]\ViewModels\LeaveType\LeaveType.??.resx
        /// </remarks>
        /// <returns>If type is one of conventional ViewModels type, returns Tuple(resourceName, assembmyName). Otherwise, null</returns>
        private Tuple<string, string> LeaveManagementViewModelDataAnnotationsMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.Namespace?.Split(new char[] { '.' });
            if (nameSpacePath.Length > 2 && nameSpacePath.ElementAt(1).Equals(ViewModelsRootNamespace)) {
                string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            }
            else
                return null;
        }

        /// <summary>
        /// Gets resource type and assembly for localizing controller messages.
        /// </summary>
        /// <param name="expectedType"></param>
        /// <returns></returns>
        private Tuple<string, string> LeaveManagementControllerMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.FullName?.Split(new char[] { '.' });
            if (nameSpacePath.Length > 2 && nameSpacePath.ElementAt(1).Equals(ControllersRootNameSpace)) {
                string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            }
            else
                return null;
        }

        private Tuple<string, string> LeaveManagementIdentityMapper (Type expectedType) {
            Tuple<string, string> result = null;
            string[] nameSpacePath = expectedType.FullName?.Split(new char[] { '.' });
            if (nameSpacePath.Any(x => x.Equals("Identity"))) {
                result = Tuple.Create("Areas.Identity.IdentityInputModel", this.GetType().Assembly.FullName);
            }
            return result;
        }

        #endregion


        public IStringLocalizer Create(Type type) => MapRessourceToType(StringLocalizerFactory, type);

        public IStringLocalizer Create(string baseName, string location) => CreateLocalizer(baseName, location, StringLocalizerFactory);


        private IStringLocalizer CommandsLocalizerField = null;

        public IStringLocalizer CommandsLocalizer {
            get {
                if (CommandsLocalizerField == null)
                    CommandsLocalizerField = CreateLocalizer("LeaveManagement.CommandsLocalizer.CommandLocalization", this.GetType().Assembly.FullName, StringLocalizerFactory);
                return CommandsLocalizerField;
            }
        }


        private IStringLocalizer MenuLocalizerField = null;
        public IStringLocalizer MenuLocalizer {
            get {
                if(MenuLocalizerField == null)
                    MenuLocalizerField = CreateLocalizer("LeaveManagement.MenuLocalizer.MenuLocalizer", this.GetType().Assembly.FullName, StringLocalizerFactory);
                return MenuLocalizerField;
            }
        }

        
        private IHtmlLocalizer HtmlIdentityLocalizerField;

        public IHtmlLocalizer HtmlIdentityLocalizer {
            get {
                if (HtmlIdentityLocalizerField == null)
                    HtmlIdentityLocalizerField = HtmlLocalizerFactory.Create("LeaveManagement.IdentityLocalizer.IdentityLocalizer", this.GetType().Assembly.FullName);
                return HtmlIdentityLocalizerField;
            }
        }

        private IStringLocalizer StringIdentityLocalizerField;

        public IStringLocalizer StringIdentityLocalizer {
            get {
                if (StringIdentityLocalizerField == null)
                    StringIdentityLocalizerField = StringLocalizerFactory.Create("LeaveManagement.IdentityLocalizer.IdentityLocalizer", this.GetType().Assembly.FullName);
                return StringIdentityLocalizerField;
            }
        }
    }
}
