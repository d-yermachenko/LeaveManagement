using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ResourceAutoCompleter.Translator {
    public class Translator {

        private readonly TranslatorConfiguration _TranslatorConfiguration;

        private readonly ILogger _Logger;

        public Translator(TranslatorConfiguration configuration, ILogger logger) {
            _TranslatorConfiguration = configuration;
            _Logger = logger;
        }

        public async Task<Tuple<string, string>[]> TranslateText(string inputText, string sourceLang = "", string [] toLanguagecodes = null) {
            if(toLanguagecodes == null || toLanguagecodes.Length == 0)
                return new Tuple<string, string>[] { Tuple.Create(sourceLang, inputText)};

            Tuple<string, string>[] result = new Tuple<string, string>[toLanguagecodes.Length];

            string address = _TranslatorConfiguration.EndpointAddress+ _TranslatorConfiguration.TranslationMethodName;

            object[] body = new object[] { new { Text = inputText } };
            var requestBody = JsonConvert.SerializeObject(body);

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage()) {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(address + "?"+ GetQueryString(toLanguagecodes, sourceLang));
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _TranslatorConfiguration.Key);
                request.Headers.Add("Ocp-Apim-Subscription-Region", _TranslatorConfiguration.Region);

                // Send the request and get response.
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                // Read response as a string.
                string translationResponce = await response.Content.ReadAsStringAsync();
                try {
                    TranslationResult[] deserializedOutput = JsonConvert.DeserializeObject<TranslationResult[]>(translationResponce);
                    // Iterate over the deserialized results.
                    if (deserializedOutput.Length != 1)
                        throw new InvalidOperationException("Answer must contain translations to only one phrase");

                    var translations = deserializedOutput.First();
                    for (int i = 0; i < translations.Translations.Length; i++) {
                        Translation translation = translations.Translations[i];
                        result[i] = Tuple.Create(translation.To, translation.Text);
                    }
                }
                catch(Exception) {
                    JObject responce = JsonConvert.DeserializeObject<JObject>(translationResponce);
                    _Logger?.LogError(translationResponce);
                    throw;
                }
                
                
            }
            return result;
        }

        private static string GetQueryString(string[] languages, string fromLanguage = null) {
            List<KeyValuePair<string, string>> queryParams = new List<KeyValuePair<string, string>> {
                new KeyValuePair<string, string>("api-version", "3.0")
            };
            if (!String.IsNullOrWhiteSpace(fromLanguage))
                queryParams.Add(new KeyValuePair<string, string>("from", fromLanguage));
            foreach (string lang in languages) {
                queryParams.Add(new KeyValuePair<string, string>("to", lang));
            }
            StringBuilder queryBuilder = new StringBuilder();
            for(int i = 0; i < queryParams.Count; i++) {
                if (i != 0)
                    queryBuilder.Append('&');
                var pair = queryParams[i];
                queryBuilder.Append($"{pair.Key}={pair.Value}");
            }
            return queryBuilder.ToString();
        } 

    }
}
