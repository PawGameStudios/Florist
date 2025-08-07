using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Image _bg;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Button _button;
    private ShopItemInfo _shopItemInfo;
    private ShopScroll _shopScroll;
    private int _index;
    private Sprite _itemBg;

    public void Init(ShopItemInfo shopItemInfo, ShopScroll shopScroll, int index, Sprite itemBg)
    {
        _itemBg = itemBg;
        _bg.sprite = itemBg;
        _shopScroll = shopScroll;
        _index = index;
        _shopItemInfo = shopItemInfo;
        _icon.sprite = shopItemInfo.Icon;

        string[] nameArray = shopItemInfo.Name.Split('_');
        if (nameArray.Length > 1)
        {
            _nameText.text = $"{LocalizationManager.GetLocalizedText(nameArray[1])} {LocalizationManager.GetLocalizedText(nameArray[0])}";
        }
        else
        {
            _nameText.text = LocalizationManager.GetLocalizedText(shopItemInfo.Name);
        }

        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id, _shopItemInfo.UnlockDay);
        float money = SaveSystem.Inst.GeneralData.Money;

        if (shopItemInfo.IsSelectable)
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = true;
                    _buttonText.text = LocalizationManager.GetLocalizedText("select");
                    break;
                case ShopData.ItemState.Selected:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("selected");
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
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("owned");
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
                _buttonText.text = LocalizationManager.GetLocalizedText("selected");
                SaveSystem.Inst.ShopData.SetSelectedState(_shopItemInfo.Id);
            }
            else
            {
                _button.interactable = true;
                _buttonText.text = LocalizationManager.GetLocalizedText("select");
                SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
            }
        }
        else if (itemState == ShopData.ItemState.Selected)
        {
            _button.interactable = true;
            _buttonText.text = LocalizationManager.GetLocalizedText("select");
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

        Init(_shopItemInfo, _shopScroll, _index, _itemBg);
    }
}


public class DecorationItem : MonoBehaviour
{
    [SerializeField] private Image _bg;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Button _button;
    private ShopItemInfo _shopItemInfo;
    private DecorationScroll _decorationScroll;
    private int _index;

    public void Init(ShopItemInfo shopItemInfo, DecorationScroll decorationScroll, int index)
    {
        _decorationScroll = decorationScroll;
        _index = index;
        _shopItemInfo = shopItemInfo;
        _nameText.text = LocalizationManager.GetLocalizedText(shopItemInfo.Name);
        _icon.sprite = shopItemInfo.Icon;

        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id);
        float money = SaveSystem.Inst.GeneralData.Money;

        if (shopItemInfo.IsSelectable)
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = true;
                    _buttonText.text = LocalizationManager.GetLocalizedText("select");
                    break;
                case ShopData.ItemState.Selected:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("selected");
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
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    _button.interactable = money >= shopItemInfo.Price;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("owned");
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
                _buttonText.text = LocalizationManager.GetLocalizedText("selected");
                SaveSystem.Inst.ShopData.SetSelectedState(_shopItemInfo.Id);
            }
            else
            {
                _button.interactable = true;
                _buttonText.text = LocalizationManager.GetLocalizedText("select");
                SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
            }
        }
        else if (itemState == ShopData.ItemState.Selected)
        {
            _button.interactable = true;
            _buttonText.text = LocalizationManager.GetLocalizedText("select");
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
                _decorationScroll.OnItemSelected(_index);
                break;
        }

        Init(_shopItemInfo, _decorationScroll, _index);
    }
}