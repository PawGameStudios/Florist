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
using System.Collections;

[Serializable]
public class EarningsInfo
{
    public int GivenMoney;
    public int Change;
    public int Tip;
    public int Rent;
    public int Refund; // Legacy save field; refunds are no longer part of settlement.
    public int Cost;
    public int Profit;
    public int Price;
    public int DayNumber;
    public int CurrentCustomerPrice;
    public int CurrentCustomerCash;

    public EarningsInfo Copy() => (EarningsInfo)MemberwiseClone();

    public void Reset()
    {
        DayNumber = SaveSystem.Inst.GeneralData.CurrentDayIndex + 1;
        CurrentCustomerPrice = 0;
        CurrentCustomerCash = 0;
        GivenMoney = 0;
        Change = 0;
        Tip = 0;
        Rent = Mathf.Max(0, Configs.EconomyConfig.DailyRent);
        Refund = 0;
        Cost = 0;
        Profit = 0;
        Price = 0;
    }

    public void CalculateProfit()
    {
        Profit = GivenMoney + Tip - Change - Rent - Cost;
    }
}

[Serializable]
public struct OrderInfo
{
    public List<BouquetModel> BouquetModels;
}

public class DukkanPage : Page
{
    private enum HintType
    {
        None,
        GiveFlower
    }

