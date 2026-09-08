using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopItem : MonoBehaviour
{
    [SerializeField] private Sprite _lockedBgSprite;
    [SerializeField] private Image _bg;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private TextMeshProUGUI _buttonText;
    [SerializeField] private TextMeshProUGUI _lockText;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _lock;
    [SerializeField] private GameObject _purchasedObject;
    [SerializeField] private Button _button;
    [SerializeField] private Image _buyButtonImage;
    [SerializeField] private Material _disabledButtonMaterial;
    [SerializeField] private TextMeshProUGUI _notEnoughMoneyText;
    [SerializeField, Min(0f)] private float _notEnoughMoneyMoveDistance = 50f;
    [SerializeField, Min(0.01f)] private float _notEnoughMoneyDuration = .65f;
    [SerializeField] private TextMeshProUGUI _purchaseFeedbackText;
    [SerializeField] private Image _purchaseGlow;
    [SerializeField] private AudioSource _purchaseAudio;
    [SerializeField] private AudioClip _purchaseSound;
    private Vector3 _iconStartScale;
    private Vector2 _buttonStartPosition;
    private Sequence _purchaseSequence;
    private Tween _buttonShake;
    private Action _onPurchaseFeedbackCompleted;
    private Material _defaultButtonMaterial;
    private Vector2 _notEnoughMoneyStartPosition;
    private Color _notEnoughMoneyStartColor;
    private Sequence _notEnoughMoneySequence;
    private ShopItemInfo _shopItemInfo;
    private ShopScroll _shopScroll;
    private int _index;
    private Sprite _itemBg;

    public float Width => ((RectTransform)transform).sizeDelta.x;

    private void Awake()
    {
        _defaultButtonMaterial = _buyButtonImage.material;
        _notEnoughMoneyStartPosition = _notEnoughMoneyText.rectTransform.anchoredPosition;
        _notEnoughMoneyStartColor = _notEnoughMoneyText.color;
        _notEnoughMoneyText.gameObject.SetActive(false);
        _iconStartScale = _icon.rectTransform.localScale;
        _buttonStartPosition = _buyButtonImage.rectTransform.anchoredPosition;
        _purchaseFeedbackText.gameObject.SetActive(false);
        _purchaseGlow.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GeneralData.MoneyAmountChanged += Refresh;
        Refresh();
    }

    private void OnDisable()
    {
        GeneralData.MoneyAmountChanged -= Refresh;
        ResetNotEnoughMoneyFeedback();
        CompletePurchaseFeedback();
    }

    public void Refresh()
    {
        if (_shopItemInfo != null) Init(_shopItemInfo, _shopScroll, _index, _itemBg);
    }

    public void Init(ShopItemInfo shopItemInfo, ShopScroll shopScroll, int index, Sprite itemBg)
    {
        _itemBg = itemBg;
        _bg.sprite = itemBg;
        _shopScroll = shopScroll;
        _index = index;
        _shopItemInfo = shopItemInfo;
        _icon.sprite = shopItemInfo.Icon;

        string[] nameArray = shopItemInfo.Name.Split('_');
        if (!string.IsNullOrWhiteSpace(shopItemInfo.NameLocalizationKey))
        {
            _nameText.text = LocalizationManager.GetLocalizedText(shopItemInfo.NameLocalizationKey) ?? shopItemInfo.Name;
        }
        else if (nameArray.Length > 1)
        {
            _nameText.text = $"{LocalizationManager.GetLocalizedText(nameArray[1])} {LocalizationManager.GetLocalizedText(nameArray[0])}";
        }
        else
        {
            _nameText.text = LocalizationManager.GetLocalizedText(shopItemInfo.Name);
        }

        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id, _shopItemInfo.UnlockDay);
        float money = SaveSystem.Inst.GeneralData.Money;
        bool isUnaffordable = itemState == ShopData.ItemState.Purchasable && money < shopItemInfo.Price;
        _buyButtonImage.material = isUnaffordable ? _disabledButtonMaterial : _defaultButtonMaterial;

        if (shopItemInfo.PurchaseDisabled)
        {
            _lock.SetActive(true);
            _button.gameObject.SetActive(false);
            _lockText.text = LocalizationManager.GetLocalizedText("coming_soon");
            return;
        }

        if (shopItemInfo.IsSelectable)
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    // _purchasedObject.SetActive(false);
                    _button.gameObject.SetActive(false);
                    _bg.sprite = _lockedBgSprite;
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    _lockText.text = string.Format(LocalizationManager.GetLocalizedText("unlock_day"), shopItemInfo.UnlockDay + 1);
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    // _purchasedObject.SetActive(false);
                    _button.gameObject.SetActive(true);
                    _button.interactable = true;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    // _purchasedObject.SetActive(true);
                    _button.gameObject.SetActive(true);
                    _button.interactable = true;
                    _buttonText.text = LocalizationManager.GetLocalizedText("select");
                    break;
                case ShopData.ItemState.Selected:
                    _lock.SetActive(false);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("selected");
                    break;
            }
        }
        else
        {
            switch (itemState)
            {
                case ShopData.ItemState.Locked:
                    _lock.SetActive(true);
                    // _purchasedObject.SetActive(false);
                    _button.gameObject.SetActive(false);
                    _bg.sprite = _lockedBgSprite;
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("locked");
                    _lockText.text = string.Format(LocalizationManager.GetLocalizedText("unlock_day"), shopItemInfo.UnlockDay + 1);
                    break;
                case ShopData.ItemState.Purchasable:
                    _lock.SetActive(false);
                    // _purchasedObject.SetActive(false);
                    _button.gameObject.SetActive(true);
                    _button.interactable = true;
                    _buttonText.text = $"<sprite=0> {shopItemInfo.Price}";
                    break;
                case ShopData.ItemState.Purchased:
                    _lock.SetActive(false);
                    // _purchasedObject.SetActive(true);
                    _button.gameObject.SetActive(true);
                    _button.interactable = false;
                    _buttonText.text = LocalizationManager.GetLocalizedText("owned");
                    break;
            }
        }
        if (_purchaseSequence != null) _button.interactable = false;
    }

    public void SetSelected(bool isSelected)
    {
        if (!_shopItemInfo.IsSelectable || _shopItemInfo.PurchaseDisabled) return;
        ShopData.ItemState state = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id, _shopItemInfo.UnlockDay);
        if (state != ShopData.ItemState.Purchased && state != ShopData.ItemState.Selected) return;
        if (isSelected) SaveSystem.Inst.ShopData.SetSelectedState(_shopItemInfo.Id);
        else SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
        Refresh();
    }

    public void OnButtonClicked()
    {
        if (_shopItemInfo == null || _shopItemInfo.PurchaseDisabled || _purchaseSequence != null) return;
        ShopData.ItemState itemState = SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id, _shopItemInfo.UnlockDay);
        switch (itemState)
        {
            case ShopData.ItemState.Purchasable:
                TryPurchase();
                break;
            case ShopData.ItemState.Purchased:
                if (_shopItemInfo.IsSelectable) _shopScroll.OnItemSelected(_index);
                break;
        }

        Init(_shopItemInfo, _shopScroll, _index, _itemBg);
    }

    public bool TryPurchase(Action onFeedbackCompleted = null)
    {
        if (!isActiveAndEnabled || _shopItemInfo == null || _shopItemInfo.PurchaseDisabled ||
            _purchaseSequence != null || _shopItemInfo.Price < 0 ||
            SaveSystem.Inst.ShopData.GetItemState(_shopItemInfo.Id, _shopItemInfo.UnlockDay) != ShopData.ItemState.Purchasable)
            return false;

        if (SaveSystem.Inst.GeneralData.Money < _shopItemInfo.Price)
        {
            PlayNotEnoughMoneyFeedback();
            return false;
        }

        SaveSystem.Inst.ShopData.SetPurchasedState(_shopItemInfo.Id);
        SaveSystem.Inst.GeneralData.ChangeMoney(-_shopItemInfo.Price);
        Refresh();
        ResetNotEnoughMoneyFeedback();
        _onPurchaseFeedbackCompleted = onFeedbackCompleted;
        _button.interactable = false;
        _purchaseFeedbackText.text = LocalizationManager.GetLocalizedText("shop_purchase_success");
        _purchaseFeedbackText.alpha = 0f;
        _purchaseFeedbackText.gameObject.SetActive(true);
        _purchaseGlow.sprite = _icon.sprite;
        _purchaseGlow.color = new Color(1f, .83f, .35f, .7f);
        _purchaseGlow.rectTransform.localScale = _iconStartScale;
        _purchaseGlow.gameObject.SetActive(true);
        if (SaveSystem.Inst.GeneralData.IsSoundOn) _purchaseAudio.PlayOneShot(_purchaseSound);
        if (SaveSystem.Inst.GeneralData.IsVibrationOn) HapticsController.PlayLightHaptic();

        // Independent time keeps shop feedback visible while gameplay is paused.
        _purchaseSequence = DOTween.Sequence().SetUpdate(true)
            .Append(_icon.rectTransform.DOScale(_iconStartScale * 1.16f, .18f).SetEase(Ease.OutQuad))
            .Append(_icon.rectTransform.DOScale(_iconStartScale, .25f).SetEase(Ease.OutBack))
            .Insert(0f, _purchaseGlow.rectTransform.DOScale(_iconStartScale * 1.4f, .55f).SetEase(Ease.OutCubic))
            .Insert(0f, _purchaseGlow.DOFade(0f, .55f))
            .Insert(0f, _purchaseFeedbackText.DOFade(1f, .15f))
            .AppendInterval(.5f)
            .Append(_purchaseFeedbackText.DOFade(0f, .2f))
            .OnComplete(CompletePurchaseFeedback);
        return true;
    }

    private void CompletePurchaseFeedback()
    {
        _purchaseSequence?.Kill();
        _purchaseSequence = null;
        _icon.rectTransform.localScale = _iconStartScale;
        _purchaseFeedbackText.gameObject.SetActive(false);
        _purchaseGlow.gameObject.SetActive(false);
        _purchaseAudio.Stop();
        // A committed purchase must also release its caller if the card is hidden early.
        Action onCompleted = _onPurchaseFeedbackCompleted;
        _onPurchaseFeedbackCompleted = null;
        if (isActiveAndEnabled) Refresh();
        onCompleted?.Invoke();
    }

    public void SetItemUnlocked()
    {
        // The calendar controls availability; a tutorial must never bypass it or erase ownership.
        Refresh();
    }

    private void PlayNotEnoughMoneyFeedback()
    {
        ResetNotEnoughMoneyFeedback();
        _notEnoughMoneyText.text = LocalizationManager.GetLocalizedText("decoration_not_enough_money");
        _buttonShake = _buyButtonImage.rectTransform
            .DOShakeAnchorPos(.3f, new Vector2(9f, 0f), 12, 0f).SetUpdate(true);

        RectTransform feedbackTransform = _notEnoughMoneyText.rectTransform;
        _notEnoughMoneyText.gameObject.SetActive(true);

        _notEnoughMoneySequence = DOTween.Sequence().SetUpdate(true)
            .Append(feedbackTransform
                .DOAnchorPosY(_notEnoughMoneyStartPosition.y + _notEnoughMoneyMoveDistance,
                    _notEnoughMoneyDuration)
                .SetEase(Ease.OutCubic))
            .Join(_notEnoughMoneyText
                .DOFade(0f, _notEnoughMoneyDuration)
                .SetEase(Ease.InQuad))
            .OnComplete(() =>
            {
                _notEnoughMoneySequence = null;
                _notEnoughMoneyText.gameObject.SetActive(false);
                feedbackTransform.anchoredPosition = _notEnoughMoneyStartPosition;
                _notEnoughMoneyText.color = _notEnoughMoneyStartColor;
            });
    }

    private void ResetNotEnoughMoneyFeedback()
    {
        _buttonShake?.Kill();
        _buttonShake = null;
        _buyButtonImage.rectTransform.anchoredPosition = _buttonStartPosition;
        _notEnoughMoneySequence?.Kill();
        _notEnoughMoneySequence = null;
        _notEnoughMoneyText.rectTransform.anchoredPosition = _notEnoughMoneyStartPosition;
        _notEnoughMoneyText.color = _notEnoughMoneyStartColor;
        _notEnoughMoneyText.gameObject.SetActive(false);
    }
}
