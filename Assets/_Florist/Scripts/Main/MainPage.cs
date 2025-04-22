using System;

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
        gameObject.SetActive(true);
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
            // TODO:
            // gameObject.SetActive(false);
            // PageData pageData = new()
            // {
            //     LoadFromSaveData = true,
            // };
            // References.DukkanPage.Open(pageData);
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.Workshop)
        {
            // TODO:
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.EndDay)
        {
            // TODO:
        }
    }

}
