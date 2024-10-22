using System.Collections.Generic;
using UnityEngine;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopScroll : MonoBehaviour
{
    [SerializeField] private Transform _shopItemParent;
    private bool _isInitialized = false;
    private List<ShopItem> _items = new();

    public void Init(List<ShopItemInfo> items, ShopItem prefab)
    {
        if (_isInitialized)
            return;

        foreach (var item in items)
        {
            var shopItem = Instantiate(prefab, _shopItemParent);
            shopItem.Init(item, this, _items.Count);
            _items.Add(shopItem);
        }
        _isInitialized = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnItemSelected(int index)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (i == index)
                _items[i].SetSelected(true);
            else
                _items[i].SetSelected(false);
        }
    }
}
