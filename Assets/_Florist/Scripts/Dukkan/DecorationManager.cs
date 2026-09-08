using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class DecorationManager : MonoBehaviour
{
    public enum DecorationType
    {
        Chandeliers,
        Wall,
        Curtains,
        Shelves,
        Paintings,
        Trim
    }

    [Serializable]
    private class DecorationImageGroup
    {
        public DecorationType Type;
        public Image[] Images;
    }

    [Header("UI")]
    [SerializeField] private DecorationOverlay _decorationOverlay;
    [SerializeField] private DecorationScroll _decorationScrollPrefab;
    [SerializeField] private DecorationItem _decorationItemPrefab;
    [SerializeField] private RectTransform _decorationScrollParent;
    [SerializeField] private Button[] _decorationButtons;

    [Header("Dukkan")]
    [SerializeField] private RectTransform _dukkanContent;
    [SerializeField] private GameObject _customerRoot;
    [SerializeField] private RectTransform _mainCanvasOriginalPlaceholder;
    [SerializeField] private RectTransform _decorationModePlaceholder;
    [SerializeField] private RectTransform[] _decoCanvasPlaceholders;
    [SerializeField] private RectTransform[] _decoScrollPlaceholders;
    [SerializeField] private DecorationImageGroup[] _decorationImages;
    [SerializeField, Min(0f)] private float _transitionDuration = .35f;

    private DecorationScroll _activeScroll;
    private DecorationType _currentDecorationType;
    private bool _customerWasActive;
    private bool _isDecorationModeOpen;

    private void Awake()
    {
        for (int i = 0; i < _decorationButtons.Length; i++)
        {
            int categoryIndex = i;
            _decorationButtons[i].onClick.AddListener(() => OnDecorationTypeChangeButtonClicked(categoryIndex));
        }

        _decorationOverlay.Initialize(OpenDecorationPage, CloseDecorationPage, OnDecorationBackClicked);
    }

    private void Start()
    {
        ApplySavedDecorations();
    }

    private void OnDestroy()
    {
        _dukkanContent.DOKill();
    }

    private void OnDisable()
    {
        CloseDecorationPageImmediately();
    }

    public void OpenDecorationPage()
    {
        if (_isDecorationModeOpen)
            return;

        References.DukkanPage.SetDecorationPaused(true);
        _customerWasActive = _customerRoot.activeSelf;
        _customerRoot.SetActive(false);
        _isDecorationModeOpen = true;

        _decorationOverlay.ShowOverview();
        MoveContent(_decorationModePlaceholder);
    }

    public void CloseDecorationPage()
    {
        if (!_isDecorationModeOpen)
            return;

        _isDecorationModeOpen = false;
        ClearActiveScroll();
        _decorationOverlay.HideMode();
        MoveContent(_mainCanvasOriginalPlaceholder);
        _customerRoot.SetActive(_customerWasActive);
        References.DukkanPage.SetDecorationPaused(false);
    }

    public void CloseDecorationPageImmediately()
    {
        if (!_isDecorationModeOpen)
            return;

        _isDecorationModeOpen = false;
        ClearActiveScroll();
        _decorationOverlay.HideMode();
        _dukkanContent.DOKill();
        CopyTransform(_dukkanContent, _mainCanvasOriginalPlaceholder);
        _customerRoot.SetActive(_customerWasActive);
        References.DukkanPage.SetDecorationPaused(false);
    }

    public void OnCloseButtonClicked()
    {
        CloseDecorationPage();
    }

    public void OnDecorationTypeChangeButtonClicked(int id)
    {
        if (!_isDecorationModeOpen || id < 0 || id >= _decoCanvasPlaceholders.Length ||
            id >= _decoScrollPlaceholders.Length || !Enum.IsDefined(typeof(DecorationType), id))
        {
            return;
        }

        DecorationType decorationType = (DecorationType)id;
        ShopConfig.DecorationItemGroup group = Configs.ShopConfig.GetDecorationGroup(decorationType);
        if (group == null || group.Items == null || group.Items.Count == 0)
            return;

        _currentDecorationType = decorationType;
        ClearActiveScroll();

        _activeScroll = Instantiate(_decorationScrollPrefab, _decorationScrollParent);
        CopyTransform(_activeScroll.Root, _decoScrollPlaceholders[id]);
        _activeScroll.Initialize(group.Items, _decorationItemPrefab, TrySelectDecoration);

        _decorationOverlay.ShowCategory();
        MoveContent(_decoCanvasPlaceholders[id]);
    }

    public void OnDecorationBackClicked()
    {
        if (!_isDecorationModeOpen)
            return;

        ClearActiveScroll();
        _decorationOverlay.ShowOverview();
        MoveContent(_decorationModePlaceholder);
    }

    public void OnDecorationButtonClicked(int id)
    {
        List<ShopConfig.DecorationItemInfo> items = Configs.ShopConfig.GetDecorationItems(_currentDecorationType);
        if (items == null || id < 0 || id >= items.Count)
            return;

        TrySelectDecoration(items[id]);
    }

    public bool TrySelectDecoration(ShopConfig.DecorationItemInfo item)
    {
        if (item == null || SaveSystem.Inst?.ShopData == null)
            return false;

        ShopData.ItemState state = SaveSystem.Inst.ShopData.GetItemState(item.Id, item.UnlockDay);
        if (state == ShopData.ItemState.Locked)
            return false;

        if (state == ShopData.ItemState.Purchasable)
        {
            if (SaveSystem.Inst.GeneralData.Money < item.Price)
                return false;

            SaveSystem.Inst.GeneralData.ChangeMoney(-item.Price);
            SaveSystem.Inst.ShopData.SetPurchasedState(item.Id);
        }

        SaveSystem.Inst.ShopData.SelectDecoration(_currentDecorationType, item.Id);
        ApplyDecoration(_currentDecorationType, item.DecorationSprite);
        if (_activeScroll != null)
            _activeScroll.RefreshItems();
        return true;
    }

    public void ApplySavedDecorations()
    {
        if (Configs.ShopConfig?.DecorationItems == null || SaveSystem.Inst?.ShopData == null)
            return;

        foreach (ShopConfig.DecorationItemGroup group in Configs.ShopConfig.DecorationItems)
        {
            foreach (ShopConfig.DecorationItemInfo item in group.Items)
            {
                if (!SaveSystem.Inst.ShopData.IsSelected(item.Id))
                    continue;

                ApplyDecoration(group.Type, item.DecorationSprite);
                break;
            }
        }
    }

    private void ApplyDecoration(DecorationType type, Sprite sprite)
    {
        if (_decorationImages == null)
            return;

        foreach (DecorationImageGroup imageGroup in _decorationImages)
        {
            if (imageGroup == null || imageGroup.Type != type)
                continue;

            if (imageGroup.Images == null)
            {
                Debug.LogWarning($"DecorationManager: No image array assigned for {type}.", this);
                return;
            }

            for (int i = 0; i < imageGroup.Images.Length; i++)
            {
                Image image = imageGroup.Images[i];
                if (image == null)
                {
                    Debug.LogWarning($"DecorationManager: Missing image for {type} at index {i}. Assign it in the Decoration Images list.", this);
                    continue;
                }

                image.sprite = sprite;
                image.enabled = sprite != null;
            }

            return;
        }
    }

    private void MoveContent(RectTransform placeholder)
    {
        _dukkanContent.DOKill();
        _dukkanContent.DOAnchorPos(placeholder.anchoredPosition, _transitionDuration).SetEase(Ease.OutCubic);
        _dukkanContent.DOScale(placeholder.localScale, _transitionDuration).SetEase(Ease.OutCubic);
        _dukkanContent.DOLocalRotate(placeholder.localEulerAngles, _transitionDuration).SetEase(Ease.OutCubic);
    }

    private void ClearActiveScroll()
    {
        if (_activeScroll == null)
            return;

        Destroy(_activeScroll.gameObject);
        _activeScroll = null;
    }

    private static void CopyTransform(RectTransform target, RectTransform placeholder)
    {
        target.anchoredPosition = placeholder.anchoredPosition;
        target.localRotation = placeholder.localRotation;
        target.localScale = placeholder.localScale;
    }
}
