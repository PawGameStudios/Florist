using System;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Florist.Merge
{
    public sealed class MergeEnergyAds : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private MergeEconomyConfig economyConfig;
        [SerializeField] private Button watchButton;
        [SerializeField] private TextMeshProUGUI buttonText;
        private bool pending;
        private string feedback;
        private const string Prefix = "Florist.Merge.EnergyAd.";

        private void OnEnable()
        {
            watchButton.onClick.AddListener(Watch);
            gameManager.OnEnergyChanged += Refresh;
            Ads.AdManager.OnRewardedLoaded += Refresh;
            Refresh();
        }
        private void OnDisable()
        {
            watchButton.onClick.RemoveListener(Watch);
            gameManager.OnEnergyChanged -= Refresh;
            Ads.AdManager.OnRewardedLoaded -= Refresh;
            CancelInvoke();
        }
        private int UsedToday => PlayerPrefs.GetString(Prefix + "Day", "") == DateTime.UtcNow.ToString("yyyy-MM-dd")
            ? PlayerPrefs.GetInt(Prefix + "Count", 0) : 0;
        private double Cooldown
        {
            get
            {
                if (!DateTime.TryParse(PlayerPrefs.GetString(Prefix + "Last", ""), CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind, out var last)) return 0;
                return Math.Max(0, (last.ToUniversalTime().AddSeconds(economyConfig.EnergyAdCooldownSeconds) - DateTime.UtcNow).TotalSeconds);
            }
        }
        private void Refresh()
        {
            bool capped = gameManager.Energy >= gameManager.MaxEnergy;
            bool limited = UsedToday >= economyConfig.EnergyAdsPerUtcDay;
            bool waiting = Cooldown > 0;
            bool loaded = Ads.AdManager.Instance != null && Ads.AdManager.Instance.IsRewardedAdLoaded;
            watchButton.interactable = economyConfig.EnableEnergyAds && !pending && !capped && !limited && !waiting && loaded;
            buttonText.text = feedback ?? (pending ? MergeLocalization.Text("merge_ad_opening") : capped ? MergeLocalization.Text("merge_energy_full") : limited ? MergeLocalization.Text("merge_ad_limit") :
                waiting ? MergeLocalization.Format("merge_ad_cooldown", Math.Ceiling(Cooldown)) : !loaded ? MergeLocalization.Text("merge_ad_loading") :
                MergeLocalization.Format("merge_watch_ad", Math.Min(economyConfig.RewardedEnergy, gameManager.MaxEnergy - gameManager.Energy), UsedToday, economyConfig.EnergyAdsPerUtcDay));
        }
        private void Watch()
        {
            if (pending) return;
            Refresh();
            if (!watchButton.interactable) return;
            pending = true;
            feedback = null;
            Refresh();
            Ads.AdManager.Instance.ShowRewardedAd(watched =>
            {
                if (this == null || !pending) return;
                pending = false;
                if (watched)
                {
                    int used = UsedToday;
                    PlayerPrefs.SetString(Prefix + "Day", DateTime.UtcNow.ToString("yyyy-MM-dd"));
                    PlayerPrefs.SetInt(Prefix + "Count", used + 1);
                    PlayerPrefs.SetString(Prefix + "Last", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));
                    gameManager.GrantRewardedEnergy(economyConfig.RewardedEnergy);
                }
                else
                {
                    feedback = MergeLocalization.Text("merge_ad_no_reward");
                    Invoke(nameof(ClearFeedback), 3);
                }
                Refresh();
            });
        }
        private void ClearFeedback() { feedback = null; Refresh(); }
    }
}
