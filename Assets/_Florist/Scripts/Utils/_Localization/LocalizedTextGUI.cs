using UnityEngine;
using UnityEngine.UI;

public class LocalizedTextGUI : MonoBehaviour
{
    [SerializeField] private LocalizationManager.TextType textType = LocalizationManager.TextType.NORMAL;
    [SerializeField] private string textName = null;

    private void Awake()
    {
        LoadText(GetComponent<Text>());
        Destroy(this);
    }

    private bool LoadText(Text textObject)
    {
        string text = textName;
        if (text == "")
        {
            string[] splitted = textObject.name.Split('_');
            textType = LocalizationManager.TextType.NORMAL;
            if (splitted.Length == 3)
            {
                if (splitted[2] == "U")
                    textType = LocalizationManager.TextType.UPPER;
                else if (splitted[2] == "L")
                    textType = LocalizationManager.TextType.LOWER;
                else if (splitted[2] == "T")
                    textType = LocalizationManager.TextType.TITLE;
                text = splitted[1];
            }
        }
        textObject.text = LocalizationManager.GetLocalizedText(text, textType);
        return true;
    }
}
