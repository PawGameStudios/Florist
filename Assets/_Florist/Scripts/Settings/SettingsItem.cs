using UnityEngine;

public class SettingsItem : MonoBehaviour
{
    [SerializeField] private GameObject _openObjects;
    [SerializeField] private GameObject _closeObjects;
    private bool _isOn;

    public void SetIsOn(bool isOn)
    {
        _isOn = isOn;
        _openObjects.SetActive(isOn);
        _closeObjects.SetActive(!isOn);
    }

    public void Toggle()
    {
        _isOn = !_isOn;
        _openObjects.SetActive(_isOn);
        _closeObjects.SetActive(!_isOn);
    }
}
