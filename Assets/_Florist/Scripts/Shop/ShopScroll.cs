using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using ShopItemInfo = ShopConfig.ShopItemInfo;

public class ShopScroll : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private Transform _shopItemParent;
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private RectTransform _viewport;
    private bool _isInitialized = false;
    private Tween _scrollTween;
    private bool _restoreScrollRect;
    private readonly Vector3[] _corners = new Vector3[4];
    private readonly List<ShopItem> _items = new();
    private List<int> _displayOrder;
    private const float DURATION = .8f;
    private const Ease SCROLL_EASE = Ease.OutQuint;

    private void OnDisable()
    {
        _scrollTween?.Kill();
        RestoreScrollRect();
    }

    public void Init(List<ShopItemInfo> items, ShopItem prefab, Sprite itemBg)
    {
        if (_isInitialized)
        {
            foreach (var item in _items) item.Refresh();
            return;
        }

        foreach (var item in items)
        {
            var shopItem = Instantiate(prefab, _shopItemParent);
            shopItem.Init(item, this, _items.Count, itemBg);
            _items.Add(shopItem);
        }

        // Keep config indices for tutorial callbacks; only change the visual order.
        _displayOrder = Enumerable.Range(0, items.Count)
            .OrderBy(index => items[index].UnlockDay)
            .ToList();
        for (int displayIndex = 0; displayIndex < _displayOrder.Count; displayIndex++)
            _items[_displayOrder[displayIndex]].transform.SetSiblingIndex(displayIndex);

        _isInitialized = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
        foreach (var item in _items) item.Refresh();
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void OnItemSelected(int index)
    {
        for (int i = 0; i < _items.Count; i++)
        {
            if (i == index)
                _items[i].SetSelected(true);
            else
                _items[i].SetSelected(false);
        }
    }

    public void SetScrollToItem(int itemIndex, Action onComplete = null)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return;
        }

        SetScrollPosition(itemIndex, onComplete);
    }

    public Transform GetItemTransform(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return null;
        }
        return _items[itemIndex].transform;
    }

    public void SetItemUnlocked(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return;
        }
        _items[itemIndex].SetItemUnlocked();
    }

    public bool SimulateButtonClick(int itemIndex, Action onFeedbackCompleted = null)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return false;
        }
        return _items[itemIndex].TryPurchase(onFeedbackCompleted);
    }

    private void SetScrollPosition(int itemIndex, Action onComplete)
    {
        _scrollTween?.Kill();
        RestoreScrollRect();
        _scrollRect.StopMovement();
        _restoreScrollRect = _scrollRect.enabled;
        _scrollRect.enabled = false;

        // Resolve layout after sibling sorting and before measuring actual card positions.
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollContent);
        Canvas.ForceUpdateCanvases();

        Bounds contentBounds = GetBoundsInViewport(_scrollContent);
        Bounds itemBounds = GetBoundsInViewport((RectTransform)_items[itemIndex].transform);
        Rect view = _viewport.rect;
        float offset = view.center.x - itemBounds.center.x;
        if (contentBounds.size.x > view.width)
            offset = Mathf.Clamp(offset, view.xMax - contentBounds.max.x, view.xMin - contentBounds.min.x);
        else
            offset = view.xMin - contentBounds.min.x;

        // Convert the viewport displacement into the content parent's coordinate space.
        Vector3 worldOffset = _viewport.TransformVector(new Vector3(offset, 0, 0));
        float targetX = _scrollContent.anchoredPosition.x +
            _scrollContent.parent.InverseTransformVector(worldOffset).x;
        _scrollTween = _scrollContent.DOAnchorPosX(targetX, DURATION).SetEase(SCROLL_EASE).OnComplete(() =>
        {
            _scrollTween = null;
            RestoreScrollRect();
            Canvas.ForceUpdateCanvases();
            onComplete?.Invoke();
        });
    }

    private Bounds GetBoundsInViewport(RectTransform rect)
    {
        rect.GetWorldCorners(_corners);
        var bounds = new Bounds(_viewport.InverseTransformPoint(_corners[0]), Vector3.zero);
        for (int i = 1; i < _corners.Length; i++)
            bounds.Encapsulate(_viewport.InverseTransformPoint(_corners[i]));
        return bounds;
    }

    private void RestoreScrollRect()
    {
        if (!_restoreScrollRect) return;
        _scrollRect.StopMovement();
        _scrollRect.enabled = true;
        _restoreScrollRect = false;
    }
}
