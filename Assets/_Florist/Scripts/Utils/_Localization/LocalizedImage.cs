using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LocalizedImage : MonoBehaviour
{
    // [SerializeField] private string imageName = null;
    // [SerializeField] private Sprite EngSprite = null;
    [SerializeField] private Sprite TrSprite = null;
    [SerializeField] private Sprite EsSprite = null;

    private void Awake()
    {
        LoadImage(GetComponent<Image>());
        Destroy(this);
    }

    private void LoadImage(Image imageObject)
    {
        // imageObject.sprite = TrSprite;
        if(Application.systemLanguage == SystemLanguage.Turkish)
            imageObject.sprite = TrSprite;
        else if(Application.systemLanguage == SystemLanguage.Spanish)
            imageObject.sprite = EsSprite;
        else
        {
            // imageObject.sprite = EngSprite;
        }
    }
}
