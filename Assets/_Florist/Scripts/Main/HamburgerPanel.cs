using UnityEngine;

public class HamburgerPanel : MonoBehaviour
{
    [SerializeField] private SettingsPanel _settings;
    [SerializeField] private Animator _animator;

    public void OnHamburgerClicked()
    {
        _animator.Play("Open");
    }

    public void OnHamburgerExitClicked()
    {
        _animator.Play("Close");
    }

    public void OnSettingsClicked()
    {
        _settings.Open();
    }

    public void OnAchievementsClicked()
    {
        // TODO: Open achievements page
    }

}
