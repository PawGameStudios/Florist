using System;
using UnityEngine;

public class PageData
{
    public bool LoadFromSaveData;
    public PageType PageType;
    public Vector2 Position;
    public Vector2 Size;
}

public abstract class Page : MonoBehaviour
{
    public PageType PageType => _pageType;
    [SerializeField] private PageType _pageType;

    public virtual void Open(PageData pageData = null, Action onCompleted = null)
    {
        SaveSystem.Inst.SaveData.LastPage = _pageType;
    }

    public abstract void Close(PageData pageData = null, Action onCompleted = null);
}
