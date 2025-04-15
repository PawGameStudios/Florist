using TMPro;
using System;
using UnityEngine;
using System.Collections.Generic;

public class ShopPage : Page
{
    public enum ShopButtonType
    {
        Flowers,
        Bouquets,
        Upgrades,
        Decorations,
    }
    public Transform ShopScrollParent;
    [SerializeField] private TextMeshProUGUI _moneyAmount, _diamondAmount;
    [SerializeField] private ShopButton _flowersButton, _bouquetsButton, _upgradesButton, _decorationsButton;
    private ShopScroll _currentScroll;
    private ShopButtonType _currentButtonType;
    private ShopButton _currentButton;

    private void OnEnable()
    {
        GeneralData.MoneyAmountChanged += OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged += OnDiamondAmountChanged;
        OnMoneyAmountChanged();
        OnDiamondAmountChanged();
    }

    private void Start()
    {
        _currentButton = _flowersButton;
        _currentButton.OnButtonClicked();
        _currentButtonType = ShopButtonType.Flowers;
    }

    private void OnDisable()
    {
        GeneralData.MoneyAmountChanged -= OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged -= OnDiamondAmountChanged;
    }

    public override void Close(PageData pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageData pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
        gameObject.SetActive(true);
    }


    #region Buttons
    public void OnCloseClicked()
    {
        Close();
    }

    public void OnButtonClicked(ShopButtonType type)
    {
        if (_currentButtonType == type)
        {
            return;
        }

        _currentButton.CloseScroll();
        _currentButtonType = type;

        _currentButton = _currentButtonType switch
        {
            ShopButtonType.Flowers => _flowersButton,
            ShopButtonType.Bouquets => _bouquetsButton,
            ShopButtonType.Upgrades => _upgradesButton,
            ShopButtonType.Decorations => _decorationsButton,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
    #endregion


    private void OnMoneyAmountChanged()
    {
        _moneyAmount.text = $"{SaveSystem.Inst.GeneralData.Money:0.##}";
    }

    private void OnDiamondAmountChanged()
    {
        _diamondAmount.text = $"{SaveSystem.Inst.GeneralData.Diamonds:0.##}";
    }
}
