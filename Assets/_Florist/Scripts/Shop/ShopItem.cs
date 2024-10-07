using UnityEngine;
using UnityEngine.UI;
using TMPro;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private Image _icon;
    [SerializeField] private Button _button;
    private string _itemName;

    public void Init(ShopItemInfo shopItemInfo)
    {
        _itemName = shopItemInfo.Name;
        _nameText.text = shopItemInfo.Name;
        _icon.sprite = shopItemInfo.Icon;

        if (SaveSystem.Inst.ShopData.HasItem(_itemName))
        {
            _button.interactable = false;
            _buttonText.text = "Owned";
        }
        else
        {
            _button.interactable = true;
            _buttonText.text = shopItemInfo.Price.ToString();
        }
    }

    public void OnBuyClicked()
    {
        SaveSystem.Inst.ShopData.AddItem(_itemName);
    }
}
