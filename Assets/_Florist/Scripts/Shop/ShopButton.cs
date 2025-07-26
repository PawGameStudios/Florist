using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using ShopButtonType = ShopPage.ShopButtonType;

public class ShopButton : MonoBehaviour
{
    [SerializeField] private ShopButtonType _type;
    [SerializeField] private ShopScroll _shopScrollPrefab;
    [SerializeField] private ShopItem _shopItemPrefab;
    [SerializeField] private Sprite _shopItemBg;
    [SerializeField] private List<ItemType> _itemTypes = new();
    [ShowIf("@_itemTypes.Count > 1")][SerializeField] private List<GameObject> _subButtons = null;
    [SerializeField] private List<GameObject> _arrowObjects;
    private readonly List<ShopScroll> _scrolls = new();
    private bool _isOpen = false;
    private bool _isExtended = false;
    private bool _isInitialized = false;

    public void OnButtonClicked()
    {
        if (_subButtons != null && _subButtons.Count > 0)
        {
            if (!_isInitialized)
            {
                int _subButtonsCount = _subButtons.Count;
                for (int i = 0; i < _subButtonsCount; i++)
                {
                    var scroll = Instantiate(_shopScrollPrefab, References.ShopPage.ShopScrollParent);
                    scroll.Init(Configs.ShopConfig.GetItems(_itemTypes[i]), _shopItemPrefab, _shopItemBg);
                    _scrolls.Add(scroll);
                }
                _isInitialized = true;
            }

            ToggleSubButtons();
            if (!_isOpen)
            {
                OnSubButtonClicked(0);
            }
        }
        else
        {
            if (!_isOpen)
            {
                if (!_isInitialized)
                {
                    var scroll = Instantiate(_shopScrollPrefab, References.ShopPage.ShopScrollParent);
                    scroll.Init(Configs.ShopConfig.GetItems(_itemTypes[0]), _shopItemPrefab, _shopItemBg);
                    _scrolls.Add(scroll);
                    _isInitialized = true;
                }

                References.ShopPage.OnButtonClicked(_type);
                _isOpen = true;
                _scrolls[0].Open();
                _arrowObjects[0].SetActive(true);
            }
        }
    }

    public void OnSubButtonClicked(int index)
    {
        for (int i = 0; i < _subButtons.Count; i++)
        {
            if (i == index)
            {
                References.ShopPage.OnButtonClicked(_type);
                _scrolls[i].Open();
                _arrowObjects[i].SetActive(true);
            }
            else
            {
                _scrolls[i].Close();
                _arrowObjects[i].SetActive(false);
            }
        }
    }

    public void CloseScroll()
    {
        if (_subButtons != null && _subButtons.Count > 0)
        {
            for (int i = 0; i < _subButtons.Count; i++)
            {
                _scrolls[i].Close();
                _arrowObjects[i].SetActive(false);
            }

            _isExtended = false;
            foreach (var subButton in _subButtons)
            {
                subButton.SetActive(_isExtended);
            }
        }
        else
        {
            _isOpen = false;
            _scrolls[0].Close();
            _arrowObjects[0].SetActive(false);
        }
    }

    public void SetScrollToItem(int scrollIndex, int itemIndex, Action<Transform> onItemSelected = null)
    {
        if (scrollIndex < 0 || scrollIndex >= _scrolls.Count)
        {
            Debug.LogError($"Scroll index {scrollIndex} is out of range.");
            return;
        }

        Debug.LogError($"Setting scroll to item. Scroll index: {scrollIndex}, Item index: {itemIndex}");
        _scrolls[scrollIndex].SetScrollToItem(itemIndex, onComplete: () =>
        {
            onItemSelected?.Invoke(_scrolls[scrollIndex].GetItemTransform(itemIndex));
        });
    }

    public void SimulateItemButtonClick(int scrollIndex, int itemIndex)
    {
        _scrolls[scrollIndex].SimulateButtonClick(itemIndex);
    }

    private void ToggleSubButtons()
    {
        _isExtended = !_isExtended;
        foreach (var subButton in _subButtons)
        {
            subButton.SetActive(_isExtended);
        }
    }
}
