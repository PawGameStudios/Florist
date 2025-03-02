using System;
using AYellowpaper.SerializedCollections;
using UnityEngine;
using UnityEngine.EventSystems;
using FlowerType = LevelConfig.FlowerType;

[Serializable]
public class BouquetModel
{
    public SerializedDictionary<FlowerType, int> Flowers;

    public BouquetModel()
    {
        Flowers = new SerializedDictionary<FlowerType, int>();
    }
}

public class Bouquet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public BouquetModel Model => _model;
    [SerializeField] private DukkanPage _dukkan;
    private Vector2 _offset;
    private Vector3 _startPosition;
    private BouquetModel _model;

    public void SetModel(BouquetModel model)
    {
        _model = model;
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
