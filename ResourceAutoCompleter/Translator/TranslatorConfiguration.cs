using System;
using System.Collections.Generic;
using System.Text;

namespace ResourceAutoCompleter.Translator {
    public class TranslatorConfiguration {

        public string EndpointAddress { get; set; }

        public string TranslationMethodName { get; set; }

        public string Key { get; set; }

        public string Region { get; set; }

        public static TranslatorConfiguration Default => new TranslatorConfiguration() {
            Key = string.Empty,
            EndpointAddress = "https://api.cognitive.microsofttranslator.com/",
            TranslationMethodName = "/translate"
        };

    }
}
