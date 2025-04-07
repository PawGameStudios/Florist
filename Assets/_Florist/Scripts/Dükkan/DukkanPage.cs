using TMPro;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Conversa.Runtime;
using Conversa.Runtime.Interfaces;
using Conversa.Runtime.Events;
using Sirenix.OdinInspector;
using FlowerDeliveredInfo = Customer.FlowerDeliveredInfo;
using System.Collections.Generic;
using System;
using Config;

[Serializable]
public class EarningsInfo
{
    public float Earnings;
    public float Tip;
    public float Rent;
    public float Refund;
    public float Cost;
    public float Profit;

    public void Reset()
    {
        Earnings = 0;
        Tip = 0;
        Rent = 0;
        Refund = 0;
        Cost = 0;
        Profit = 0;
    }

    public void CalculateProfit()
    {
        Profit = Earnings + Tip - Rent - Refund - Cost;
    }
}

[Serializable]
public struct OrderInfo
{
    public List<BouquetModel> BouquetModels;
}

public class DukkanPage : Page
{
    [SerializeField] private TextMeshProUGUI _moneyAmount, _diamondAmount;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private RectTransform _flowerDeliveryArea;
    [SerializeField] private Customer _customer;
    [SerializeField] private Bouquet _bouquet;
    [SerializeField] private PosController _posController;
    [SerializeField] private GameObject _contentObjects, _topObjects;
    private DayInfo _dayInfo;
    private ConversationRunner _convoRunner;
    private int _currentCustomerIndex;
    private Sequence _sequence;
    public EarningsInfo _earningsInfo = new();
    public FlowerDeliveredInfo _flowerDeliveredInfo = new();


    #region Decorations
    [FoldoutGroup("Decorations")][SerializeField] private Image _outsideImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _doorImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _floorImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _leftWallImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _rightWallImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _flowerStandImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _decorImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _tableImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _pcImage;
    [FoldoutGroup("Decorations")][SerializeField] private Image _posImage;
    #endregion


    private void OnEnable()
    {
        GeneralData.MoneyAmountChanged += OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged += OnDiamondAmountChanged;
        OnMoneyAmountChanged();
        OnDiamondAmountChanged();
        SetItems();
    }

    private void OnDisable()
    {
        _sequence?.Kill();

        GeneralData.MoneyAmountChanged -= OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged -= OnDiamondAmountChanged;
        CancelInvoke();
    }

