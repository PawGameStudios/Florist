using UnityEngine;
using UnityEngine.UI;
using Ads;

public class AdRewardController : MonoBehaviour
{
    [SerializeField] private Button[] _adButtons;

    private void OnEnable()
    {
        AdManager.OnRewardedLoaded += OnRewardedLoadedHandler;
        AdManager.OnRewardedWathed += OnRewardedWathedHandler;
    }

    private void OnDisable()
    {
        AdManager.OnRewardedLoaded -= OnRewardedLoadedHandler;
        AdManager.OnRewardedWathed -= OnRewardedWathedHandler;
    }

    private void OnRewardedLoadedHandler()
    {
        foreach (var button in _adButtons)
        {
            button.interactable = true;
        }
    }

    private void OnRewardedWathedHandler()
    {
        foreach (var button in _adButtons)
        {
            button.interactable = false;
        }
    }
}
