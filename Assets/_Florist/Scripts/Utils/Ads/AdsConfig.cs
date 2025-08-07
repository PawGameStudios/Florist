using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "AdsConfig", menuName = "PlayerDuo/Configs/Ads")]
public class AdsConfig : ScriptableObject
{
    public bool UseTestAds;
    public bool UseInterAds;
    public bool UseRewardedAds;
    public bool UseBannerAds;
    [ShowIf("UseInterAds")] public string InterAdUnitId;
    [ShowIf("UseRewardedAds")] public string RewardedAdUnitId;
    [ShowIf("UseBannerAds")] public string BannerAdUnitId;

    public string GetInterAdUnitId()
    {
        string adUnitId = "unused";

        if (UseTestAds)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/1033173712";
#elif UNITY_IPHONE
                adUnitId = "ca-app-pub-3940256099942544/4411468910";
#else
                adUnitId = "ca-app-pub-3940256099942544/1033173712";
#endif
        }
        else
        {
#if UNITY_EDITOR
            adUnitId = "ca-app-pub-3940256099942544/1033173712";
#else
                adUnitId = InterAdUnitId;
#endif
        }

        return adUnitId;
    }

    public string GetRewardedAdUnitId()
    {
        string adUnitId = "unused";

        if (UseTestAds)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/5224354917";
#elif UNITY_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/1712485313";
#else
            adUnitId = "ca-app-pub-3940256099942544/5224354917";
#endif
        }
        else
        {
#if UNITY_EDITOR
            adUnitId = "ca-app-pub-3940256099942544/5224354917";
#else
            adUnitId = RewardedAdUnitId;
#endif
        }

        return adUnitId;
    }

    public string GetBannerAdUnitId()
    {
        string adUnitId = "unused";

        if (UseTestAds)
        {
#if UNITY_ANDROID
            adUnitId = "ca-app-pub-3940256099942544/6300978111";
#elif UNITY_IPHONE
            adUnitId = "ca-app-pub-3940256099942544/2934735716";
#else
            adUnitId = "ca-app-pub-3940256099942544/6300978111";
#endif
        }
        else
        {
#if UNITY_EDITOR
            adUnitId = "ca-app-pub-3940256099942544/6300978111";
#else
            adUnitId = BannerAdUnitId;
#endif
        }

        return adUnitId;
    }

}