using UnityEngine;

namespace Conversa.Demo
{
	[CreateAssetMenu(menuName = "Conversa/Demo/Locale Dictionary")]
	public class LocaleDictionary : ScriptableObject
	{
		[SerializeField] private LocaleDictionaryEntry[] entries;

		public string Get(string key)
		{
			foreach (var entry in entries)
			{
				if (entry.Key == key)
				{
					return entry.Value;
				}
			}

			return key;
		}
	}

	[System.Serializable]
	public class LocaleDictionaryEntry
	{
		[SerializeField] private string key;
		[SerializeField] private string value;

		public string Key => key;
		public string Value => value;
	}
}
