using MelenitasDev.SoundsGood;
using UnityEngine;

public class PrivacyController : MonoBehaviour
{
    [SerializeField] private Popup _popUp;

    public void ShowPrivacyScreen()
    {
        if (SaveSystem.Inst.GeneralData.IsPrivacyPolicyShown)
        {
            return;
        }

        _popUp.SetPositiveButtonListener(OnAcceptClicked)
            .SetNegaiveButtonListener(OnDeclineClicked)
            .Open();
        FirebaseController.Instance.SendCustomEvent($"privacy_displayed");
    }

    public void OnCloseClicked()
    {
        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        _popUp.Close();
    }

    private void OnAcceptClicked()
    {
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted = true;
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyShown = true;

        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        _popUp.Close();
        FirebaseController.Instance.SendCustomEvent($"privacy_accepted");
    }

    private void OnDeclineClicked()
    {
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted = false;
        SaveSystem.Inst.GeneralData.IsPrivacyPolicyShown = true;

        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();

        FirebaseController.Instance.SendCustomEvent($"privacy_declined");

        OnCloseClicked();
    }

    private void OnPrivacyLinkClicked()
    {
        // SoundController.PlaySound(SFX.ButtonClick);
        HapticsController.PlayButtonHaptic();
        Application.OpenURL("https://paw-games.com/privacy-policy");
    }
}
