using UnityEngine;
using TMPro;
using DG.Tweening;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using System;
using Config;
using ShopButtonType = ShopPage.ShopButtonType;

public class EndDayPage : Page
{
    [SerializeField] private GameObject _endDayPanel;
    [SerializeField] private GameObject _nextDayButtonObject;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private Image _nightImage;
    [SerializeField] private Image _nightLightImage;
    [SerializeField] private Image _dayImage;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _revenueText;
    [SerializeField] private TextMeshProUGUI _tipText;
    [SerializeField] private TextMeshProUGUI _rentText;
    [SerializeField] private TextMeshProUGUI _refundText;
    [SerializeField] private TextMeshProUGUI _flowerCostText;
    [SerializeField] private TextMeshProUGUI _profitText;
    [SerializeField] private Tutorial _tutorial;
    [SerializeField] private Sprite _shopItemSprite;
    [SerializeField] private Sprite _nextDayButtonSprite;
    private Sequence _sequence;
    private DayEvent _newItemIntroductionEvent = null;

    private void OnDisable()
    {
        _sequence?.Kill();
        CancelInvoke();
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        SaveSystem.Inst.GeneralData.IncreaseDayIndex();

        _endDayPanel.SetActive(true);

        _fadeImage.gameObject.SetActive(true);
        _fadeImage.color = new Color(0, 0, 0, .95f);
        _nightLightImage.color = new Color(1, 1, 1, 1);
        _nightImage.color = new Color(1, 1, 1, 1);
        _dayImage.color = new Color(1, 1, 1, 1);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.InSine).OnComplete(() =>
        {
            _fadeImage.gameObject.SetActive(false);
            onCompleted?.Invoke();
        }));
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        _endDayPanel.SetActive(false);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_nightLightImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.Append(_nightImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.Append(_dayImage.DOFade(endValue: 0, duration: .4f).SetEase(Ease.Linear));
        _sequence.AppendCallback(() =>
        {
            gameObject.SetActive(false);
            onCompleted?.Invoke();
        });
    }

    public void SetData(EarningsInfo earningsInfo, DayEvent newItemIntroductionEvent)
    {
        _endDayPanel.SetActive(true);
        _fadeImage.gameObject.SetActive(false);
        _newItemIntroductionEvent = newItemIntroductionEvent;

        _dayText.text = $"{LocalizationManager.GetLocalizedText("day")}: {SaveSystem.Inst.GeneralData.CurrentDayIndex + 1}";
        _revenueText.text = (earningsInfo.GivenMoney - earningsInfo.Change).ToString("0.00");
        _tipText.text = earningsInfo.Tip.ToString("0.00");
        _rentText.text = $"-{earningsInfo.Rent:0.00}";
        _refundText.text = $"-{earningsInfo.Refund:0.00}";
        _flowerCostText.text = $"-{earningsInfo.Cost:0.00}";
        _profitText.text = earningsInfo.Profit.ToString("0.00");

        SaveSystem.Inst.GeneralData.ChangeMoney(earningsInfo.Profit);
    }

    public void OnNextDayButtonClicked()
    {
        HapticsController.PlayButtonHaptic();

        if (_newItemIntroductionEvent != null)
        {
            if (_newItemIntroductionEvent.TriggerAnimation)
            {
                References.ShopPage.Open();
                PlayEvent();
            }
            else
            {
                EnableNewItem();
                References.MainPage.Open();
                Close();
            }
        }
        else
        {
            References.MainPage.Open();
            Close(onCompleted: () =>
            {
            });
        }
    }

    private void PlayEvent()
    {
        Debug.Log($"#endday# PlayEvent, _newItemIntroductionEvent: {_newItemIntroductionEvent}");

        _nextDayButtonObject.SetActive(false);
        References.ShopPage.Open();

        if (_newItemIntroductionEvent.EventType == SpecialEvents.InroduceFlower)
        {
            References.ShopPage.SimulateFlowerButtonClick(_newItemIntroductionEvent.IntroducedFlowerType, _newItemIntroductionEvent.IntroducedFlowerColor, OnScrollSet);
        }
        else if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroducePaper)
        {
            References.ShopPage.SimulateWrapperButtonClick(_newItemIntroductionEvent.PaperType, OnScrollSet);
        }
        else // if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroduceRibbon)
        {
            References.ShopPage.SimulateRibbonButtonClick(_newItemIntroductionEvent.RibbonType, OnScrollSet);
        }
    }

    private void OnScrollSet(Transform itemTransform)
    {
        Debug.Log($"#endday# OnScrollSet, itemTransform: {itemTransform}");

        _tutorial.Init()
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp, Tutorial.ObjectActivationOptions.Bg)
                .SetClickableState(Tutorial.ClickableState.HighlightArea)
                .PointTo(itemTransform.position, Tutorial.PointDirection.Right)
                .Highlight(_shopItemSprite, itemTransform)
                .SetExplanation(LocalizationManager.GetLocalizedText("tut_new_item_explanation"))
                .SetClickCallback(OnScrollBuyClicked)
                .StartTutorial();
    }

    private void OnScrollBuyClicked()
    {
        Debug.Log($"#endday# OnScrollBuyClicked, _newItemIntroductionEvent: {_newItemIntroductionEvent}");

        if (_newItemIntroductionEvent.EventType == SpecialEvents.InroduceFlower)
        {
            References.ShopPage.SimulateFlowerBuyButtonClick(_newItemIntroductionEvent.IntroducedFlowerType, _newItemIntroductionEvent.IntroducedFlowerColor);
        }
        else if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroducePaper)
        {
            References.ShopPage.SimulateWrapperBuyButtonClick(_newItemIntroductionEvent.PaperType);
        }
        else // if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroduceRibbon)
        {
            References.ShopPage.SimulateRibbonBuyButtonClick(_newItemIntroductionEvent.RibbonType);
        }

        References.ShopPage.Close();

        _newItemIntroductionEvent = null;
        _nextDayButtonObject.SetActive(true);
        _tutorial.FinishTutorial();
    }

    private void EnableNewItem()
    {
        if (_newItemIntroductionEvent.EventType == SpecialEvents.InroduceFlower)
        {
            string itemId = Configs.WorkshopConfig.GetFlowerId(_newItemIntroductionEvent.IntroducedFlowerType, _newItemIntroductionEvent.IntroducedFlowerColor);
            ShopConfig.ShopItemInfo itemInfo = Configs.ShopConfig.GetItemById(ItemType.Flower, itemId);

            SaveSystem.Inst.GeneralData.ChangeMoney(-itemInfo.Price);
            SaveSystem.Inst.ShopData.SetPurchasedState(itemInfo.Id);
        }
        else if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroducePaper)
        {
            string itemId = Configs.WorkshopConfig.GetWrappingPaperId(_newItemIntroductionEvent.PaperType);
            ShopConfig.ShopItemInfo itemInfo = Configs.ShopConfig.GetItemById(ItemType.Wrapper, itemId);

            SaveSystem.Inst.GeneralData.ChangeMoney(-itemInfo.Price);
            SaveSystem.Inst.ShopData.SetPurchasedState(itemInfo.Id);
        }
        else // if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroduceRibbon)
        {
            string itemId = Configs.WorkshopConfig.GetRibbonId(_newItemIntroductionEvent.RibbonType);
            ShopConfig.ShopItemInfo itemInfo = Configs.ShopConfig.GetItemById(ItemType.Ribbon, itemId);

            SaveSystem.Inst.GeneralData.ChangeMoney(-itemInfo.Price);
            SaveSystem.Inst.ShopData.SetPurchasedState(itemInfo.Id);
        }
    }
}
