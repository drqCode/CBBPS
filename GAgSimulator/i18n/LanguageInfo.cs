using System.Collections.Generic;

namespace GAgSimulator.Localization {
	public class LanguageInfo {
		private static Dictionary<string, LanguageInfo> languagesById = new Dictionary<string, LanguageInfo>();

		public static LanguageInfo Romanian = new LanguageInfo("ro", "GAgSimulator.i18n.lang_ro");

		public readonly string identifier;
		public readonly string pathToResourceFile;

		private LanguageInfo(string identifier, string pathToResource) {
			this.identifier = identifier;
			this.pathToResourceFile = pathToResource;
			languagesById.Add(identifier, this);
		}

		public static LanguageInfo getLanguageInfo(string id) {
			LanguageInfo languageInfo = null;
			languagesById.TryGetValue(id, out languageInfo);
			return languageInfo;
		}
	}
}
