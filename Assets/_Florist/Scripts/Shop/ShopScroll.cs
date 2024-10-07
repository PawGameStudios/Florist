using System.Collections.Generic;
using UnityEngine;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopScroll : MonoBehaviour
{
    [SerializeField] private Transform _shopItemParent;
    private bool _isInitialized = false;

    public void Init(List<ShopItemInfo> items, ShopItem prefab)
    {
        if (_isInitialized)
            return;

        foreach (var item in items)
        {
            var shopItem = Instantiate(prefab, _shopItemParent);
            shopItem.Init(item);
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
}
