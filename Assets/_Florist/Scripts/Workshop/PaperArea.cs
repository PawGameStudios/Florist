using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaperArea : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _paperArea;
    [SerializeField] private RectTransform _saplingArea;
    [SerializeField] private Transform _scissorPosRef;
    [SerializeField] private Transform _scissor;
    [SerializeField] private GameObject _scissorMaskObject;
    private float _bottomMostY;
    private float _saplingRightMostPosX;
    private float _saplingLeftMostPosX;
    private float _paperRightMostX;
    private float _paperLeftMostX;
    private float _paperMiddleX;
    private bool _isPosCalculated = false;
    private bool _isDragging = false;
    private bool _isScissorUsed = false;
    private Vector2 _offset;
    private Vector3 _startPosition;
    private Sequence _sequence;
    private List<RectTransform> _papersInUse = new();
    private List<GameObject> _flowersForBouquet = new();

    void OnDisable()
    {
        _sequence?.Kill();
        _isScissorUsed = false;
        _isPosCalculated = false;
        _isDragging = false;
    }

    public void GetPaperToArea(Transform paper)
    {
        paper.SetParent(_paperArea);
        paper.localPosition = Vector3.zero;
        paper.localEulerAngles = Vector3.zero;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging)
            return;

        if (!_isPosCalculated)
        {
            _bottomMostY = _paperArea.position.y - _paperArea.rect.height / 2f;
            _saplingRightMostPosX = _saplingArea.position.x + _saplingArea.rect.width / 2f;
            _saplingLeftMostPosX = _saplingArea.position.x - _saplingArea.rect.width / 2f;
            _paperRightMostX = _paperArea.position.x + _paperArea.rect.width / 2f;
            _paperLeftMostX = _paperArea.position.x - _paperArea.rect.width / 2f;
            _paperMiddleX = _paperArea.position.x;
            _isPosCalculated = true;
        }

        Vector3 targetPos;
        Vector3 targetRot;

        if (eventData.position.x >= _paperMiddleX)
        {
            float saplingXForFlower = _paperMiddleX + (eventData.position.x - _paperMiddleX) * (_saplingLeftMostPosX - _paperMiddleX) / (_paperRightMostX - _paperMiddleX);
            targetPos = new Vector3(saplingXForFlower, _bottomMostY, 0);

            float angle = 90 - Mathf.Atan2(eventData.position.y - targetPos.y, eventData.position.x - targetPos.x) * Mathf.Rad2Deg;
            targetRot = new Vector3(0, 0, -angle);
        }
        else
        {
            float saplingXForFlower = _paperMiddleX + (eventData.position.x - _paperMiddleX) * (_saplingRightMostPosX - _paperMiddleX) / (_paperLeftMostX - _paperMiddleX);
            targetPos = new Vector3(saplingXForFlower, _bottomMostY, 0);

            float angle = 90 - Mathf.Atan2(eventData.position.y - targetPos.y, eventData.position.x - targetPos.x) * Mathf.Rad2Deg;
            targetRot = new Vector3(0, 0, -angle);
        }

        float targetRotationZ = targetRot.z;
        if (targetRotationZ < -60 || targetRotationZ > 60)
        {
            return;
        }

        var newFlower = References.WorkshopPage.CreateNewFlower(eventData.position, targetRot);
        if (newFlower != null)
        {
            _flowersForBouquet.Add(newFlower);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
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
        _isDragging = false;
        if (References.WorkshopPage.CheckIfInMachineArea(transform.position) && _isScissorUsed)
        {
            References.WorkshopPage.OnFlowerGivenToMachine();
        }
        else if (References.WorkshopPage.CheckIfInTrashArea(transform.position))
        {
            References.WorkshopPage.OnFlowerGivenToTrash(this);
        }
        else
        {
            transform.position = _startPosition;
        }
    }

    public void OnScissorClicked()
    {
        _scissor.gameObject.SetActive(true);

        float time = 0;
        Vector3 p0 = _scissor.position;
        Vector3 p1 = _scissor.position + new Vector3(Random.Range(0, 300), Random.Range(-300, 300), 0);
        Vector3 p2 = new(_saplingRightMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 destination = new(_saplingRightMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 finalDestination = new(_saplingLeftMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);

        var initTween = DOTween.To(() => time, t => time = t, 1, 1).OnUpdate(() =>
        {
            _scissor.position = GetPointOnBezier(time, p0, p1, p2, destination);
        });

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(initTween.SetEase(Ease.InSine));
        _sequence.Append(_scissor.DOMove(finalDestination, .1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _scissorMaskObject.SetActive(true);
            _isScissorUsed = true;
        }));
    }

    private Vector3 GetPointOnBezier(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 destination)
    {
        Vector3 p01 = Vector3.Lerp(p0, p1, t);
        Vector3 p12 = Vector3.Lerp(p1, p2, t);
        Vector3 p23 = Vector3.Lerp(p2, destination, t);
        Vector3 p012 = Vector3.Lerp(p01, p12, t);
        Vector3 p123 = Vector3.Lerp(p12, p23, t);
        Vector3 pointOnCurve = Vector3.Lerp(p012, p123, t);
        return pointOnCurve;
    }
}
