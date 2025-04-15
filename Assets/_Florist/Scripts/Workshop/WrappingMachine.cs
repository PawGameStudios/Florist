using UnityEngine;
using UnityEngine.UI;

public class WrappingMachine : MonoBehaviour
{
    public Rect Rect => _rectTransform.rect;
    [SerializeField] private RectTransform _rectTransform;
    [SerializeField] private Transform _rightDoor;
    [SerializeField] private Transform _leftDoor;
    [SerializeField] private GameObject _greenLightObject;

    public void OpenMachine()
    {
        // Logic to open the wrapping machine
        Debug.Log("Wrapping machine opened.");
    }
}
