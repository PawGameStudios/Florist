using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
    [SerializeField] private Popup _popup;
    [SerializeField] private SettingsItem _soundItem;
    [SerializeField] private SettingsItem _musicItem;
    [SerializeField] private SettingsItem _vibrationItem;
    [SerializeField] private SettingsItem _notifItem;

    public void Open()
    {
        _soundItem.SetIsOn(SaveSystem.Inst.GeneralData.IsSoundOn);
        _musicItem.SetIsOn(SaveSystem.Inst.GeneralData.IsMusicOn);
        _vibrationItem.SetIsOn(SaveSystem.Inst.GeneralData.IsVibrationOn);
        _notifItem.SetIsOn(SaveSystem.Inst.GeneralData.IsNotificationsOn);
        _popup.Open();
    }

    public void Close()
    {
        _popup.Close();
    }

    public void OnMusicClicked()
    {
        HapticsController.PlayButtonHaptic();
        SaveSystem.Inst.GeneralData.IsMusicOn = !SaveSystem.Inst.GeneralData.IsMusicOn;
        _musicItem.Toggle();
    }

    public void OnSoundClicked()
    {
        HapticsController.PlayButtonHaptic();
        SaveSystem.Inst.GeneralData.IsSoundOn = !SaveSystem.Inst.GeneralData.IsSoundOn;
        _soundItem.Toggle();
    }

    public void OnVibrationClicked()
    {
        SaveSystem.Inst.GeneralData.IsVibrationOn = !SaveSystem.Inst.GeneralData.IsVibrationOn;
        _vibrationItem.Toggle();

        if (SaveSystem.Inst.GeneralData.IsVibrationOn)
        {
            HapticsController.PlayButtonHaptic();
        }
    }

    public void OnNotificationsClicked()
    {
        HapticsController.PlayButtonHaptic();
        SaveSystem.Inst.GeneralData.IsNotificationsOn = !SaveSystem.Inst.GeneralData.IsNotificationsOn;
        _notifItem.Toggle();
    }

}
