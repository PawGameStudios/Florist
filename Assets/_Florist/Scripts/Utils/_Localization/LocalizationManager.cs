using UnityEngine;
using System;
using AYellowpaper.SerializedCollections;

public static class LocalizationManager
{
    public enum TextType { NORMAL, UPPER, LOWER, TITLE };


    #region Private Definitions
    [Newtonsoft.Json.JsonObject(Newtonsoft.Json.MemberSerialization.Fields)]
    private class LocalizationData
    {
        public SerializedDictionary<string, string> Items = null;

        public string GetText(string key)
        {
            if (Items.ContainsKey(key))
                return Items[key];
            return null;
        }
    }
    #endregion


    #region Private Variables
    private static LocalizationData s_localizationData;
    private static System.Globalization.TextInfo s_currentTextInfo;
    #endregion


    #region Public Methods
    public static string GetLocalizedText(string key, TextType textType = TextType.NORMAL)
    {
        string result = s_localizationData.GetText(key);
        switch (textType)
        {
            case TextType.UPPER:
                result = s_currentTextInfo.ToUpper(result);
                break;
            case TextType.LOWER:
                result = s_currentTextInfo.ToLower(result);
                break;
            case TextType.TITLE:
                result = s_currentTextInfo.ToTitleCase(result);
                break;
            default:
                break;
        }
        return result;
    }
    #endregion


    #region Private Methods
    static LocalizationManager()
    {
        LoadLocalizedText("en-US");
        // if (Application.systemLanguage == SystemLanguage.Turkish)
        // {
        //     LoadLocalizedText("tr-TR");
        // }
        // else if (Application.systemLanguage == SystemLanguage.Spanish)
        // {
        //     LoadLocalizedText("es-ES");
        // }
        // else
        // {
        //     LoadLocalizedText("en-US");
        // }
    }

    private static void LoadLocalizedText(string lang)
    {
        s_localizationData = LoadDict(lang);
        try
        {
            s_currentTextInfo = new System.Globalization.CultureInfo(lang).TextInfo;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw new Exception("Culture info fault");
        }
    }

    private static LocalizationData LoadDict(string lang)
    {
        try
        {
            string data = Resources.Load<TextAsset>($"Localization/{lang}").text;
            return FileManager.DeserializeObject<LocalizationData>(data);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            return null;
        }
    }
    #endregion


}

