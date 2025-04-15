using UnityEngine;
using UnityEngine.EventSystems;

public class PaperArea : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private RectTransform _paperArea;
    [SerializeField] private RectTransform _saplingArea;
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
            // float saplingXForFlower = (eventData.position.x - _paperMiddleX) * (_paperMiddleX - _saplingRightMostPosX) / (_paperLeftMostX - _paperMiddleX) + _saplingRightMostPosX;
            float saplingXForFlower = _paperMiddleX + (eventData.position.x - _paperMiddleX) * (_saplingRightMostPosX - _paperMiddleX) / (_paperLeftMostX - _paperMiddleX);
            targetPos = new Vector3(saplingXForFlower, _bottomMostY, 0);

            float angle = 90 - Mathf.Atan2(eventData.position.y - targetPos.y, eventData.position.x - targetPos.x) * Mathf.Rad2Deg;
            targetRot = new Vector3(0, 0, -angle);
        }

        References.WorkshopPage.OnPaperAreaClicked(targetPos, targetRot);
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
        if (References.WorkshopPage.CheckIfInMachineArea(transform.position))
        {
            References.WorkshopPage.OnFlowerGivenToMachine();
        }
        else
        {
            transform.position = _startPosition;
        }
    }
}
