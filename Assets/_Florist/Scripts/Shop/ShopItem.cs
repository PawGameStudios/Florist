using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Button _button;
    private ShopItemInfo _shopItemInfo;
    private ShopScroll _shopScroll;
    private int _index;

    public void Init(ShopItemInfo shopItemInfo, ShopScroll shopScroll, int index)
    {
        _shopScroll = shopScroll;
        _index = index;
        _shopItemInfo = shopItemInfo;
        _nameText.text = shopItemInfo.Name;
        _icon.sprite = shopItemInfo.Icon;

        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id);
        long money = SaveSystem.Inst.GeneralData.Money;

        if (shopItemInfo.IsSelectable)
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    _button.interactable = false;
                    _buttonText.text = "Locked";
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = shopItemInfo.Price.ToString();
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = true;
                    _buttonText.text = "Select";
                    break;
                case ShopData.ItemState.Selected:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = "Selected";
                    break;
            }
        }
        else
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    _button.interactable = false;
                    _buttonText.text = "Locked";
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = shopItemInfo.Price.ToString();
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = "Owned";
                    break;
            }
        }
    }

    public void SetSelected(bool isSelected)
    {
        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id);
        if (itemState == ShopData.ItemState.Purchased)
        {
            if (isSelected)
            {
                _button.interactable = false;
                _buttonText.text = "Selected";
                SaveSystem.Inst.ShopData.SetSelectedState(_shopItemInfo.Id);
            }
            else
            {
                _button.interactable = true;
                _buttonText.text = "Select";
                SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
            }
        }
        else if (itemState == ShopData.ItemState.Selected)
        {
            _button.interactable = true;
            _buttonText.text = "Select";
            SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
        }
    }

    public void OnButtonClicked()
    {
        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id);
        switch (itemState)
        {
            case ShopData.ItemState.Purchasable:
                SaveSystem.Inst.GeneralData.ChangeMoney(-_shopItemInfo.Price);
                SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
                break;
            case ShopData.ItemState.Purchased:
                _shopScroll.OnItemSelected(_index);
                break;
        }

        Init(_shopItemInfo, _shopScroll, _index);
    }
}
