using System;
using UnityEngine;

public class MainPage : Page
{
    void OnEnable()
    {
        CheckSaveData();
    }

    public override void Close(PageData pageData = null, Action onCompleted = null)
    {
        throw new NotImplementedException();
    }

    public override void Open(PageData pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
    }

    public void OnPlayClicked()
    {
        gameObject.SetActive(false);
        References.DukkanPage.Open();
    }

    public void OnShopClicked()
    {
        // gameObject.SetActive(false);
        References.ShopPage.Open();
    }

    private void CheckSaveData()
    {
        if (SaveSystem.Inst.SaveData.LastPage == PageType.MainPage)
        {
            // do nothing
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.Dukkan)
        {
            gameObject.SetActive(false);
            PageData pageData = new()
            {
                PageType = PageType.Dukkan,
                Position = References.DukkanPage.transform.position,
                Size = References.DukkanPage.GetComponent<RectTransform>().sizeDelta
            };
            References.DukkanPage.Open(pageData);
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.Workshop)
        {
            gameObject.SetActive(false);
            References.WorkshopPage.Open();
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.EndDay)
        {
            gameObject.SetActive(false);
            References.EndDayPage.Open();
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.MainPage)
        {
            gameObject.SetActive(false);
        }
    }

}
