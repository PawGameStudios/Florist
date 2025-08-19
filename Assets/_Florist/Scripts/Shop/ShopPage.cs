using TMPro;
using System;
using UnityEngine;
using System.Collections.Generic;
using Config;

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


    #region Tutorial
    public void SimulateFlowerButtonClick(FlowerType flowerType, FlowerColor flowerColor, Action<Transform> onScrollSet)
    {
        _flowersButton.OnButtonClicked();
        _flowersButton.SetScrollToItem(scrollIndex: 0, itemIndex: Configs.WorkshopConfig.GetFlowerItemIndex(flowerType, flowerColor), onItemSelected: onScrollSet);
    }

    public void SimulateWrapperButtonClick(WrappingPaperType paperType, Action<Transform> onScrollSet)
    {
        _bouquetsButton.OnButtonClicked();
        _bouquetsButton.SetScrollToItem(scrollIndex: 0, itemIndex: Configs.WorkshopConfig.GetWrappingPaperItemIndex(paperType), onItemSelected: onScrollSet);
    }

    public void SimulateRibbonButtonClick(RibbonType ribbonType, Action<Transform> onScrollSet)
    {
        _bouquetsButton.OnButtonClicked();
        _bouquetsButton.SetScrollToItem(scrollIndex: 1, itemIndex: Configs.WorkshopConfig.GetRibbonItemIndex(ribbonType), onItemSelected: onScrollSet);
    }

    public void SimulateFlowerBuyButtonClick(FlowerType flowerType, FlowerColor flowerColor)
    {
        _flowersButton.SimulateItemButtonClick(scrollIndex: 0, itemIndex: Configs.WorkshopConfig.GetFlowerItemIndex(flowerType, flowerColor));
    }

    public void SimulateWrapperBuyButtonClick(WrappingPaperType paperType)
    {
        _bouquetsButton.SimulateItemButtonClick(scrollIndex: 0, itemIndex: Configs.WorkshopConfig.GetWrappingPaperItemIndex(paperType));
    }

    public void SimulateRibbonBuyButtonClick(RibbonType ribbonType)
    {
        _bouquetsButton.SimulateItemButtonClick(scrollIndex: 1, itemIndex: Configs.WorkshopConfig.GetRibbonItemIndex(ribbonType));
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
