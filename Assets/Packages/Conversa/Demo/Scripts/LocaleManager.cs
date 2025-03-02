using UnityEngine;

namespace Conversa.Demo
{
	public class LocaleManager : MonoBehaviour
	{
		[SerializeField] private LocaleDictionary englishLocale;
		[SerializeField] private LocaleDictionary spanishLocale;

		// Use a singleton pattern to access the locale manager from anywhere
		public static LocaleManager Instance { get; private set; }

		private LocaleDictionary currentLocale;

		private void Awake()
		{
			Instance = this;
		}

		private LocaleDictionary GetCurrentLocale() => currentLocale ? currentLocale : englishLocale;

		public void SetEnglish() => SetLocale(Locale.English);

		public void SetSpanish() => SetLocale(Locale.Spanish);

		public void SetLocale(Locale locale)
		{
			switch (locale)
			{
				case Locale.English:
					currentLocale = englishLocale;
					break;
				case Locale.Spanish:
					currentLocale = spanishLocale;
					break;
				default:
					currentLocale = englishLocale;
					break;
			}
		}

		public string Get(string key) => GetCurrentLocale().Get(key);

		public enum Locale { English, Spanish }
	}
}
