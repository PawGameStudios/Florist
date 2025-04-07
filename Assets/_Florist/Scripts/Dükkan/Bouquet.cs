using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using Config;
using Sirenix.OdinInspector;

[Serializable]
public class BouquetModel
{
    public SerializedDictionary<FlowerType, int> Flowers;
    [ReadOnly] public RibbonType RibbonType;
    [ReadOnly] public WrappingPaperType WrappingPaperType;
    [ReadOnly] public float BeautyPercentage;
    [ReadOnly] public BouquetType BouquetType;

    public BouquetModel()
    {
        Flowers = new SerializedDictionary<FlowerType, int>();
        BeautyPercentage = 1;
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
