using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationItem : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _selected;
    [SerializeField] private GameObject _lock;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _statusText;

    private ShopConfig.DecorationItemInfo _item;
    private Func<ShopConfig.DecorationItemInfo, bool> _onClicked;

    public void Initialize(ShopConfig.DecorationItemInfo item, Func<ShopConfig.DecorationItemInfo, bool> onClicked)
    {
        _item = item;
        _onClicked = onClicked;
        _icon.sprite = item.Icon != null ? item.Icon : item.DecorationSprite;
        _nameText.text = item.Name;
        Refresh();
    }

    public void Refresh()
    {
        if (_item == null || SaveSystem.Inst?.ShopData == null)
            return;

        ShopData.ItemState state = SaveSystem.Inst.ShopData.GetItemState(_item.Id, _item.UnlockDay);
        bool hasMoney = SaveSystem.Inst.GeneralData.Money >= _item.Price;

        _selected.SetActive(state == ShopData.ItemState.Selected);
        _lock.SetActive(state == ShopData.ItemState.Locked);
        _button.interactable = state != ShopData.ItemState.Locked &&
                               state != ShopData.ItemState.Selected &&
                               (state != ShopData.ItemState.Purchasable || hasMoney);

        _statusText.text = state switch
        {
            ShopData.ItemState.Locked => $"Gün {_item.UnlockDay}",
            ShopData.ItemState.Purchasable when _item.Price > 0 => $"{_item.Price} F",
            ShopData.ItemState.Purchasable => "Seç",
            ShopData.ItemState.Purchased => "Seç",
            ShopData.ItemState.Selected => "Seçili",
            _ => string.Empty
        };
    }

    public void OnButtonClicked()
    {
        if (_item != null && (_onClicked?.Invoke(_item) ?? false))
            Refresh();
    }
}
