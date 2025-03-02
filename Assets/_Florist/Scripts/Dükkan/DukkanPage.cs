using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Conversa.Runtime;
using Conversa.Runtime.Interfaces;
using Conversa.Runtime.Events;
using static LevelConfig;
using Sirenix.OdinInspector;
using FlowerDeliveredInfo = Customer.FlowerDeliveredInfo;

public class DukkanPage : Page
{
    [SerializeField] private TextMeshProUGUI _moneyAmount, _diamondAmount;
    [SerializeField] private DayTimeManager _dayTimeManager;
    [SerializeField] private HappinessMeter _happinessMeter;
    [SerializeField] private RectTransform _flowerDeliveryArea;
    [SerializeField] private Customer _customer;
    [SerializeField] private Bouquet _bouquet;
    [SerializeField] private PosController _posController;
    private DayInfo _dayInfo;
    private ConversationRunner _convoRunner;
    // private Conversation _currentConversation;
    private int _currentCustomerIndex;


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
        // TODO: delete this line
        Open();

        GeneralData.MoneyAmountChanged += OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged += OnDiamondAmountChanged;
        OnMoneyAmountChanged();
        OnDiamondAmountChanged();
        SetItems();
    }

    private void OnDisable()
    {
        GeneralData.MoneyAmountChanged -= OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged -= OnDiamondAmountChanged;
        CancelInvoke();
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
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

    public void OnFlowerReady(BouquetModel bouquetModel)
    {
        Debug.Log("Flower Ready");
        _posController.ResetPos();
        _bouquet.SetModel(bouquetModel);
        _bouquet.gameObject.SetActive(true);
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

            FlowerDeliveredInfo flowerDeliveredInfo = _customer.GetOrderInfo(_bouquet.Model);
            // TODO: check if order is correct
            // decrease happiness if not
            // increase happiness if correct
            // decrease money with bouquet price
            // increase money with tip
            // start dialogue with goodbye conversation based on happiness
        }

        // TODO: call later when the conversation is over
        _customer.PlayExitAnimation();
        _happinessMeter.StopCountdown();

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
            CustomerInfo customer = Configs.LevelConfig.Customers[customerType];
            Conversation initialConversation = customer.GetInitialConversation();

            _customer.SetCustomer(customer);
            _customer.PlayEnterAnimation(onComplete: () =>
            {
                _convoRunner?.OnConversationEvent.RemoveAllListeners();
                _convoRunner = new ConversationRunner(initialConversation);
                _convoRunner.OnConversationEvent.AddListener(HandleConversationEvent);
                _convoRunner.Begin();
                _happinessMeter.StartCountdown();
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
        _posController.ReceivePayment(payment);

        // TODO: delete this line
        // below must be called a bouquet is ready in the workshop
        BouquetModel bouquetModel = new();
        {
            bouquetModel.Flowers.Add(FlowerType.Gypsum, 4);
            bouquetModel.Flowers.Add(FlowerType.Eucalyptus, 4);
            bouquetModel.Flowers.Add(FlowerType.Daisy, 4);
        }
        OnFlowerReady(bouquetModel);
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
}
