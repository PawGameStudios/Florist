using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DecorationScroll : MonoBehaviour
{
    public RectTransform Root => _root;

    [SerializeField] private RectTransform _root;
    [SerializeField] private RectTransform _content;

    private readonly List<DecorationItem> _items = new();
    private Func<ShopConfig.DecorationItemInfo, bool> _onItemSelected;

    public void Initialize(List<ShopConfig.DecorationItemInfo> itemInfos, DecorationItem itemPrefab,
        Func<ShopConfig.DecorationItemInfo, bool> onItemSelected)
    {
        _onItemSelected = onItemSelected;

        foreach (ShopConfig.DecorationItemInfo itemInfo in itemInfos)
        {
            DecorationItem item = Instantiate(itemPrefab, _content);
            item.Initialize(itemInfo, SelectItem);
            _items.Add(item);
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
        _content.anchoredPosition = Vector2.zero;
    }

    public void RefreshItems()
    {
        foreach (DecorationItem item in _items)
            item.Refresh();
    }

    private bool SelectItem(ShopConfig.DecorationItemInfo item)
    {
        return _onItemSelected != null && _onItemSelected.Invoke(item);
    }
}
