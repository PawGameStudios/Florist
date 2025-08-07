using MelenitasDev.SoundsGood;
using UnityEngine;

public class PrivacyController : MonoBehaviour
{
    [SerializeField] private Popup _popUp;

    public void ShowPrivacyScreen()
    {
        _popUp.Open();
        FirebaseController.Instance.SendCustomEvent($"privacy_displayed");
    }

    public void OnCloseClicked()
    {
        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        _popUp.Close();
    }

    public void OnAcceptClicked()
    {
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted = true;
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyShown = true;

        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        _popUp.Close();
        FirebaseController.Instance.SendCustomEvent($"privacy_accepted");
    }

    public void OnDeclineClicked()
    {
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted = false;
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyShown = true;

        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        FirebaseController.Instance.SendCustomEvent($"privacy_declined");

        OnCloseClicked();
    }

    public void OnPrivacyLinkClicked()
    {
        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();
        Application.OpenURL("???");
    }
}
