using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Config;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[Serializable]
public struct BouquetSaveInfo
{
    public List<Vector3> Positions;
    public List<Vector3> Rotations;
}

[Serializable]
public class BouquetFlowerInfo
{
    public FlowerType FlowerType;
    public int Count;
    public FlowerColor FlowerColor;
    [HideInInspector] public Vector3 Position;
    [HideInInspector] public Vector3 Rotation;
    [HideInInspector] public Vector3 Pivot;
    [HideInInspector] public bool IsFlowerCut;
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

    public void Clear()
    {
        Flowers.Clear();
        RibbonType = RibbonType.None;
        WrappingPaperType = WrappingPaperType.None;
        BouquetType = BouquetType.None;
    }
}

public class Bouquet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public OrderInfo Order => _order;
    private Vector2 _offset;
    private Vector3 _startPosition;
    private OrderInfo _order;

    public void SetOrder(OrderInfo order, GameObject bouquetObject)
    {
        _order = order;
        var newBouquet = Instantiate(bouquetObject, transform);
        newBouquet.transform.localPosition = Vector3.zero;
        newBouquet.transform.localEulerAngles = Vector3.zero;

        newBouquet.TryGetComponent<PaperArea>(out var paperAreaComponent);
        if (paperAreaComponent != null)
        {
            Destroy(paperAreaComponent);
        }
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
        if (References.DukkanPage.CheckIfInCustomerArea(transform.position))
        {
            References.DukkanPage.OnFlowerDelivered();
            transform.position = _startPosition;
        }
        else
        {
            transform.position = _startPosition;
        }
    }
}
