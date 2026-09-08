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

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
        // Initialize synchronously: end-of-day navigation runs before Unity calls Start.
        if (_currentButton == null)
            _flowersButton.OnButtonClicked();
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
        if (_currentButton != null && _currentButtonType == type)
        {
            return;
        }

        if (_currentButton != null)
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
        _flowersButton.SetScrollToItem(scrollIndex: 0, itemIndex: Configs.ShopConfig.FlowerItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetFlowerId(flowerType, flowerColor)), onItemSelected: onScrollSet);
    }

    public void SimulateWrapperButtonClick(WrappingPaperType paperType, Action<Transform> onScrollSet)
    {
        _bouquetsButton.OnButtonClicked();
        _bouquetsButton.SetScrollToItem(scrollIndex: 0, itemIndex: Configs.ShopConfig.WrapperItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetWrappingPaperId(paperType)), onItemSelected: onScrollSet);
    }

    public void SimulateRibbonButtonClick(RibbonType ribbonType, Action<Transform> onScrollSet)
    {
        _bouquetsButton.OnButtonClicked();
        _bouquetsButton.SetScrollToItem(scrollIndex: 1, itemIndex: Configs.ShopConfig.RibbonItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetRibbonId(ribbonType)), onItemSelected: onScrollSet);
    }

    public bool SimulateFlowerBuyButtonClick(FlowerType flowerType, FlowerColor flowerColor, Action onFeedbackCompleted = null)
    {
        return _flowersButton.SimulateItemButtonClick(scrollIndex: 0, itemIndex: Configs.ShopConfig.FlowerItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetFlowerId(flowerType, flowerColor)), onFeedbackCompleted: onFeedbackCompleted);
    }

    public bool SimulateWrapperBuyButtonClick(WrappingPaperType paperType, Action onFeedbackCompleted = null)
    {
        return _bouquetsButton.SimulateItemButtonClick(scrollIndex: 0, itemIndex: Configs.ShopConfig.WrapperItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetWrappingPaperId(paperType)), onFeedbackCompleted: onFeedbackCompleted);
    }

    public bool SimulateRibbonBuyButtonClick(RibbonType ribbonType, Action onFeedbackCompleted = null)
    {
        return _bouquetsButton.SimulateItemButtonClick(scrollIndex: 1, itemIndex: Configs.ShopConfig.RibbonItems.FindIndex(item => item.Id == Configs.WorkshopConfig.GetRibbonId(ribbonType)), onFeedbackCompleted: onFeedbackCompleted);
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
