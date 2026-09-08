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
    // Reuse the existing receipt row and preserve serialized prefab references.
    [UnityEngine.Serialization.FormerlySerializedAs("_refundText")]
    [SerializeField] private TextMeshProUGUI _changeText;
    [SerializeField] private TextMeshProUGUI _flowerCostText;
    [SerializeField] private TextMeshProUGUI _profitText;
    [SerializeField] private Tutorial _tutorial;
    [SerializeField] private Sprite _shopItemSprite;
    [SerializeField] private Sprite _nextDayButtonSprite;
    private Sequence _sequence;
    private DayEvent _newItemIntroductionEvent = null;
    private bool _purchaseInProgress;

    private void OnDisable()
    {
        _sequence?.Kill();
        CancelInvoke();
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        _nextDayButtonObject.SetActive(true);

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
        var data = SaveSystem.Inst.GeneralData;
        int completedDay = earningsInfo.DayNumber > 0 ? earningsInfo.DayNumber - 1 : data.CurrentDayIndex;
        if (data.LastSettledDayNumber < completedDay + 1)
        {
            data.LastSettledDayNumber = completedDay + 1;
            data.ChangeMoney(earningsInfo.Profit);
            data.IncreaseDayIndex();
        }
        SaveSystem.Inst.SaveData.EarningsInfo = earningsInfo.Copy();
        DisplayEarnings(earningsInfo, completedDay);
    }

    public void Restore(EarningsInfo earningsInfo)
    {
        DisplayEarnings(earningsInfo ?? new EarningsInfo(), Mathf.Max(0, SaveSystem.Inst.GeneralData.CurrentDayIndex - 1));
    }

    private void DisplayEarnings(EarningsInfo earningsInfo, int completedDay)
    {
        _newItemIntroductionEvent = GetDailyIntroduction();

        _dayText.text = $"{LocalizationManager.GetLocalizedText("day")}: {completedDay + 1}";
        _revenueText.text = earningsInfo.GivenMoney.ToString("0.00");
        _tipText.text = earningsInfo.Tip.ToString("0.00");
        _rentText.text = $"-{earningsInfo.Rent:0.00}";
        _changeText.text = $"-{earningsInfo.Change:0.00}";
        _flowerCostText.text = $"-{earningsInfo.Cost:0.00}";
        _profitText.text = earningsInfo.Profit.ToString("0.00");


    }

    public void OnNextDayButtonClicked()
    {
        HapticsController.PlayButtonHaptic();
        // Purchases or balance changes may have happened since displaying the receipt.
        _newItemIntroductionEvent = GetDailyIntroduction();

        if (_newItemIntroductionEvent != null)
        {
            if (_newItemIntroductionEvent.TriggerAnimation)
            {
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
        var itemRect = (RectTransform)itemTransform;

        _tutorial.Init()
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand, Tutorial.ObjectActivationOptions.PopUp, Tutorial.ObjectActivationOptions.Bg)
                .SetClickableState(Tutorial.ClickableState.HighlightArea)
                .PointTo(itemRect.TransformPoint(itemRect.rect.center), Tutorial.PointDirection.Right)
                .Highlight(_shopItemSprite, itemTransform)
                .SetExplanation(LocalizationManager.GetLocalizedText("tut_new_item_explanation"))
                .SetClickCallback(OnScrollBuyClicked)
                .StartTutorial();
    }

    private void OnScrollBuyClicked()
    {
        if (_purchaseInProgress || _newItemIntroductionEvent == null) return;
        Debug.Log($"#endday# OnScrollBuyClicked, _newItemIntroductionEvent: {_newItemIntroductionEvent}");

        _purchaseInProgress = true;
        bool purchased;
        if (_newItemIntroductionEvent.EventType == SpecialEvents.InroduceFlower)
        {
            purchased = References.ShopPage.SimulateFlowerBuyButtonClick(_newItemIntroductionEvent.IntroducedFlowerType, _newItemIntroductionEvent.IntroducedFlowerColor, OnPurchaseFeedbackCompleted);
        }
        else if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroducePaper)
        {
            purchased = References.ShopPage.SimulateWrapperBuyButtonClick(_newItemIntroductionEvent.PaperType, OnPurchaseFeedbackCompleted);
        }
        else // if (_newItemIntroductionEvent.EventType == SpecialEvents.IntroduceRibbon)
        {
            purchased = References.ShopPage.SimulateRibbonBuyButtonClick(_newItemIntroductionEvent.RibbonType, OnPurchaseFeedbackCompleted);
        }

        if (!purchased)
        {
            _purchaseInProgress = false;
            return;
        }

        _tutorial.FinishTutorial();
    }

    private void OnPurchaseFeedbackCompleted()
    {
        _purchaseInProgress = false;
        if (!isActiveAndEnabled) return;
        References.ShopPage.Close();

        _newItemIntroductionEvent = null;
        _nextDayButtonObject.SetActive(true);
    }

    private void EnableNewItem()
    {
        // Introductions reveal a product. Only the explicit Shop purchase spends money.
        _newItemIntroductionEvent = null;
    }

    private DayEvent GetDailyIntroduction()
    {
        int day = SaveSystem.Inst.GeneralData.CurrentDayIndex;
        var workshop = Configs.WorkshopConfig;
        foreach (var item in Configs.ShopConfig.FlowerItems)
        {
            if (!CanIntroduceItem(item, day)) continue;
            var flower = workshop.FlowerInfo.Find(f => f.Id == item.Id);
            if (flower != null) return new DayEvent { IsEvent = true, EventType = SpecialEvents.InroduceFlower,
                IntroducedFlowerType = flower.FlowerType, IntroducedFlowerColor = flower.Color, TriggerAnimation = true };
        }
        foreach (var item in Configs.ShopConfig.WrapperItems)
        {
            if (!CanIntroduceItem(item, day)) continue;
            var paper = workshop.WrappingPaperInfo.Find(p => p.Id == item.Id);
            if (paper != null) return new DayEvent { IsEvent = true, EventType = SpecialEvents.IntroducePaper,
                PaperType = paper.WrappingPaperType, TriggerAnimation = true };
        }
        foreach (var item in Configs.ShopConfig.RibbonItems)
        {
            if (!CanIntroduceItem(item, day)) continue;
            var ribbon = workshop.RibbonInfo.Find(r => r.Id == item.Id);
            if (ribbon != null) return new DayEvent { IsEvent = true, EventType = SpecialEvents.IntroduceRibbon,
                RibbonType = ribbon.RibbonType, TriggerAnimation = true };
        }
        return null;
    }

    private static bool CanIntroduceItem(ShopConfig.ShopItemInfo item, int day)
    {
        return item != null && item.UnlockDay == day && !item.PurchaseDisabled &&
            item.Price >= 0 && SaveSystem.Inst.GeneralData.Money >= item.Price &&
            SaveSystem.Inst.ShopData.GetItemState(item.Id, item.UnlockDay) == ShopData.ItemState.Purchasable;
    }
}
