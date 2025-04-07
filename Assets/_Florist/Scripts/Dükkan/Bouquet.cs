using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Config;
using Sirenix.OdinInspector;
using System.Collections.Generic;


[Serializable]
public class BouquetFlowerInfo
{
    public FlowerType FlowerType;
    public int Count;
    public FlowerColor FlowerColor;
}

[Serializable]
public class BouquetModel
{
    public List<BouquetFlowerInfo> Flowers;
    [ReadOnly] public RibbonType RibbonType;
    [ReadOnly] public WrappingPaperType WrappingPaperType;
    [ReadOnly] public BouquetType BouquetType;

    public BouquetModel()
    {
        Flowers = new();
    }

    public void AddNewFlowers(BouquetFlowerInfo flowerInfo)
    {
        if (flowerInfo.Count <= 0) return;

        foreach (var flower in Flowers)
        {
            if (flower.FlowerType == flowerInfo.FlowerType && flower.FlowerColor == flowerInfo.FlowerColor)
            {
                flower.Count += flowerInfo.Count;
                return;
            }
        }
        Flowers.Add(flowerInfo);
    }

    public BouquetFlowerInfo GetFlowersWithType(FlowerType flowerType, FlowerColor flowerColor)
    {
        foreach (var flower in Flowers)
        {
            if (flower.FlowerType == flowerType && flower.FlowerColor == flowerColor)
            {
                return flower;
            }
        }
        return null;
    }
}

public class Bouquet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public OrderInfo Order => _order;
    [SerializeField] private DukkanPage _dukkan;
    private Vector2 _offset;
    private Vector3 _startPosition;
    private OrderInfo _order;

    public void SetOrder(OrderInfo order)
    {
        _order = order;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _startPosition = transform.position;
        _offset = new Vector2(transform.position.x, transform.position.y) - eventData.position;
        transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position + _offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dukkan.CheckIfInCustomerArea(transform.position))
        {
            _dukkan.OnFlowerDelivered();
            transform.position = _startPosition;
        }
        else
        {
            transform.position = _startPosition;
        }
    }
}
