using ResourceAutoCompleter.Translator;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceAutoCompleter {
    public class AutocompleteResourceConfiguration {
        public string Folder { get; set; }

        public bool TranslateResources { get; set; }

        public string ExcludeCultures { get; set; }


        public TranslatorConfiguration TranslatorConfiguration { get; set; } = null;
    }
}
