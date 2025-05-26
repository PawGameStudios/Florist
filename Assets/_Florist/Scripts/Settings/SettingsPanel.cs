using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Popup _popup;
    [SerializeField] private SettingsItem _soundItem;

    public void Open()
    {
        _popup.Open();
    }

    public void Close()
    {
        _popup.Close();
    }

    public void OnMusicClicked()
    {
    }

    public void OnSoundClicked()
    {
    }

    public void OnVibrationClicked()
    {
    }

}
