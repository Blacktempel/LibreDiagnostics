/*
* This Source Code Form is subject to the terms of the Mozilla Public
* License, v. 2.0. If a copy of the MPL was not distributed with this
* file, You can obtain one at https://mozilla.org/MPL/2.0/.
*
* Copyright (c) 2025 Florian K.
*
*/

using System.Globalization;

namespace LibreDiagnostics.Language
{
    public static class Culture
    {
        #region Fields

        public const string DEFAULT = "en-US";

        static readonly IReadOnlyDictionary<string, string> _LanguageAliases = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "zh-Hans-CN", "zh-CN" },
            { "zh-Hant-TW", "zh-TW" },
        };

        #endregion

        #region Properties

        public static string[] Languages
        {
            get
            {
                return
                [
                    "de-DE",
                    DEFAULT,
                    "es-ES",
                    //"fi-FI",
                    //"fr-FR",
                    //"it-IT",
                    "ja-JP",
                    //"ru-RU",
                    "zh-Hans-CN",
                    "zh-Hant-TW",
                ];
            }
        }

        #endregion

        #region Public

        public static void SetCurrent(string language)
        {
            language = TryGetCultureAlias(language);
            var culture = new CultureInfo(language);

            Resources.Resources.Culture = culture;

            Thread.CurrentThread.CurrentCulture   = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }

        public static List<CultureItem> GetAll()
        {
            var cultures = GetCultures()
                .OrderBy(ci => ci.DisplayName)
                .Select(ci => new CultureItem { Text = ci.DisplayName, Value = ci.Name });

            return cultures.ToList();
        }

        #endregion

        #region Private

        static IEnumerable<CultureInfo> GetCultures()
        {
            return CultureInfo.GetCultures(CultureTypes.SpecificCultures)
                .Where(ci =>
                {
                    //Check default list first
                    if (Languages.Any(l => string.Equals(ci.Name, l, StringComparison.OrdinalIgnoreCase)))
                    {
                        return true;
                    }

                    //Check aliases
                    if (_LanguageAliases.TryGetValue(ci.Name, out var alias))
                    {
                        return true;
                    }

                    return false;
                });
        }

        static string TryGetCultureAlias(string cultureName)
        {
            //Return alias if exists
            if (_LanguageAliases.TryGetValue(cultureName, out var alias))
            {
                return alias;
            }

            //Return original if no alias exists
            return cultureName;
        }

        #endregion
    }
}
