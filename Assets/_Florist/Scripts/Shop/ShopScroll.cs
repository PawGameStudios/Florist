using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using ShopItemInfo = ShopConfig.ShopItemInfo;
using MEC;

public class ShopScroll : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private Transform _shopItemParent;
    private bool _isInitialized = false;
    private Tween _scrollTween;
    private float _itemWidth;
    private readonly List<ShopItem> _items = new();
    private const float DURATION = .8f;
    private const float SCROLL_SPACING = 10f;
    private const Ease SCROLL_EASE = Ease.OutQuint;

    private void OnDisable()
    {
        _scrollTween?.Kill();
    }

    public void Init(List<ShopItemInfo> items, ShopItem prefab, Sprite itemBg)
    {
        if (_isInitialized)
            return;

        foreach (var item in items)
        {
            var shopItem = Instantiate(prefab, _shopItemParent);
            shopItem.Init(item, this, _items.Count, itemBg);
            _items.Add(shopItem);
        }

        _itemWidth = _items[0].GetComponent<RectTransform>().sizeDelta.x;

        _isInitialized = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
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

        Timing.RunCoroutine(SetScrollPosition(itemIndex, onComplete).CancelWith(gameObject));
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

    public void SimulateButtonClick(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return;
        }
        _items[itemIndex].OnButtonClicked();
    }

    private IEnumerator<float> SetScrollPosition(int itemIndex, Action onComplete)
    {
        yield return Timing.WaitForSeconds(1);

        var targetPosX = _scrollContent.localPosition.x;
        targetPosX -= _itemWidth * itemIndex + SCROLL_SPACING * (itemIndex - 1);

        Debug.Log($"current pos: {_scrollContent.localPosition.x}, target pos: {targetPosX}");

        _scrollTween?.Kill();
        _scrollTween = _scrollContent.DOLocalMoveX(targetPosX, DURATION).SetEase(SCROLL_EASE).OnComplete(() => onComplete?.Invoke());
    }
}

public class DecorationScroll : MonoBehaviour
{
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private Transform _shopItemParent;
    private bool _isInitialized = false;
    private Tween _scrollTween;
    private float _itemWidth;
    private DecorationManager.DecorationType _decorationType;
    private readonly List<DecorationItem> _items = new();
    private const float DURATION = .8f;
    private const float SCROLL_SPACING = 10f;
    private const Ease SCROLL_EASE = Ease.OutQuint;

    private void OnDisable()
    {
        _scrollTween?.Kill();
    }

    public void Init(List<ShopItemInfo> items, DecorationItem prefab, DecorationManager.DecorationType decorationType)
    {
        if (_isInitialized)
            return;

        _decorationType = decorationType;

        foreach (var item in items)
        {
            var decorationItem = Instantiate(prefab, _shopItemParent);
            decorationItem.Init(item, this, _items.Count);
            _items.Add(decorationItem);
        }

        _itemWidth = _items[0].GetComponent<RectTransform>().sizeDelta.x;

        _isInitialized = true;
    }

    public void Open()
    {
        gameObject.SetActive(true);
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

    public void SetScrollToItem(int itemIndex)
    {
        var targetPosX = _scrollContent.position.x;
        targetPosX += _itemWidth * itemIndex + SCROLL_SPACING * (itemIndex - 1);

        _scrollTween?.Kill();
        _scrollTween = _scrollContent.DOLocalMoveX(-targetPosX, DURATION).SetEase(SCROLL_EASE);
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

    public void SimulateButtonClick(int itemIndex)
    {
        if (itemIndex < 0 || itemIndex >= _items.Count)
        {
            Debug.LogError($"Item index {itemIndex} is out of range.");
            return;
        }
        _items[itemIndex].OnButtonClicked();
    }
}
