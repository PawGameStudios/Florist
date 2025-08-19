using System;
using UnityEngine;
using Sirenix.OdinInspector;

public class PageParams
{
    public bool LoadFromSaveData;
    public PageType PreviousPage;
}

public abstract class Page : SerializedMonoBehaviour
{
    public PageType PageType => _pageType;
    [SerializeField] private PageType _pageType;

    public virtual void Open(PageParams pageData = null, Action onCompleted = null)
    {
        if (_pageType != PageType.TopCanvas && _pageType != PageType.Shop)
        {
            SaveSystem.Inst.SaveData.LastPage = _pageType;
        }
        gameObject.SetActive(true);
        FirebaseController.Instance.SendCustomEvent($"page_opened_{_pageType}");
    }

    public virtual void Close(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    [Button("OpenPage")]
    public void OpenPage()
    {
        Open();
    }

    [Button("ClosePage")]
    public void ClosePage()
    {
        Close();
    }
}
