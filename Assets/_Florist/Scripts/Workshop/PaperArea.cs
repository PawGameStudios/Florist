using System.Collections.Generic;
using Config;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class PaperArea : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private enum State
    {
        None,
        AddingFlowers,
        ScissorInUse,
        ScissorUsed,
        InMachine,
        MachineDone,
        InRibbonArea,
        Done
    }

    public bool CanUseScissor => _state == State.AddingFlowers;
    [SerializeField] private RectTransform _paperArea;
    [SerializeField] private RectTransform _saplingArea;
    [SerializeField] private Transform _scissorPosRef;
    [SerializeField] private GameObject _scissorMaskObject;
    [SerializeField] private Canvas _canvas;
    private float _bottomMostY;
    private float _saplingRightMostPosX;
    private float _saplingLeftMostPosX;
    private float _paperRightMostX;
    private float _paperLeftMostX;
    private float _paperMiddleX;
    private bool _isPosCalculated = false;
    private bool _isDragging = false;
    private Vector2 _offset;
    private Vector3 _startPosition;
    private Sequence _sequence;
    private readonly List<GameObject> _flowersForBouquet = new();
    private BouquetModel _bouquetModel = new();
    private State _state = State.None;
    private const float ANGLE_LIMIT = 40f;
    private Vector3 _lastEventDataPosition;
    private GameObject _scissorObject;

    void OnDisable()
    {
        _sequence?.Kill();
        _isPosCalculated = false;
        _isDragging = false;
    }

    void Update()
    {
        if (_isDragging)
        {
            References.WorkshopPage.CheckIfPaperAreaInScreenEdge(_lastEventDataPosition, _paperArea.rect.size.x, this);
        }
    }

    public void GetPaperToArea(GameObject paper, WrappingPaperType paperType)
    {
        // TODO: Implement paper animation and position setting
        // paper.SetParent(_paperArea);
        // paper.localPosition = Vector3.zero;
        // paper.localEulerAngles = Vector3.zero;

        _bouquetModel = new BouquetModel
        {
            Flowers = new List<BouquetFlowerInfo>(),
            WrappingPaperType = paperType
        };

        _state = State.AddingFlowers;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging)
            return;

        if (_state != State.AddingFlowers)
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
        if (targetRotationZ < -ANGLE_LIMIT || targetRotationZ > ANGLE_LIMIT)
        {
            return;
        }

        var newFlower = References.WorkshopPage.CreateNewFlower(eventData.position, targetRot, _paperArea);
        if (newFlower != null)
        {
            _flowersForBouquet.Add(newFlower.gameObject);
            _bouquetModel.Flowers.Add(new BouquetFlowerInfo
            {
                FlowerType = newFlower.FlowerType,
                FlowerColor = newFlower.FlowerColor,
            });
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_state == State.ScissorInUse || _state == State.InMachine || _state == State.InRibbonArea || _state == State.Done)
            return;

        _lastEventDataPosition = eventData.position;
        _isDragging = true;
        _startPosition = transform.localPosition;
        _offset = new Vector2(transform.position.x, transform.position.y) - eventData.position;
        transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_state == State.ScissorInUse || _state == State.InMachine || _state == State.InRibbonArea || _state == State.Done)
            return;

        _canvas.sortingOrder = 10;
        _lastEventDataPosition = eventData.position;
        transform.position = eventData.position + _offset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_state == State.ScissorInUse || _state == State.InMachine || _state == State.InRibbonArea || _state == State.Done)
            return;

        _canvas.sortingOrder = 5;
        _isDragging = false;

        if (References.WorkshopPage.CheckIfInTrashArea(transform.position))
        {
            References.WorkshopPage.OnFlowerGivenToTrash(this);
            return;
        }

        if (_state == State.AddingFlowers)
        {
            transform.localPosition = _startPosition;
        }
        else if (_state == State.ScissorUsed)
        {
            if (References.WorkshopPage.CheckIfInMachineArea(transform.position))
            {
                _state = State.InMachine;
                References.WorkshopPage.OnFlowerGivenToMachine(this);
            }
            else
            {
                transform.localPosition = _startPosition;
            }
        }
        else if (_state == State.MachineDone)
        {
            if (References.WorkshopPage.CheckIfInRibbonArea(transform.position))
            {
                _state = State.InRibbonArea;
                References.WorkshopPage.OnFlowerGivenToRibbon(this);
            }
            else
            {
                transform.localPosition = _startPosition;
            }
        }
    }

    public void OnRibbonSelected(RibbonType ribbonType)
    {
        if (_state != State.InRibbonArea)
            return;

        Debug.Log($"Selected ribbon type: {ribbonType}");
        _bouquetModel.RibbonType = ribbonType;
        _state = State.Done;
    }

    public void OnMachineDone()
    {
        if (_state != State.InMachine)
            return;

        Debug.Log($"OnMachineDone");
        _state = State.MachineDone;
        // TODO: show actual bouquet here
    }

    public void OnScissorClicked(Transform scissor)
    {
        _state = State.ScissorInUse;

        _scissorObject = scissor.gameObject;
        _scissorObject.SetActive(true);
        scissor.SetParent(_paperArea);

        float time = 0;
        Vector3 p0 = scissor.position;
        Vector3 p1 = scissor.position + new Vector3(Random.Range(0, 300), Random.Range(-300, 300), 0);
        Vector3 p2 = new(_saplingRightMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 destination = new(_saplingRightMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 finalDestination = new(_saplingLeftMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);

        var initTween = DOTween.To(() => time, t => time = t, 1, 1).OnUpdate(() =>
        {
            scissor.position = GetPointOnBezier(time, p0, p1, p2, destination);
        });

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(initTween.SetEase(Ease.InSine));
        _sequence.Append(scissor.DOMove(finalDestination, .1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _scissorMaskObject.SetActive(true);
            _scissorObject.SetActive(false);
            _state = State.ScissorUsed;
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