    public List<BouquetModel> CurrentOrder => _customer.CurrentOrder;
    public List<string> ConvoHistory => _convoHistory;
    public CustomerInfo CurrentCustomerInfo => _customer.CustomerInfo;
    public float CustomerServiceSeconds => _customer.ServiceSeconds;
    public BouquetModel DeliveredBouquet
    {
        get
        {
            if (_bouquet == null)
            {
                return null;
            }
            if (_bouquet.Order.BouquetModels == null)
            {
                return null;
            }
            if (_bouquet.Order.BouquetModels.Count == 0)
            {
                return null;
            }
            return _bouquet.Order.BouquetModels[0];
        }
    }
    public OrderInfo OrderInfo => _bouquet != null ? _bouquet.Order : new OrderInfo();
    public DayInfo DayInfo => _dayInfo;
    public DukkanSaveState DukkanState => _dukkanSaveState;
    public EarningsInfo EarningsInfo => _earningsInfo;
    public int CurrentCustomerIndex => _nextCustomerIndex;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private RectTransform _flowerDeliveryArea;
    [SerializeField] private Customer _customer;
    [SerializeField] private Bouquet _bouquet;
    [SerializeField] private PosController _posController;
    [SerializeField] private GameObject _contentObjects;
    [SerializeField] private PaperArea _paperAreaForLoad;
    [SerializeField] private Tutorial _tutorial;
    private DayInfo _dayInfo;
    private ConversationRunner _convoRunner;
    private int _nextCustomerIndex;
    private Sequence _sequence;
    public EarningsInfo _earningsInfo = new();
    public FlowerDeliveredInfo _flowerDeliveredInfo = new();
    private DukkanSaveState _dukkanSaveState = DukkanSaveState.None;
    private DayEvent _newItemInroduceEvent = null;
    private List<string> _convoHistory = new();
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

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        References.DecorationManager.CloseDecorationPageImmediately();
        _contentObjects.SetActive(true);
        References.TopCanvas.Open();

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(_fadeImage.DOFade(endValue: .65f, duration: .3f).SetEase(Ease.Linear));
        _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.Linear));
        _sequence.Join(_canvasGroup.DOFade(endValue: 0, duration: .3f).SetEase(Ease.Linear).OnComplete(() =>
        {
            _fadeImage.color = new Color(0, 0, 0, 0);
            _canvasGroup.alpha = 0;
            onCompleted?.Invoke();
            gameObject.SetActive(false);
        }));
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);

        if (pageData != null && pageData.LoadFromSaveData)
        {
            _canvasGroup.alpha = 1;
            _posController.ResetPos();
            _contentObjects.SetActive(true);
            _customer.gameObject.SetActive(false);
            References.TopCanvas.Open();

            Load(SaveSystem.Inst.SaveData.DukkanParams);
        }
        else
        {
            _dukkanSaveState = DukkanSaveState.CustomerProgress;

            References.HappinessMeter.ResetHappinessMeter();

            _contentObjects.SetActive(false);
            References.TopCanvas.Close();

            _earningsInfo ??= new EarningsInfo();
            _earningsInfo.Reset();

            _canvasGroup.alpha = 0;

            _sequence?.Kill();
            _sequence = DOTween.Sequence();
            _sequence.Append(_fadeImage.DOFade(endValue: .65f, duration: .3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                _contentObjects.SetActive(true);
                References.TopCanvas.Open();
            }));
            _sequence.Append(_fadeImage.DOFade(endValue: 0, duration: .3f).SetEase(Ease.Linear));
            _sequence.Join(_canvasGroup.DOFade(endValue: 1, duration: .3f).SetEase(Ease.Linear).OnComplete(() =>
            {
                onCompleted?.Invoke();

                _nextCustomerIndex = 0;
                if (SaveSystem.Inst.GeneralData.CurrentDayConfigIndex == Configs.LevelConfig.Days.Count)
                {
                    _dayInfo = Configs.LevelConfig.RandomDayInfo;
                }
                else
                {
                    _dayInfo = Configs.LevelConfig.Days[SaveSystem.Inst.GeneralData.CurrentDayConfigIndex];
                }

                _customer.gameObject.SetActive(false);
                _posController.ResetPos();

                References.DayTimeManager.StartDayTimeCountdown(0);
            }));
            Invoke(nameof(StartNextEvent), 1);
        }
    }

    public void OnDayTimeEnded()
    {
        Debug.Log("Day Ended");
        // Finish the current customer's delivery, payment and farewell first.
        // StartNextEvent checks the clock at the next service boundary.
    }

    public void SetDecorationPaused(bool paused)
    {
        References.DayTimeManager.SetDecorationPaused(paused);
        References.HappinessMeter.SetDecorationPaused(paused);
        _customer.SetServiceTimerPaused(paused);
    }

    public void OnFlowerReady(OrderInfo orderInfo, GameObject bouquetObject)
    {
        Debug.Log("Flower Ready");

        SaveSystem.Inst.SaveData.LastPage = PageType.Dukkan;
        _dukkanSaveState = DukkanSaveState.FlowerReady;

        _bouquet.SetOrder(orderInfo, bouquetObject);
        _bouquet.gameObject.SetActive(true);

        // calculate the cost of the bouquet
        int cost = 0;
        foreach (var bouquet in orderInfo.BouquetModels)
        {
            foreach (var flower in bouquet.Flowers)
            {
                cost += flower.Count * Configs.WorkshopConfig.GetFlowerCost(flower.FlowerType, flower.FlowerColor);
            }

            cost += Configs.WorkshopConfig.GetRibbonCost(bouquet.RibbonType);
            cost += Configs.WorkshopConfig.GetWrappingPaperCost(bouquet.WrappingPaperType);
        }
        _earningsInfo.Cost += cost;

        if (!SaveSystem.Inst.SaveData.IsDukkanTutorialFinished)
        {
            ShowGiveFlowerTutorial();
        }
        else
        {
            PrepareHint(HintType.GiveFlower);
        }
    }

    public void OnFlowerDelivered()
    {
        Debug.Log("Flower Delivered");

        SaveSystem.Inst.SaveData.IsDukkanTutorialFinished = true;
        StopHints();

        HapticsController.PlayMediumHaptic();

        _dukkanSaveState = DukkanSaveState.Payment;

        _bouquet.gameObject.SetActive(false);

        References.HappinessMeter.StopHappinessCountdown();
        _customer.StopServiceTimer();

        var pricePaymentInfo = _customer.GetOrderPayment();
        _earningsInfo.CurrentCustomerPrice = pricePaymentInfo.Item1;
        _earningsInfo.CurrentCustomerCash = pricePaymentInfo.Item2;
        _earningsInfo.Price += pricePaymentInfo.Item1;
        _earningsInfo.GivenMoney += pricePaymentInfo.Item2;
        _posController.ReceivePayment(pricePaymentInfo.Item1, pricePaymentInfo.Item2, OnPaymentMade);
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

    public void AddToConvoHistory(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        _convoHistory.Add(message);
    }

    private void OnPaymentMade(int change)
    {
        if (_dukkanSaveState == DukkanSaveState.Done) return;
        _dukkanSaveState = DukkanSaveState.Done;

        // SaveSystem.Inst.GeneralData.ChangeMoney(moneyChange);

        _earningsInfo.Change += change;

        _flowerDeliveredInfo = _customer.GetOrderInfo(_bouquet.Order.BouquetModels, new EarningsInfo
        { Price = _earningsInfo.CurrentCustomerPrice, GivenMoney = _earningsInfo.CurrentCustomerCash, Change = change });

        References.HappinessMeter.SetOrderSatisfaction(_flowerDeliveredInfo.Satisfaction);
        _earningsInfo.Tip += Mathf.RoundToInt(_customer.GetOrderPrice() * _flowerDeliveredInfo.TipPercentage / 100f);

        _convoRunner?.OnConversationEvent.RemoveAllListeners();
        _convoRunner?.OnEnd.RemoveAllListeners();
        _convoRunner = new ConversationRunner(_flowerDeliveredInfo.Conversation);
        _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
        _convoRunner.OnEnd.AddListener(HandleEndEvent);
        _convoRunner.Begin();


    }

    private void SetItems()
    {
        // // outside
        // Sprite outSideSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.OutsideDukkan);
        // if (outSideSprite != null)
        //     _outsideImage.sprite = outSideSprite;

        // // door
        // Sprite doorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Door);
        // if (doorSprite != null)
        //     _doorImage.sprite = doorSprite;

        // // floor
        // Sprite floorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Floor);
        // if (floorSprite != null)
        //     _floorImage.sprite = floorSprite;

        // // left wall
        // Sprite wallSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Wallpaper);
        // if (wallSprite != null)
        //     _leftWallImage.sprite = wallSprite;

        // // right wall
        // if (wallSprite != null)
        //     _rightWallImage.sprite = wallSprite;

        // // flower stand
        // Sprite flowerStandSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.FlowerStand);
        // if (flowerStandSprite != null)
        //     _flowerStandImage.sprite = flowerStandSprite;

        // // decor
        // Sprite decorSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Decor);
        // if (decorSprite != null)
        //     _decorImage.sprite = decorSprite;

        // // table
        // Sprite tableSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Counter);
        // if (tableSprite != null)
        //     _tableImage.sprite = tableSprite;

        // // pc
        // Sprite pcSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Pc);
        // if (pcSprite != null)
        //     _pcImage.sprite = pcSprite;

        // // pos
        // Sprite posSprite = SaveSystem.Inst.ShopData.GetSelectedItemSprite(ItemType.Pos);
        // if (posSprite != null)
        //     _posImage.sprite = posSprite;

        // speech bubbles
        _customer.SetSpeechBubbleSprites();
    }

    private void StartNextEvent() => BeginNextEvent(false);

    private void BeginNextEvent(bool resumeCurrentCustomer)
    {
        if (!resumeCurrentCustomer && References.DayTimeManager.IsDayTimeEnded)
        {
            EndDay();
            return;
        }
        Debug.Log($"#dukkan# StartNextEvent, _currentCustomerIndex: {_nextCustomerIndex}, _dayInfo.Events.Count: {_dayInfo.Events.Count}");
        FirebaseController.Instance.SendCustomEvent($"day_{SaveSystem.Inst.GeneralData.CurrentDayIndex}_customer_{_nextCustomerIndex}");

        _convoHistory.Clear();

        if (_nextCustomerIndex < _dayInfo.Events.Count)
        {
            _dukkanSaveState = DukkanSaveState.CustomerProgress;

            // References.HappinessMeter.ResetHappinessMeter();

            DayEvent dayEvent = _dayInfo.Events[_nextCustomerIndex];
            if (dayEvent.IsEvent)
            {
                _nextCustomerIndex++;
                StartNextEvent();
                return;
            }
            else
            {
                _newItemInroduceEvent = null;

                CustomerType customerType = dayEvent.CustomerType;

                CustomerInfo customer;
                if (customerType == CustomerType.Specific)
                {
                    customer = Configs.CustomerConfig.GetCustomerByName(dayEvent.CustomerName);
                }
                else
                {
                    customer = Configs.CustomerConfig.GetCustomer(customerType);
                }
                customer = CustomerOrderFactory.Prepare(customer, Configs.CustomerConfig, Configs.WorkshopConfig,
                    Configs.ShopConfig, SaveSystem.Inst.ShopData, Configs.EconomyConfig);
                Conversation initialConversation = Configs.CustomerConfig.GetInitialConvo(customer);

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
            _nextCustomerIndex++;
        }
        else
        {
            EndDay();
        }
    }

    private void EndDay()
    {
        if (SaveSystem.Inst.SaveData.LastPage == PageType.EndDay) return;
        Debug.Log("#dukkan# EndDay");

        _earningsInfo.CalculateProfit();
        References.EndDayPage.SetData(_earningsInfo, _newItemInroduceEvent);
        References.EndDayPage.Open();

        Close(onCompleted: () =>
        {
            References.TopCanvas.Close();
        });

        _newItemInroduceEvent = null;
    }

    private void GoToWorkshop()
    {
        Debug.Log("#dukkan# GoToWorkshop");

        StopHints();

        _dukkanSaveState = DukkanSaveState.InWorkshop;
        _customer.StartTimer();

        _convoRunner.OnConversationEvent.RemoveAllListeners();
        _customer.StopTalking();

        References.WorkshopPage.SetOrderCount(_customer.OrderCount)
                                    .SetConvoHistory(_convoHistory)
                                    .Open();
    }


    #region Event Listeners
    private void HandleConversationEvent(IConversationEvent convoEvent)
    {
        Debug.Log($"#dukkan# HandleConversationEvent: {convoEvent}");
        switch (convoEvent)
        {
            case ChoiceEvent choiceEvent:
                string message = _customer.Talk(choiceEvent.Key, choiceEvent.Message, choiceEvent.Options, choiceEvent.ParseOptions);
                _convoHistory.Add(message);
                break;
            case LocalizedMessageEvent localizedMessageEvent:
                message = _customer.Talk(localizedMessageEvent.Key, localizedMessageEvent.Message, () =>
                {
                    localizedMessageEvent.Advance();
                });
                _convoHistory.Add(message);
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
                    _customer.PlayExitAnimation(() => StartNextEvent());
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


    #region Save Load
    private void Load(DukkanParams dukkanParams)
    {
        Debug.Log("#dukkan# Load");

        References.HappinessMeter.ContinueHappinessCountdown(dukkanParams.CurrentTick, dukkanParams.HappinessValue);
        References.DayTimeManager.StartDayTimeCountdown(dukkanParams.TotalTimePassed);

        _posController.ResetPos();
        _earningsInfo = dukkanParams.EarningsInfo;
        _dayInfo = dukkanParams.DayInfo;
        _nextCustomerIndex = dukkanParams.NextCustomerIndex;
        _dukkanSaveState = dukkanParams.DukkanState;
        _convoHistory = dukkanParams.ConvoHistory;

        switch (_dukkanSaveState)
        {
            case DukkanSaveState.None:
            case DukkanSaveState.CustomerProgress:
                _nextCustomerIndex--;
                BeginNextEvent(resumeCurrentCustomer: true);
                break;
            case DukkanSaveState.FlowerReady:
                LoadCustomer(dukkanParams.CurrentCustomerInfo, dukkanParams.CurrentOrder);
                LoadFlower(dukkanParams.OrderInfo, dukkanParams.DeliveredBouquet, true);
                break;
            case DukkanSaveState.Payment:
                LoadCustomer(dukkanParams.CurrentCustomerInfo, dukkanParams.CurrentOrder);
                LoadFlower(dukkanParams.OrderInfo, dukkanParams.DeliveredBouquet, false);
                // Keep the original receipt on resume; never roll a second cash amount.
                if (_earningsInfo.CurrentCustomerPrice == 0)
                {
                    _earningsInfo.CurrentCustomerPrice = _customer.GetOrderPrice();
                    _earningsInfo.CurrentCustomerCash = _earningsInfo.CurrentCustomerPrice;
                }
                _posController.ReceivePayment(_earningsInfo.CurrentCustomerPrice, _earningsInfo.CurrentCustomerCash, OnPaymentMade);
                break;
            case DukkanSaveState.Done:
                StartNextEvent();
                break;
            case DukkanSaveState.InWorkshop:
                LoadCustomer(dukkanParams.CurrentCustomerInfo, dukkanParams.CurrentOrder);
                References.WorkshopPage.SetOrderCount(dukkanParams.CurrentOrder.Count)
                                        .SetConvoHistory(dukkanParams.ConvoHistory)
                                        .Open(new PageParams
                                        {
                                            LoadFromSaveData = true,
                                            PreviousPage = PageType.Dukkan
                                        });
                break;
            default:
                Debug.LogWarning($"#dukkan# Load: Unknown DukkanSaveState: {_dukkanSaveState}");
                break;
        }
    }

    private void LoadCustomer(CustomerInfo customer, List<BouquetModel> currentOrder)
    {
        Sprite customerSprite = Configs.CustomerConfig.GetCustomerSprite(customer.Name);
        _customer.LoadCustomer(customer, customerSprite, currentOrder);
        _customer.RestoreServiceTimer(SaveSystem.Inst.SaveData.DukkanParams.CustomerServiceSeconds,
            _dukkanSaveState == DukkanSaveState.Payment || _dukkanSaveState == DukkanSaveState.Done);
        _customer.EnterWithoutAnimation();
        References.HappinessMeter.StartNewHappinessCountdown();
    }

    private void LoadFlower(OrderInfo orderInfo, BouquetModel bouquetModel, bool activateBouquet)
    {
        if (bouquetModel == null)
        {
            StartNextEvent();
            return;
        }

        try
        {
            _paperAreaForLoad.SetFlowers(bouquetModel);
            _paperAreaForLoad.SetClosedPaper(bouquetModel.WrappingPaperType);
            _paperAreaForLoad.SetRibbon(bouquetModel.RibbonType);
            _paperAreaForLoad.gameObject.SetActive(true);

            Debug.Log("LoadFlower");
            // _bouquet.SetOrder(orderInfo, _paperAreaForLoad.gameObject);
            // _bouquet.gameObject.SetActive(activateBouquet);
            // _paperAreaForLoad.gameObject.SetActive(false);
            _bouquet.gameObject.SetActive(activateBouquet);
            StartCoroutine(SetBouquetRibbon(orderInfo, activateBouquet));
        }
        catch (Exception)
        {
            StartNextEvent();
        }
    }

    private IEnumerator SetBouquetRibbon(OrderInfo orderInfo, bool activateBouquet)
    {
        Debug.Log("SetBouquetRibbon 1");
        // yield return new WaitForEndOfFrame();
        // yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(.1f);
        Debug.Log("SetBouquetRibbon 2");
        _bouquet.SetOrder(orderInfo, _paperAreaForLoad.gameObject);
        _paperAreaForLoad.gameObject.SetActive(false);
    }
    #endregion


    #region Tutorial & Hints
    private void ShowGiveFlowerTutorial()
    {
        FirebaseController.Instance.SendCustomEvent($"tutorial_dukkan_give_flower");
        _tutorial.Init()
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .SwipeBetween(_bouquet.transform.position, _flowerDeliveryArea.transform.position)
                .StartTutorial();
    }

    private void PrepareHint(HintType hintType)
    {
        if (!SaveSystem.Inst.SaveData.IsDukkanTutorialFinished)
        {
            return;
        }

        CancelInvoke();
        switch (hintType)
        {
            case HintType.GiveFlower:
                Invoke(nameof(GiveFlowerHint), Configs.WorkshopConfig.GetHintDelay(SaveSystem.Inst.GeneralData.CurrentDayIndex));
                break;
        }
    }

    private void StopHints()
    {
        FirebaseController.Instance.SendCustomEvent($"StopHints");
        CancelInvoke();
        _tutorial.FinishTutorial();
    }

    private void GiveFlowerHint()
    {
        FirebaseController.Instance.SendCustomEvent($"hint_dukkan_give_flower");
        _tutorial.Init()
                .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                .SwipeBetween(_bouquet.transform.position, _flowerDeliveryArea.position)
                .StartTutorial();
    }
    #endregion
}
