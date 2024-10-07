using TMPro;
using UnityEngine;

public class ShopPage : Page
{
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private ShopItem _shopItemPrefab;
    [SerializeField] private ShopScroll _flowerScroll;
    [SerializeField] private ShopScroll _wrapperScroll;
    [SerializeField] private ShopScroll _ribbonScroll;
    [SerializeField] private ShopScroll _upgradesScroll;
    [SerializeField] private ShopScroll _accessoryScroll;
    [SerializeField] private ShopScroll _wallpaperScroll, _floorScroll, _signScroll;
    [SerializeField] private ShopScroll _counterItemsScroll, _speechBubblesScroll;
    [SerializeField] private TextMeshProUGUI _moneyAmount, _diamondAmount;
    [SerializeField] private GameObject _flowerButton, _bouquetButton, _wrapperButton, _ribbonButton;
    [SerializeField] private GameObject _upgradesButton, _decorationButton, _accessoryButton;
    [SerializeField] private GameObject _wallpaperButton, _floorButton, _signButton;
    [SerializeField] private GameObject _counterItemsButton, _speechBubblesButton;
    private ShopScroll _currentScroll;

    private void OnEnable()
    {
        GeneralData.MoneyAmountChanged += OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged += OnDiamondAmountChanged;
        OnMoneyAmountChanged();
        OnDiamondAmountChanged();
    }

    private void Start()
    {
        _currentScroll = _flowerScroll;
        _flowerScroll.Init(Configs.ShopConfig.FlowerItems, _shopItemPrefab);
        _flowerScroll.Open();
    }

    private void OnDisable()
    {
        GeneralData.MoneyAmountChanged -= OnMoneyAmountChanged;
        GeneralData.DiamondAmountChanged -= OnDiamondAmountChanged;
    }

    public override void Close()
    {
        gameObject.SetActive(false);
        _mainMenu.SetActive(true);
    }

    public override void Open()
    {
        gameObject.SetActive(true);
    }


    #region Buttons
    public void OnBouquetClicked()
    {
        bool isActive = _wrapperButton.activeSelf;
        _wrapperButton.SetActive(!isActive);
        _ribbonButton.SetActive(!isActive);
    }

    public void OnDecorationClicked()
    {
        bool isActive = _accessoryButton.activeSelf;
        _accessoryButton.SetActive(!isActive);
        _wallpaperButton.SetActive(!isActive);
        _floorButton.SetActive(!isActive);
        _signButton.SetActive(!isActive);
        _counterItemsButton.SetActive(!isActive);
        _speechBubblesButton.SetActive(!isActive);
    }

    public void OnFlowerClicked()
    {
        _currentScroll.Close();
        _flowerScroll.Init(Configs.ShopConfig.FlowerItems, _shopItemPrefab);
        _flowerScroll.Open();
        _currentScroll = _flowerScroll;
    }

    public void OnUpgradesClicked()
    {
        _currentScroll.Close();
        _upgradesScroll.Init(Configs.ShopConfig.UpgradeItems, _shopItemPrefab);
        _upgradesScroll.Open();
        _currentScroll = _upgradesScroll;
    }

    public void OnWrapperClicked()
    {
        _currentScroll.Close();
        _wrapperScroll.Init(Configs.ShopConfig.WrapperItems, _shopItemPrefab);
        _wrapperScroll.Open();
        _currentScroll = _wrapperScroll;
    }

    public void OnRibbonClicked()
    {
        _currentScroll.Close();
        _ribbonScroll.Init(Configs.ShopConfig.RibbonItems, _shopItemPrefab);
        _ribbonScroll.Open();
        _currentScroll = _ribbonScroll;
    }

    public void OnAccessoryClicked()
    {
        _currentScroll.Close();
        _accessoryScroll.Init(Configs.ShopConfig.AccessoryItems, _shopItemPrefab);
        _accessoryScroll.Open();
        _currentScroll = _accessoryScroll;
    }

    public void OnWallpaperClicked()
    {
        _currentScroll.Close();
        _wallpaperScroll.Init(Configs.ShopConfig.WallpaperItems, _shopItemPrefab);
        _wallpaperScroll.Open();
        _currentScroll = _wallpaperScroll;
    }

    public void OnFloorClicked()
    {
        _currentScroll.Close();
        _floorScroll.Init(Configs.ShopConfig.FloorItems, _shopItemPrefab);
        _floorScroll.Open();
        _currentScroll = _floorScroll;
    }

    public void OnSignClicked()
    {
        _currentScroll.Close();
        _signScroll.Init(Configs.ShopConfig.SignItems, _shopItemPrefab);
        _signScroll.Open();
        _currentScroll = _signScroll;
    }

    public void OnCounterItemsClicked()
    {
        _currentScroll.Close();
        _counterItemsScroll.Init(Configs.ShopConfig.CounterItems, _shopItemPrefab);
        _counterItemsScroll.Open();
        _currentScroll = _counterItemsScroll;
    }

    public void OnSpeechBubblesClicked()
    {
        _currentScroll.Close();
        _speechBubblesScroll.Init(Configs.ShopConfig.SpeechBubbleItems, _shopItemPrefab);
        _speechBubblesScroll.Open();
        _currentScroll = _speechBubblesScroll;
    }
    #endregion


    private void OnMoneyAmountChanged()
    {
        _moneyAmount.text = SaveSystem.Inst.GeneralData.Money.ToString();
    }

    private void OnDiamondAmountChanged()
    {
        _diamondAmount.text = SaveSystem.Inst.GeneralData.Diamonds.ToString();
    }
}
