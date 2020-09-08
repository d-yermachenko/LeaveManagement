using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Resources;

namespace ResourceAutoCompleter {
    public class AutoCompleterStringLocalizer : IStringLocalizer {
        private readonly IStringLocalizer _RealLocalizer;
        private readonly CultureInfo _Culture;
        private readonly string _ResourceName;
        private readonly AutocompleteResourceConfiguration _ResourceConfiguration;
        private readonly Translator.Translator _ResourcesTranslator;
        private readonly ILogger _Logger;

        public AutoCompleterStringLocalizer(
            IStringLocalizer realLocalizer,
            CultureInfo culture,
            string resourceName,
            AutocompleteResourceConfiguration configuration,
            ILogger logger) {
            _RealLocalizer = realLocalizer;
            _Culture = culture;
            _ResourceName = resourceName;
            _ResourceConfiguration = configuration;
            _Logger = logger;
            if ((_ResourceConfiguration?.TranslateResources ?? false) && _ResourceConfiguration.TranslatorConfiguration != null)
                _ResourcesTranslator = new Translator.Translator(_ResourceConfiguration.TranslatorConfiguration, _Logger);
        }


        public LocalizedString this[string name] {
            get {
                var result = _RealLocalizer[name];
                if (result.ResourceNotFound)
                    WriteNotFoundString(name, result.Value, result.Value);
                return result;
            }

        }

        private string GetResourcePath() {
            string resourceFileName = $"{_ResourceName}.{_Culture.Name}.csv";
            return System.IO.Path.Combine(_ResourceConfiguration.Folder, resourceFileName);

        }

        private void WriteNotFoundString(string name, string value, string comment) {
            string[] excludedCultures = _ResourceConfiguration.ExcludeCultures?.Split(new char[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (excludedCultures.Contains(_Culture.Name))
                return;
            string resourcesPath = GetResourcePath();
            List<Tuple<string, string, string>> entries;
            if (System.IO.File.Exists(resourcesPath))
                entries = ReadResxData(resourcesPath);
            else
                entries = new List<Tuple<string, string, string>>();
            if (!entries.Any(ent => ent.Item1 == name)) {
                if(!(_Culture?.Name.StartsWith("en")??true) && _ResourceConfiguration.TranslateResources && _ResourcesTranslator != null) {
                    try {
                        var translationData = _ResourcesTranslator.TranslateText(value, "en", new string[] { _Culture.TwoLetterISOLanguageName.ToLower() });
                        value = translationData.Result.First().Item2;
                    }
                    catch (Exception) {
                        ;
                    }
                }
                entries.Add(new Tuple<string, string, string>(name, value, comment));
            }
            WriteResXData(resourcesPath, entries);
        }

        private List<Tuple<string, string, string>> ReadResxData(string fileName) {
            List<Tuple<string, string, string>> localisationDatas = new List<Tuple<string, string, string>>();
            try {

                string[] resourcesLines = System.IO.File.ReadAllLines(fileName);
                foreach (string resData in resourcesLines) {
                    if (!String.IsNullOrWhiteSpace(resData)) {
                        string[] resDatas = resData.Split(new char[] { '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        if (resDatas.Length == 3)
                            localisationDatas.Add(new Tuple<string, string, string>(resDatas[0], resDatas[1], resDatas[2]));
                    }
                }
            }
            catch (FileNotFoundException fe) {
                ;
            }
            catch (Exception ce) {
                ;
            }
            return localisationDatas;

        }

        private void WriteResXData(string fileName, List<Tuple<string, string, string>> tuples) {
            string[] entries = new string[tuples.Count];
            for (int i = 0; i < tuples.Count; i++) {
                entries[i] = $"{tuples[i].Item1}\t{tuples[i].Item2}\t{tuples[i].Item3}";
            }
            File.WriteAllLines(fileName, entries);
        }


        public LocalizedString this[string name, params object[] arguments] {
            get {
                var result = _RealLocalizer[name, arguments];
                if (result.ResourceNotFound)
                    WriteNotFoundString(name, result.Name, result.Value);
                return result;
            }
        }

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) {
            return _RealLocalizer.GetAllStrings(includeParentCultures);
        }

        [Obsolete]
        public IStringLocalizer WithCulture(CultureInfo culture) => _RealLocalizer.WithCulture(culture);
    }
}
