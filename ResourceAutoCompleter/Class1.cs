using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Resources;

namespace ResourceAutoCompleter {
    public class AutoCompleterStringLocalizer: IStringLocalizer  {
        private readonly IStringLocalizer _RealLocalizer;
        private readonly IConfiguration _Configuration;
        private readonly CultureInfo _Culture;
        private readonly string _ResourceName;

        public AutoCompleterStringLocalizer(
            IStringLocalizer realLocalizer,
            IConfiguration configuration,
            CultureInfo culture,
            string resourceName) {
            _RealLocalizer = realLocalizer;
            _Configuration = configuration;
            _Culture = culture;
            _ResourceName = resourceName;
        }

        public LocalizedString this[string name] => throw new NotImplementedException();

        public LocalizedString this[string name, params object[] arguments] => throw new NotImplementedException();

        public IEnumerable<LocalizedString> GetAllStrings(bool includeParentCultures) {
            throw new NotImplementedException();
        }

        public IStringLocalizer WithCulture(CultureInfo culture) {
            throw new NotImplementedException();
        }

        


    }
}