    public override void Close(Action onCompleted = null)
    {
        // gameObject.SetActive(false);
        _contentObjects.SetActive(true);
        _topObjects.SetActive(true);

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: .95f, duration: .3f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            // gameObject.SetActive(false);
            _fadeImage.color = new Color(0, 0, 0, 0);
            onCompleted?.Invoke();
        }));

    }

    public override void Open(Action onCompleted = null)
    {
        gameObject.SetActive(true);
        _contentObjects.SetActive(false);
        _topObjects.SetActive(false);

        _earningsInfo ??= new EarningsInfo();
        _earningsInfo.Reset();

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: .95f, duration: .3f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _contentObjects.SetActive(true);
            _topObjects.SetActive(true);
        }));
        _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.InSine));

        _currentCustomerIndex = 0;
        _dayInfo = Configs.LevelConfig.Days[SaveSystem.Inst.GeneralData.CurrentDayIndex];
        _customer.gameObject.SetActive(false);
        _posController.ResetPos();
        Invoke(nameof(StartDay), 1);
    }

    public void OnDayTimeEnded()
    {
        Debug.Log("Day Ended");
        EndDay();
    }

    public void OnFlowerReady(OrderInfo orderInfo)
    {
        Debug.Log("Flower Ready");
        _posController.ResetPos();
        _bouquet.SetOrder(orderInfo);
        _bouquet.gameObject.SetActive(true);

        // calculate the cost of the bouquet
        float cost = 0;
        foreach (var bouquet in orderInfo.BouquetModels)
        {
            foreach (var flower in bouquet.Flowers)
            {
                cost += flower.Value * Configs.WorkshopConfig.GetFlowerCost(flower.Key);
            }

            cost += Configs.WorkshopConfig.GetRibbonCost(bouquet.RibbonType);
            cost += Configs.WorkshopConfig.GetWrappingPaperCost(bouquet.WrappingPaperType);
        }
        _earningsInfo.Cost += cost;
    }

    public void OnFlowerDelivered()
    {
        Debug.Log("Flower Delivered");
        _bouquet.gameObject.SetActive(false);

        if (_customer.CustomerInfo.GoodbyeConversation != null)
        {
            // this means a special goodbye conversation is set for this customer
            _convoRunner?.OnConversationEvent.RemoveAllListeners();
            _convoRunner = new ConversationRunner(_customer.CustomerInfo.GoodbyeConversation);
            _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
            _convoRunner.Begin();
        }
        else
        {
            // this means a generic goodbye conversation will be used

            FlowerDeliveredInfo flowerDeliveredInfo = _customer.GetOrderInfo(_bouquet.Order.BouquetModels);
            _flowerDeliveredInfo = flowerDeliveredInfo;
            // TODO: check if order is correct
            // decrease happiness if not
            // increase happiness if correct
            // decrease money with bouquet price
            // increase money with tip
            // start dialogue with goodbye conversation based on happiness

            float tip = flowerDeliveredInfo.Tip;
            _earningsInfo.Tip += tip;
        }

        // TODO: call later when the conversation is over
        _customer.PlayExitAnimation();
        References.HappinessMeter.StopCountdown();

        Invoke(nameof(NextCustomer), 1);
    }

    public bool CheckIfInCustomerArea(Vector2 pos)
    {
        Rect rect = _flowerDeliveryArea.rect;

        // Get the left, right, top, and bottom boundaries of the rect
        Vector3 rectPos = _flowerDeliveryArea.position;
        float leftSide = rectPos.x - rect.width / 2;
        float rightSide = rectPos.x + rect.width / 2;
        float topSide = rectPos.y + rect.height / 2;
        float bottomSide = rectPos.y - rect.height / 2;

        // Check to see if the point is in the calculated bounds
        if (pos.x >= leftSide &&
            pos.x <= rightSide &&
            pos.y >= bottomSide &&
            pos.y <= topSide)
        {
            return true;
        }
        return false;
    }

    private void SetItems()
    {
        // outside
        Sprite outSideSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.OutsideDukkan);
        if (outSideSprite != null)
            _outsideImage.sprite = outSideSprite;

        // door
        Sprite doorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Door);
        if (doorSprite != null)
            _doorImage.sprite = doorSprite;

        // floor
        Sprite floorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Floor);
        if (floorSprite != null)
            _floorImage.sprite = floorSprite;

        // left wall
        Sprite wallSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Wallpaper);
        if (wallSprite != null)
            _leftWallImage.sprite = wallSprite;

        // right wall
        if (wallSprite != null)
            _rightWallImage.sprite = wallSprite;

        // flower stand
        Sprite flowerStandSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.FlowerStand);
        if (flowerStandSprite != null)
            _flowerStandImage.sprite = flowerStandSprite;

        // decor
        Sprite decorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Decor);
        if (decorSprite != null)
            _decorImage.sprite = decorSprite;

        // table
        Sprite tableSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Counter);
        if (tableSprite != null)
            _tableImage.sprite = tableSprite;

        // pc
        Sprite pcSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Pc);
        if (pcSprite != null)
            _pcImage.sprite = pcSprite;

        // pos
        Sprite posSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Pos);
        if (posSprite != null)
            _posImage.sprite = posSprite;

        // speech bubbles
        _customer.SetSpeechBubbleSprites();
    }

    private void StartDay()
    {
        Debug.Log("#dukkan# StartDay");
        if (_dayInfo.SpecialEvent != SpecialEvents.None && _dayInfo.IsSpecialEventOnDayStart)
        {
            StartSpecialEvent();
        }
        else
        {
            NextCustomer();
        }
    }

    private void NextCustomer()
    {
        Debug.Log("#dukkan# NextCustomer");
        if (_currentCustomerIndex < _dayInfo.Customers.Count)
        {
            CustomerType customerType = _dayInfo.Customers[_currentCustomerIndex];
            CustomerInfo customer = Configs.LevelConfig.GetCustomer(customerType);
            Conversation initialConversation = Configs.LevelConfig.GetInitialConvo(customer);

            _customer.SetCustomer(customer);
            _customer.PlayEnterAnimation(onComplete: () =>
            {
                _convoRunner?.OnConversationEvent.RemoveAllListeners();
                _convoRunner = new ConversationRunner(initialConversation);
                _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
                _convoRunner.Begin();
                References.HappinessMeter.StartCountdown();
            });
            _currentCustomerIndex++;
        }
        else
        {
            EndDay();
        }
    }

    private void EndDay()
    {
        Debug.Log("#dukkan# EndDay");
        Close(onCompleted: () =>
        {
            _earningsInfo.CalculateProfit();
            References.EndDayPage.SetData(_earningsInfo);
            References.EndDayPage.Open();
        });
    }

    private void StartSpecialEvent()
    {
        Debug.Log("#dukkan# StartSpecialEvent");
    }

    private void GoToWorkshop()
    {
        Debug.Log("#dukkan# GoToWorkshop");
        _convoRunner.OnConversationEvent.RemoveAllListeners();
        _customer.StopTalking();

        float payment = _customer.GetOrderPayment();
        _earningsInfo.Earnings += payment;

        _posController.ReceivePayment(payment, () =>
        {
            References.WorkshopPage.Open();
        });
    }


    #region Event Listeners
    private void OnMoneyAmountChanged()
    {
        _moneyAmount.text = $"{SaveSystem.Inst.GeneralData.Money:0.##}";
    }

    private void OnDiamondAmountChanged()
    {
        _diamondAmount.text = $"{SaveSystem.Inst.GeneralData.Diamonds:0.##}";
    }

    private void HandleConversationEvent(IConversationEvent convoEvent)
    {
        Debug.Log($"#dukkan# HandleConversationEvent: {convoEvent}");
        switch (convoEvent)
        {
            case ChoiceEvent choiceEvent:
                _customer.Talk(choiceEvent.Key, choiceEvent.Message, choiceEvent.Options, choiceEvent.ParseOptions);
                break;
            case UserEvent userEvent:
                Debug.Log($"#dukkan# userEvent: {userEvent.Name}");
                if (userEvent.StopsFlow)
                    GoToWorkshop();
                break;
        }
    }
    #endregion


    #region Button Clicks
    public void OnCustomerButtonClicked()
    {
        Debug.Log("OnCustomerButtonClicked");
    }
    #endregion


    [Button("OpenPage")]
    public void OpenPage()
    {
        Open();
    }

    [Button("ClosePage")]
    public void ClosePage()
    {
        Close();
    }
}
