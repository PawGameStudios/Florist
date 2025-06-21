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
        SaveSystem.Inst.SaveData.LastPage = _pageType;
    }

    public abstract void Close(PageParams pageData = null, Action onCompleted = null);

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
