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

public enum DukkanState
{
    None,
    WaitingForCustomer,
    CustomerArrived,
    CustomerTalking,
    CustomerLeaving,
    FlowerReady,
    FlowerDelivered
}

public class DukkanPage : Page
{
    public DukkanState DukkanState => _dukkanState;
    public EarningsInfo EarningsInfo => _earningsInfo;
    public int CurrentCustomerIndex => _currentCustomerIndex;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private RectTransform _flowerDeliveryArea;
    [SerializeField] private Customer _customer;
    [SerializeField] private Bouquet _bouquet;
    [SerializeField] private PosController _posController;
    [SerializeField] private GameObject _contentObjects;
    private DayInfo _dayInfo;
    private ConversationRunner _convoRunner;
    private int _currentCustomerIndex;
    private Sequence _sequence;
    public EarningsInfo _earningsInfo = new();
    public FlowerDeliveredInfo _flowerDeliveredInfo = new();
    private DukkanState _dukkanState = DukkanState.None;
    private const string GO_TO_WORKSHOP = "GoToWorkshop";
    private const string END_CONVO = "EndConvo";


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
        SetItems();
    }

    private void OnDisable()
    {
        _sequence?.Kill();
        CancelInvoke();
    }

    public override void Close(PageData pageData = null, Action onCompleted = null)
    {
        _contentObjects.SetActive(true);
        References.TopCanvas.Open();

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: .95f, duration: .3f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _fadeImage.color = new Color(0, 0, 0, 0);
            onCompleted?.Invoke();
        }));

    }

    public override void Open(PageData pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        gameObject.SetActive(true);
        _contentObjects.SetActive(false);
        References.TopCanvas.Close();

        _earningsInfo ??= new EarningsInfo();
        _earningsInfo.Reset();

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: .95f, duration: .3f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _contentObjects.SetActive(true);
            References.TopCanvas.Open();
        }));
        _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.InSine));

        _currentCustomerIndex = 0;
        _dayInfo = Configs.LevelConfig.Days[SaveSystem.Inst.GeneralData.CurrentDayIndex];
        _customer.gameObject.SetActive(false);
        _posController.ResetPos();
        Invoke(nameof(StartNextEvent), 1);
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
                cost += flower.Count * Configs.WorkshopConfig.GetFlowerCost(flower.FlowerType);
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

        _flowerDeliveredInfo = _customer.GetOrderInfo(_bouquet.Order.BouquetModels);

        _convoRunner?.OnConversationEvent.RemoveAllListeners();
        _convoRunner?.OnEnd.RemoveAllListeners();
        _convoRunner = new ConversationRunner(_flowerDeliveredInfo.Conversation);
        _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
        _convoRunner.OnEnd.AddListener(HandleEndEvent);
        _convoRunner.Begin();

        // TODO: tip animation
        float tip = _earningsInfo.Earnings * _flowerDeliveredInfo.TipPercentage / 100f;
        _earningsInfo.Tip += tip;

        References.HappinessMeter.ChangeHappinessAfterOrderReceived(_flowerDeliveredInfo.HappinessChange);
        References.HappinessMeter.StopHappinessCountdown();
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

    private void StartNextEvent()
    {
        Debug.Log($"#dukkan# StartNextEvent, _currentCustomerIndex: {_currentCustomerIndex}, _dayInfo.Events.Count: {_dayInfo.Events.Count}");
        if (_currentCustomerIndex < _dayInfo.Events.Count)
        {
            DayEvent dayEvent = _dayInfo.Events[_currentCustomerIndex];
            if (dayEvent.IsEvent)
            {
                SpecialEvents specialEvent = dayEvent.EventType;
            }
            else
            {
                CustomerType customerType = dayEvent.CustomerType;
                CustomerInfo customer = Configs.LevelConfig.GetCustomer(customerType);
                Conversation initialConversation = Configs.LevelConfig.GetInitialConvo(customer);

                _customer.SetCustomer(customer);
                _customer.PlayEnterAnimation(onComplete: () =>
                {
                    _convoRunner?.OnConversationEvent.RemoveAllListeners();
                    _convoRunner = new ConversationRunner(initialConversation);
                    _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
                    _convoRunner.Begin();
                    References.HappinessMeter.StartNewHappinessCountdown();
                });
            }
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
    private void HandleConversationEvent(IConversationEvent convoEvent)
    {
        Debug.Log($"#dukkan# HandleConversationEvent: {convoEvent}");
        switch (convoEvent)
        {
            case ChoiceEvent choiceEvent:
                _customer.Talk(choiceEvent.Key, choiceEvent.Message, choiceEvent.Options, choiceEvent.ParseOptions);
                break;
            case LocalizedMessageEvent localizedMessageEvent:
                _customer.Talk(localizedMessageEvent.Key, localizedMessageEvent.Message, () =>
                {
                    localizedMessageEvent.Advance();
                });
                break;
            case UserEvent userEvent:
                Debug.Log($"#dukkan# userEvent: {userEvent.Name}");
                if (userEvent.Name == GO_TO_WORKSHOP)
                {
                    GoToWorkshop();
                }
                else if (userEvent.Name == END_CONVO)
                {
                    _customer.StopTalking();
                    _customer.PlayExitAnimation();
                    Invoke(nameof(StartNextEvent), 1);
                }
                break;
        }
    }

    private void HandleEndEvent()
    {
        Debug.Log($"#dukkan# HandleEndEvent");
        _customer.OnSpeechEnd();
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
