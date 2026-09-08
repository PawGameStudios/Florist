using System;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DecorationItem : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private GameObject _selected;
    [SerializeField] private GameObject _lock;
    [SerializeField] private Image _buyButtonImage;
    [SerializeField] private Material _disabledButtonMaterial;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _lockText;
    [SerializeField] private TextMeshProUGUI _notEnoughMoneyText;
    [SerializeField, Min(0f)] private float _notEnoughMoneyMoveDistance = 50f;
    [SerializeField, Min(0.01f)] private float _notEnoughMoneyDuration = .65f;

    private ShopConfig.DecorationItemInfo _item;
    private Func<ShopConfig.DecorationItemInfo, bool> _onClicked;
    private Material _defaultButtonMaterial;
    private Vector2 _notEnoughMoneyStartPosition;
    private Color _notEnoughMoneyStartColor;
    private Sequence _notEnoughMoneySequence;

    private void Awake()
    {
        _defaultButtonMaterial = _buyButtonImage.material;
        _notEnoughMoneyStartPosition = _notEnoughMoneyText.rectTransform.anchoredPosition;
        _notEnoughMoneyStartColor = _notEnoughMoneyText.color;
        _notEnoughMoneyText.text = LocalizationManager.GetLocalizedText("decoration_not_enough_money");
        _notEnoughMoneyText.gameObject.SetActive(false);
    }

    private void OnDisable()
    {
        ResetNotEnoughMoneyFeedback();
    }

    public void Initialize(ShopConfig.DecorationItemInfo item, Func<ShopConfig.DecorationItemInfo, bool> onClicked)
    {
        _item = item;
        _onClicked = onClicked;
        _icon.sprite = item.Icon != null ? item.Icon : item.DecorationSprite;
        Refresh();
    }

    public void Refresh()
    {
        if (_item == null || SaveSystem.Inst?.ShopData == null)
            return;

        ShopData.ItemState state = SaveSystem.Inst.ShopData.GetItemState(_item.Id, _item.UnlockDay);
        bool hasMoney = SaveSystem.Inst.GeneralData.Money >= _item.Price;
        bool isSelected = state == ShopData.ItemState.Selected;
        bool isUnaffordable = state == ShopData.ItemState.Purchasable && !hasMoney;
        bool canUseButton = state == ShopData.ItemState.Purchasable || state == ShopData.ItemState.Purchased;

        _selected.SetActive(isSelected);
        _lock.SetActive(state == ShopData.ItemState.Locked);
        _button.gameObject.SetActive(canUseButton);
        _buyButtonImage.material = isUnaffordable ? _disabledButtonMaterial : _defaultButtonMaterial;
        _button.interactable = canUseButton;

        _lockText.text = state == ShopData.ItemState.Locked
            ? string.Format(LocalizationManager.GetLocalizedText("unlock_day"), _item.UnlockDay + 1)
            : string.Empty;

        _statusText.text = state switch
        {
            ShopData.ItemState.Purchasable when _item.Price > 0 => string.Format(
                LocalizationManager.GetLocalizedText("decoration_price"), _item.Price),
            ShopData.ItemState.Purchasable => LocalizationManager.GetLocalizedText("select"),
            ShopData.ItemState.Purchased => LocalizationManager.GetLocalizedText("select"),
            ShopData.ItemState.Selected => LocalizationManager.GetLocalizedText("selected"),
            _ => string.Empty
        };
    }

    public void OnButtonClicked()
    {
        if (_item == null || SaveSystem.Inst?.ShopData == null)
            return;

        ShopData.ItemState state = SaveSystem.Inst.ShopData.GetItemState(_item.Id, _item.UnlockDay);
        if (state == ShopData.ItemState.Purchasable && SaveSystem.Inst.GeneralData.Money < _item.Price)
        {
            PlayNotEnoughMoneyFeedback();
            return;
        }

        if (_onClicked?.Invoke(_item) ?? false)
            Refresh();
    }

    private void PlayNotEnoughMoneyFeedback()
    {
        ResetNotEnoughMoneyFeedback();
        _notEnoughMoneyText.text = LocalizationManager.GetLocalizedText("decoration_not_enough_money");

        RectTransform feedbackTransform = _notEnoughMoneyText.rectTransform;
        _notEnoughMoneyText.gameObject.SetActive(true);

        _notEnoughMoneySequence = DOTween.Sequence()
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
        _notEnoughMoneySequence?.Kill();
        _notEnoughMoneySequence = null;
        _notEnoughMoneyText.rectTransform.anchoredPosition = _notEnoughMoneyStartPosition;
        _notEnoughMoneyText.color = _notEnoughMoneyStartColor;
        _notEnoughMoneyText.gameObject.SetActive(false);
    }
}
