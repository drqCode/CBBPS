using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Resources;

namespace GAgSimulator.Localization {
	public class LanguageSelector : INotifyPropertyChanged {
		/// <summary>
		/// Occurs whenever the selected language has changed.
		/// </summary>
		public event EventHandler languageChanged;

		/// <summary>
		/// Indicates the currently selected language.
		/// </summary>
		public LanguageInfo SelectedLanguage { get; private set; }

		// Handles the current resource file.        
		private ResourceManager resourceManager;

		// A reuse pool for the resource file manager (optimization).        
		private Dictionary<string, ResourceManager> resourceManagerPool = new Dictionary<string, ResourceManager>();

		private LanguageSelector() {
			ChangeLanguage(LanguageInfo.Romanian); // especially for design time
		}

		/// <summary>
		/// Global entry point for the LanguageSelector singleton class.
		/// </summary>
		public static readonly LanguageSelector Global = new LanguageSelector();

		/// <summary>
		/// Selects a new language. This will cascade onto all localized strings.
		/// </summary>
		/// <param name="language">The new language data to select.</param>
		public void ChangeLanguage(LanguageInfo language) {
			SelectedLanguage = language;

			//get the resource manager. take from pool if available.
			if (!resourceManagerPool.TryGetValue(language.identifier, out resourceManager)) {
				resourceManager = new ResourceManager(language.pathToResourceFile, this.GetType().Assembly);
				resourceManagerPool.Add(language.identifier, resourceManager);
			}

			//notifications
			languageChanged?.Invoke(this, EventArgs.Empty);
			PropertyChanged?.Invoke(this, SelectedLanguagePropertyChangedEventArgs);
		}

		/// <summary>
		/// Gets the localized string for the currently selected language.
		/// </summary>
		/// <param name="entryKey">The key in the resource dictionary to search for.</param>
		/// <returns>Localized string from the resource dictionary with the corresponding key.</returns>
		public string getLocalizedString(string entryKey) {
			if (resourceManager == null) {
				return "[i18n error - ResourceManager not yet initialized.]";
			}
			string localizedString = resourceManager.GetString(entryKey);
			return localizedString == null ? "[i18n error - Key not found.]" : localizedString;
		}

		#region INotifyPropertyChanged members

		public event PropertyChangedEventHandler PropertyChanged;
		public static PropertyChangedEventArgs SelectedLanguagePropertyChangedEventArgs = new PropertyChangedEventArgs("SelectedLanguage");

		#endregion

	}
}
