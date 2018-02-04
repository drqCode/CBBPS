using System.Collections.Generic;

namespace BranchPredictionSimulator.Localization
{
    public class LanguageInfo
    {
        private static Dictionary<string, LanguageInfo> languagesById = new Dictionary<string, LanguageInfo>();

        public static LanguageInfo Romanian = new LanguageInfo("ro", "BranchPredictionSimulator.i18n.lang_ro");
        public static LanguageInfo English = new LanguageInfo("en", "BranchPredictionSimulator.i18n.lang_en");
        public static LanguageInfo German = new LanguageInfo("de", "BranchPredictionSimulator.i18n.lang_de");

        public readonly string identifier;
        public readonly string pathToResourceFile;

        private LanguageInfo(string identifier, string pathToResource)
        {
            this.identifier = identifier;
            this.pathToResourceFile = pathToResource;
            languagesById.Add(identifier, this);
        }

        public static LanguageInfo getLanguageInfo(string id)
        {
            LanguageInfo languageInfo = null;
            languagesById.TryGetValue(id, out languageInfo);
            return languageInfo;
        }
    }
}
