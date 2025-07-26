using UnityEngine;

public interface ITouchable
{
    void OnTouchBegin(Vector3 inputPos);
    void OnTouchEnd(Vector3 inputPos);
    void OnDrag(Vector3 inputPos);
}


public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    private bool _isInput = false;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _isInput = true;
            CheckTouchBegin(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            CheckTouchEnd(Input.mousePosition);
            _isInput = false;
        }
        else if (Input.GetMouseButton(0))
        {
            if (_isInput)
            {
                CheckDrag(Input.mousePosition);
            }
        }
    }

    private void CheckDrag(Vector3 currentInputPos)
    {
        if (!_isInput)
            return;

        Ray ray = _camera.ScreenPointToRay(currentInputPos);
        Debug.DrawRay(ray.origin, ray.direction * 1000, Color.red, 5f);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<ITouchable>(out var touchable))
            {
                touchable.OnDrag(currentInputPos);
            }
        }
    }

    private void CheckTouchEnd(Vector3 currentInputPos)
    {
        if (_isInput)
            return;

        Ray ray = _camera.ScreenPointToRay(currentInputPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<ITouchable>(out var touchable))
            {
                touchable.OnTouchEnd(currentInputPos);
            }
        }
    }

    private void CheckTouchBegin(Vector3 currentInputPos)
    {
        if (!_isInput)
            return;

        Ray ray = _camera.ScreenPointToRay(currentInputPos);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.TryGetComponent<ITouchable>(out var touchable))
            {
                touchable.OnTouchBegin(currentInputPos);
            }
        }
    }
}
