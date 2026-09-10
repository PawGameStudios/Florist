using System;
namespace Florist.Merge
{
    public static class MergeLocalization
    {
        public static string Text(string key) => LocalizationManager.GetLocalizedText(key) ?? key;
        public static string Format(string key, params object[] args) => string.Format(Text(key), args);
        public static string Name(string key, string fallback) => string.IsNullOrEmpty(key) ? fallback : LocalizationManager.GetLocalizedText(key) ?? fallback;
    }
}
