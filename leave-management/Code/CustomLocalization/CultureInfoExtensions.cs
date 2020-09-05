using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LeaveManagement.CustomLocalization {
    public static class CultureInfoExtensions {
        /// <summary>
        /// Formats culture info to show flag from the library of flagIcons 
        /// </summary>
        /// <param name="cultureInfo"></param>
        /// <param name="template">Template of class. Must contain part {0}, where this method will put the country code, if not contains, it will return this string</param>
        /// <returns></returns>
        public static string GetFlagIconsCountryCode(this CultureInfo cultureInfo, string template) {
            if (!template.Contains("{0}"))
                return template;
            if (cultureInfo.IsNeutralCulture) {
                string languageCode = cultureInfo.TwoLetterISOLanguageName.ToLower();
                if (languageCode.Equals("en"))
                    languageCode = "gb"; // workaround for great britain
                return String.Format(template, languageCode);
            }

            else
                return String.Format(template, new RegionInfo(cultureInfo.LCID).TwoLetterISORegionName.ToLower());
        }
    }
}
