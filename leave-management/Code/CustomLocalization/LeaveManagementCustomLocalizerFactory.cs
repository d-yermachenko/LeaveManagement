using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.Code.CustomLocalization {
    public class LeaveManagementCustomLocalizerFactory : ILeaveManagementCustomLocalizerFactory {

        #region Private fields and constructor
        private readonly IStringLocalizerFactory StringLocalizerFactory;

        private readonly IHtmlLocalizerFactory HtmlLocalizerFactory;

        private readonly IWebHostEnvironment WebHostEnvironment;

        public LeaveManagementCustomLocalizerFactory(IStringLocalizerFactory stringLocalizerFactory,
        IHtmlLocalizerFactory htmlLocalizerFactory,
        IWebHostEnvironment hostEnvironement = null) {
            StringLocalizerFactory = stringLocalizerFactory;
            HtmlLocalizerFactory = htmlLocalizerFactory;
            WebHostEnvironment = hostEnvironement;
            InitThisAssemblyDataAnnotationRessources();
        }
        #endregion

        #region Code for initialize rules



        public ConcurrentBag<Func<Type, Tuple<string, string>>> ClassRessourceMappers { get; private set; }

        /// <summary>
        /// Gets the IStringLocalizer for given type, looking also in custom ClassRessourceMappers
        /// </summary>
        /// <param name="factory"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public IStringLocalizer MapRessourceToType(IStringLocalizerFactory factory, Type type) {
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

        private void InitThisAssemblyDataAnnotationRessources() {
            ClassRessourceMappers = new ConcurrentBag<Func<Type, Tuple<string, string>>>();
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
        private Tuple<string, string> LeaveManagementViewModelDAResourcesConventionMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.Namespace?.Split(new char[] { '.' });
            string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
            bool isTypeCorrespondsToLeaveTypeViewModel = expectedType.Assembly.GetManifestResourceNames().Any(x => x.Contains(resourceName));
            if (isTypeCorrespondsToLeaveTypeViewModel)
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            else
                return null;
        }

        private Tuple<string, string> LeaveManagementControllerResourcesConventionMapper(Type expectedType) {
            string[] nameSpacePath = expectedType.FullName?.Split(new char[] { '.' });
            string resourceName = String.Concat(string.Join('.', nameSpacePath.TakeLast(nameSpacePath.Length - 1)), ".", nameSpacePath.Last());
            bool isTypeCorrespondsToLeaveTypeViewModel = expectedType.Assembly.GetManifestResourceNames().Any(x => x.Contains(resourceName));
            if (isTypeCorrespondsToLeaveTypeViewModel)
                return Tuple.Create(resourceName, expectedType.Assembly.FullName);
            else
                return null;
        }

        #endregion



        public IHtmlLocalizer CreateHtmlLocalizer(Type type) {
            throw new NotImplementedException();
        }

        public IHtmlLocalizer CreateHtmlLocalizer(string baseName, string location) {
            throw new NotImplementedException();
        }

        public IStringLocalizer CreateStringLocalizer(Type type) => MapRessourceToType(StringLocalizerFactory, type);

        public IStringLocalizer CreateStringLocalizer(string baseName, string location) {
            throw new NotImplementedException();
        }
    }
}
