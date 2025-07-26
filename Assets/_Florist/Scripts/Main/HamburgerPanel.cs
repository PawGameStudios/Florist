using UnityEngine;

public class HamburgerPanel : MonoBehaviour
{
    [SerializeField] private SettingsPanel _settings;
    [SerializeField] private Animator _animator;

    public void OnHamburgerClicked()
    {
        HapticsController.PlayButtonHaptic();
        _animator.Play("Open");
    }

    public void OnHamburgerExitClicked()
    {
        HapticsController.PlayButtonHaptic();
        _animator.Play("Close");
    }

    public void OnSettingsClicked()
    {
        HapticsController.PlayButtonHaptic();
        _settings.Open();
    }

    public void OnAchievementsClicked()
    {
        HapticsController.PlayButtonHaptic();
        // TODO: Open achievements page
    }

    public void OnDecorationClicked()
    {
        HapticsController.PlayButtonHaptic();
        References.DecorationManager.OpenDecorationPage();
    }

}
