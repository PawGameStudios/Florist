using System;
using System.Collections.Generic;
using Config;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PaperArea : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum State
    {
        None,
        WaitingForPaper,
        AddingFlowers,
        ScissorInUse,
        ScissorUsed,
        InMachine,
        MachineDone,
        InRibbonArea,
        Done
    }

    public State PaperState => _state;
    public GameObject BouquetObject => _bouquetObject;
    public BouquetModel BouquetModel => _bouquetModel;
    public bool CanAddFlowers => _state == State.AddingFlowers;
    public bool IsEmpty => _state == State.WaitingForPaper;
    public bool CanUseScissor => _state == State.AddingFlowers;
    public Transform RibbonPosRef => _ribbonPosRef;
    [SerializeField] private RectTransform _paperArea;
    [SerializeField] private Image _paperImage;
    [SerializeField] private Image _paperRollImage;
    [SerializeField] private Image _paperClosedImage;
    [SerializeField] private Image _guideImage;
    [SerializeField] private RectTransform _saplingArea;
    [SerializeField] private Transform _scissorPosRef;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _bouquetObject;
    [SerializeField] private Material _maskMaterial;
    [SerializeField] private Flower _flowerForLoad;
    [SerializeField] private Transform _rollImageInitRef;
    [SerializeField] private Transform _paperImageInitRef;
    [SerializeField] private Transform _rollImageFinalRef;
    [SerializeField] private Transform _paperImageFinalRef;
    [SerializeField] private Transform _ribbonPosRef;
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
    private Sequence _sequence, _paperOpenSequence;
    private Tween _moveTween;
    private BouquetModel _bouquetModel = new();
    private State _state = State.None;
    private Vector3 _lastEventDataPosition;
    private GameObject _scissorObject, _ribbonObject;
    private readonly List<Flower> _allFlowers = new();
    private readonly List<Flower> _unCutFlowers = new();
    private const float PAPER_OPEN_DURATION = 1f;
    private const float ANGLE_LIMIT = 40f, Y_POS_LIMIT = 350;

    void OnDisable()
    {
        _moveTween?.Kill();
        _sequence?.Kill();
        _paperOpenSequence?.Kill();
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

    public void SetAvailable(bool isAvailable)
    {
        if (isAvailable)
            _state = State.WaitingForPaper;
        else
            _state = State.None;
    }

    public void Reset()
    {
        _state = State.None;
        _bouquetModel.Clear();
        _unCutFlowers.Clear();
        _paperRollImage.gameObject.SetActive(false);
        _paperClosedImage.gameObject.SetActive(false);
        _paperImage.gameObject.SetActive(false);
        _guideImage.color = new Color(1, 1, 1, 0);

        _paperImage.transform.position = _paperImageInitRef.position;
        _paperRollImage.transform.position = _rollImageInitRef.position;
        _paperRollImage.transform.localScale = new Vector3(1, 1, 1);

        for (int i = 0; i < _allFlowers.Count; i++)
        {
            Destroy(_allFlowers[i].gameObject);
        }
        Destroy(_scissorObject);
        Destroy(_ribbonObject);

        _allFlowers.Clear();
    }

    public void GetPaperToArea(Sprite paperSprite, Sprite rollSprite, Sprite closedSprite, WrappingPaperType paperType, Action onCompleted)
    {
        _paperImage.sprite = paperSprite;
        _paperRollImage.sprite = rollSprite;
        _paperClosedImage.sprite = closedSprite;
        _bouquetModel = new BouquetModel
        {
            Flowers = new List<BouquetFlowerInfo>(),
            WrappingPaperType = paperType
        };

        _paperRollImage.gameObject.SetActive(true);
        _paperImage.gameObject.SetActive(true);
        _paperClosedImage.gameObject.SetActive(false);

        _paperImage.transform.position = _paperImageInitRef.position;
        _paperRollImage.transform.position = _rollImageInitRef.position;

        _guideImage.color = new Color(1, 1, 1, 0);

        _paperOpenSequence?.Kill();
        _paperOpenSequence = DOTween.Sequence();
        _paperOpenSequence.Append(_paperRollImage.transform.DOMove(_rollImageFinalRef.position, PAPER_OPEN_DURATION).SetEase(Ease.OutSine));
        _paperOpenSequence.Join(_paperRollImage.transform.DOScaleY(0, PAPER_OPEN_DURATION).SetEase(Ease.OutSine));
        _paperOpenSequence.Join(_paperImage.transform.DOMove(_paperImageFinalRef.position, PAPER_OPEN_DURATION).SetEase(Ease.OutSine));
        _paperOpenSequence.Append(_guideImage.DOColor(new Color(1, 1, 1, 1), .1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            _state = State.AddingFlowers;
            onCompleted?.Invoke();
        }));
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isDragging)
            return;

        if (_state != State.AddingFlowers && _state != State.ScissorUsed)
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
        if (eventData.position.y < Y_POS_LIMIT)
        {
            var pos = eventData.position;
            pos.y = Y_POS_LIMIT;
            eventData.position = pos;
        }

        Flower newFlower = References.WorkshopPage.CreateNewFlower(targetPos: eventData.position,
                                                        targetRotation: targetRot,
                                                        parent: _bouquetObject.transform);
        if (newFlower != null)
        {
            _state = State.AddingFlowers;
            _bouquetModel.Flowers.Add(new BouquetFlowerInfo
            {
                FlowerType = newFlower.FlowerType,
                FlowerColor = newFlower.FlowerColor,
                Count = 1,
                Position = newFlower.transform.localPosition,
                Rotation = newFlower.transform.localEulerAngles,
                Pivot = newFlower.GetComponent<RectTransform>().pivot,
                IsFlowerCut = false
            });
            _unCutFlowers.Add(newFlower);
            _allFlowers.Add(newFlower);
        }
    }

    public void MoveTo(Vector3 position, float duration = .5f)
    {
        _moveTween?.Kill();
        _moveTween = transform.DOMove(position, duration).SetEase(Ease.OutCubic).OnComplete(() =>
        {
            _moveTween = null;
        });
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
            _state = State.WaitingForPaper;
            _guideImage.color = new Color(1, 1, 1, 0);
            _paperRollImage.gameObject.SetActive(false);
            _paperClosedImage.gameObject.SetActive(false);
            _bouquetModel.Clear();

            transform.localPosition = _startPosition;

            for (int i = 2; i < _bouquetObject.transform.childCount; i++)
            {
                Destroy(_bouquetObject.transform.GetChild(i).gameObject);
            }
            _unCutFlowers.Clear();
            _allFlowers.Clear();
            References.WorkshopPage.OnFlowerGivenToTrash(this);
            return;
        }

        if (_state == State.AddingFlowers)
        {
            transform.localPosition = _startPosition;
        }
        else if (_state == State.ScissorUsed && _unCutFlowers.Count == 0)
        {
            if (References.WorkshopPage.CheckIfInMachineArea(transform.position))
            {
                _state = State.InMachine;
                _guideImage.color = new Color(1, 1, 1, 0);
                _paperRollImage.gameObject.SetActive(false);
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

    public void OnMachineDone()
    {
        if (_state != State.InMachine)
            return;

        Debug.Log($"OnMachineDone");
        _state = State.MachineDone;

        _paperImage.gameObject.SetActive(false);
        _paperClosedImage.gameObject.SetActive(true);
    }

    public void OnRibbonSelected(RibbonType ribbonType, GameObject ribbonObject)
    {
        if (_state != State.InRibbonArea)
            return;

        Debug.Log($"Selected ribbon type: {ribbonType}");
        _bouquetModel.RibbonType = ribbonType;
        _state = State.Done;

        _ribbonObject = ribbonObject;

        References.WorkshopPage.OnFlowerReady(_bouquetModel, this.gameObject);
    }

    public void OnScissorClicked(Transform scissor)
    {
        HapticsController.PlayButtonHaptic();
        _state = State.ScissorInUse;

        _scissorObject = scissor.gameObject;
        _scissorObject.SetActive(true);
        scissor.SetParent(_paperArea);

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

        float time = 0;
        Vector3 p0 = scissor.position;
        Vector3 p1 = scissor.position + new Vector3(Random.Range(0, 300), Random.Range(-300, 300), 0);
        Vector3 p2 = new(_saplingLeftMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 destination = new(_saplingLeftMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);
        Vector3 finalDestination = new(_saplingRightMostPosX, _scissorPosRef.position.y, _scissorPosRef.position.z);

        var initTween = DOTween.To(() => time, t => time = t, 1, 1).OnUpdate(() =>
        {
            scissor.position = GetPointOnBezier(time, p0, p1, p2, destination);
        });

        _sequence?.Kill();
        _sequence = DOTween.Sequence();
        _sequence.Append(initTween.SetEase(Ease.InSine));
        _sequence.Join(scissor.DOLocalRotate(new Vector3(0, 0, -180), .5f).SetEase(Ease.InSine));
        _sequence.Append(scissor.DOMove(finalDestination, .1f).SetEase(Ease.OutSine).OnComplete(() =>
        {
            // TODO: sound
            HapticsController.PlayMediumHaptic();

            foreach (Flower flower in _unCutFlowers)
            {
                flower.FlowerImage.material = _maskMaterial;
            }
            foreach (BouquetFlowerInfo flowerInfo in _bouquetModel.Flowers)
            {
                flowerInfo.IsFlowerCut = true;
            }
            _unCutFlowers.Clear();

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


    #region Save/Load
    public void SetPaper(WrappingPaperType paperType)
    {
        _paperRollImage.gameObject.SetActive(false);
        _paperImage.gameObject.SetActive(true);
        _paperClosedImage.gameObject.SetActive(false);

        _bouquetModel.WrappingPaperType = paperType;

        _paperImage.sprite = Configs.WorkshopConfig.GetWrappingPaperSprite(paperType);
        _paperImage.transform.position = _paperImageFinalRef.position;
    }

    public void SetClosedPaper(WrappingPaperType paperType)
    {
        _paperRollImage.gameObject.SetActive(false);
        _paperImage.gameObject.SetActive(false);
        _paperClosedImage.gameObject.SetActive(true);

        _bouquetModel.WrappingPaperType = paperType;

        _paperClosedImage.sprite = Configs.WorkshopConfig.GetWrappingPaperClosedSprite(paperType);
    }

    public void SetFlowers(BouquetModel bouquetModel)
    {
        _bouquetModel = bouquetModel;

        foreach (BouquetFlowerInfo flowerInfo in bouquetModel.Flowers)
        {
            Debug.Log($"Loading flower: {flowerInfo.FlowerType}, Color: {flowerInfo.FlowerColor}, Position: {flowerInfo.Position}, Rotation: {flowerInfo.Rotation}, Pivot: {flowerInfo.Pivot}");
            Flower flowerObject = Instantiate(_flowerForLoad, _bouquetObject.transform);
            flowerObject.GetComponent<RectTransform>().pivot = flowerInfo.Pivot;
            flowerObject.transform.SetLocalPositionAndRotation(flowerInfo.Position, Quaternion.Euler(flowerInfo.Rotation));
            flowerObject.FlowerImage.sprite = Configs.WorkshopConfig.GetFlowerSprite(flowerInfo.FlowerType, flowerInfo.FlowerColor);
            flowerObject.name = $"{flowerInfo.FlowerType}_{flowerInfo.FlowerColor}";
            flowerObject.gameObject.SetActive(true);

            if (flowerInfo.IsFlowerCut)
            {
                flowerObject.FlowerImage.material = _maskMaterial;
            }
            else
            {
                _unCutFlowers.Add(flowerObject);
            }
        }
    }

    public void SetState(State state)
    {
        _state = state;
    }

    public void SetRibbon(RibbonType ribbonType)
    {
        Debug.Log("Setting ribbon type: " + ribbonType);

        _bouquetModel.RibbonType = ribbonType;
        var ribbonAnimator = Instantiate(Configs.WorkshopConfig.GetRibbonAnimator(ribbonType), transform);
        _ribbonObject = ribbonAnimator.gameObject;
        Invoke(nameof(PlayRibbonAnimation), 0.1f);
    }

    private void PlayRibbonAnimation()
    {
        _ribbonObject.transform.SetPositionAndRotation(_ribbonPosRef.position, _ribbonPosRef.rotation);
        _ribbonObject.GetComponent<Animator>().Play("Idle");
    }
    #endregion
}
