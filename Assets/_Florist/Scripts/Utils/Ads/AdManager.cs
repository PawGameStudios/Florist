using UnityEngine;
using GoogleMobileAds.Api;
using System;
using System.Threading;
using GoogleMobileAds.Ump.Api;

namespace Ads
{
    public class AdManager : MonoSingleton<AdManager>
    {
        public static Action OnRewardedLoaded, OnRewardedWathed;

        public bool IsRewardedAdLoaded => !_rewardShowing && _rewardedAd != null && _rewardedAd.CanShowAd();

        [SerializeField] private GameObject _bannerPanel;
        private InterstitialAd _interstitialAd;
        private int _interReloadCount = 0;
        private RewardedAd _rewardedAd;
        private int _rewardedReloadCount = 0;
        private BannerView _bannerView;
        private int _bannerReloadCount = 0;
        private int _initializaionTryCount = 0;
        private bool _isInitialized = false;
        private bool _isBannerLoaded = false;
        private Action<bool> _onInterClosed, _onRewardedClosed;
        private bool _interCallback = false, _rewardLoadedCallback = false;
        private bool _rewardShowing, _rewardGranted;
        private SynchronizationContext _unityContext;

        protected override void VirtualAwake() => _unityContext = SynchronizationContext.Current;
        private void OnUnityThread(Action action) => _unityContext.Post(_ => action(), null);

        private void OnDisable()
        {
            CancelInvoke();
            _interstitialAd?.Destroy();
            _rewardedAd?.Destroy();
        }

        private void Start()
        {
            if (SaveSystem.Inst.GeneralData.IsUserConsentAsked && SaveSystem.Inst.GeneralData.IsUserConsentForAds)
            {
                InitializeAds();
            }
            else
            {
                // Create a ConsentRequestParameters object.
                ConsentRequestParameters request = new();

                // Check the current consent information status.
                ConsentInformation.Update(request, OnConsentInfoUpdated);
            }
        }

        public void SetMute(bool IsSoundOn)
        {
            MobileAds.SetApplicationMuted(!IsSoundOn);
        }

        private void InitializeAds()
        {
            Debug.Log($"#ads# InitializeAds");
            Debug.Log($"#ads# {ConsentInformation.ConsentStatus}");
            _initializaionTryCount++;
            if (_initializaionTryCount > 3)
            {
                return;
            }

            if (!SaveSystem.Inst.GeneralData.IsUserConsentForAds)
            {
                return;
            }

            if (_bannerPanel != null)
            {
                _bannerPanel.SetActive(false);
            }

            if (SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted)
            {
                RequestConfiguration requestConfiguration = new()
                {
                    PublisherPrivacyPersonalizationState = PublisherPrivacyPersonalizationState.Enabled
                };
                MobileAds.SetRequestConfiguration(requestConfiguration);
            }

            // Initialize the Google Mobile Ads SDK.
            MobileAds.Initialize((initStatus) =>
            {
                _initializaionTryCount = 0;
                _isInitialized = true;

                if (_rewardedAd != null && _rewardedAd.CanShowAd())
                    _rewardLoadedCallback = true;
                else if (Configs.AdsConfig.UseRewardedAds)
                    LoadRewardedAd();

                if (!(_interstitialAd != null && _interstitialAd.CanShowAd()) && Configs.AdsConfig.UseInterAds)
                    LoadInterstitialAd();

                if (Configs.AdsConfig.UseBannerAds)
                    LoadBannerAd();
            });
        }

        private void OnConsentInfoUpdated(FormError consentError)
        {
            if (consentError != null)
            {
                // Handle the error.
                Debug.LogError($"#ads# Consent update error: {consentError}");
                return;
            }

            // If the error is null, the consent information state was updated.
            // You are now ready to check if a form is available.
            ConsentForm.LoadAndShowConsentFormIfRequired(formError =>
            {
                if (formError != null)
                {
                    // Consent gathering failed.
                    Debug.LogError($"#ads# Consent form error: {formError}");
                    return;
                }

                // Consent has been gathered.
                SaveSystem.Inst.GeneralData.IsUserConsentForAds = ConsentInformation.CanRequestAds();
                SaveSystem.Inst.GeneralData.IsUserConsentAsked = true;

                if (SaveSystem.Inst.GeneralData.IsUserConsentForAds)
                {
                    // If user consent is given, initialize ads.
                    InitializeAds();
                }
            });
        }

        private void Update()
        {
            if (_interCallback)
            {
                _interCallback = false;
                _onInterClosed?.Invoke(true);
                _onInterClosed = null;
            }

            if (_rewardLoadedCallback)
            {
                _rewardLoadedCallback = false;
                OnRewardedLoaded?.Invoke();

            }
        }


        #region Banner
        public void ShowBannerAd()
        {
            if (SaveSystem.Inst.SaveData.IsFirstSession)
            {
                return;
            }
            if (!Configs.AdsConfig.UseBannerAds)
            {
                return;
            }

            if (_bannerView != null)
            {
                // Debug.Log("#ads# Showing banner view.");
                _bannerView.Show();
                if (_isBannerLoaded && _bannerPanel != null)
                    _bannerPanel.SetActive(true);
            }
        }

        public void HideBannerAd()
        {
            if (_bannerView != null)
            {
                // Debug.Log("#ads# Hiding banner view.");
                _bannerView.Hide();
                if (_bannerPanel != null)
                    _bannerPanel.SetActive(false);
            }
        }

        private void CreateBannerView()
        {
            // Debug.Log("#ads# Creating banner view");

            if (!Configs.AdsConfig.UseBannerAds) return;

            // If we already have a banner, destroy the old one.
            if (_bannerView != null)
            {
                _bannerView.Destroy();
                _bannerView = null;
            }

            // Create a 320x50 banner at top of the screen
            string adUnitId = Configs.AdsConfig.GetBannerAdUnitId();
            _bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
        }

        private void LoadBannerAd()
        {
            if (!Configs.AdsConfig.UseBannerAds) return;

            _isBannerLoaded = false;
            _bannerReloadCount++;
            if (_bannerReloadCount > 3)
                return;

            if (!_isInitialized)
            {
                InitializeAds();
                ReloadBanner();
                return;
            }

            // create an instance of a banner view first.
            if (_bannerView == null)
            {
                CreateBannerView();
            }

            // create our request used to load the ad.
            var adRequest = new AdRequest();

            // send the request to load the ad.
            // Debug.Log("#ads# Loading banner ad.");
            ListenToAdEvents();
            _bannerView.LoadAd(adRequest);
        }

        private void ListenToAdEvents()
        {
            // Raised when an ad is loaded into the banner view.
            _bannerView.OnBannerAdLoaded += () =>
            {
                // Debug.Log("#ads# Banner view loaded an ad with response : " + _bannerView.GetResponseInfo());
                _isBannerLoaded = true;
                HideBannerAd();
            };
            // Raised when an ad fails to load into the banner view.
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) =>
            {
                // Debug.LogError("#ads# Banner view failed to load an ad with error : " + error);
            };
            // Raised when the ad is estimated to have earned money.
            _bannerView.OnAdPaid += (AdValue adValue) =>
            {
                // Debug.Log(string.Format("#ads# Banner view paid {0} {1}.",
                // adValue.Value,
                //     adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            _bannerView.OnAdImpressionRecorded += () =>
            {
                // Debug.Log("#ads# Banner view recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            _bannerView.OnAdClicked += () =>
            {
                // Debug.Log("#ads# Banner view was clicked.");
            };
            // Raised when an ad opened full screen content.
            _bannerView.OnAdFullScreenContentOpened += () =>
            {
                // Debug.Log("#ads# Banner view full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            _bannerView.OnAdFullScreenContentClosed += () =>
            {
                // Debug.Log("#ads# Banner view full screen content closed.");
            };
        }

        private void ReloadBanner()
        {
            Invoke(nameof(LoadBannerAd), 2f);
        }
        #endregion


        #region Interstitial
        public void ShowInterstitialAd(Action<bool> callback)
        {
            if (!Configs.AdsConfig.UseInterAds || SaveSystem.Inst.SaveData.IsFirstSession)
            {
                callback?.Invoke(false);
                return;
            }

            if (_interstitialAd != null && _interstitialAd.CanShowAd())
            {
                // Debug.Log("#ads# Showing interstitial ad.");
                _onInterClosed = callback;
                _interstitialAd.Show();
            }
            else
            {
                // Debug.Log("#ads# Interstitial ad is not ready yet.");
                callback?.Invoke(false);
            }
        }

        private void LoadInterstitialAd()
        {
            if (!Configs.AdsConfig.UseInterAds)
                return;

            _interReloadCount++;
            if (_interReloadCount > 3)
                return;

            if (!_isInitialized)
            {
                InitializeAds();
                ReloadInter();
                return;
            }

            // Clean up the old ad before loading a new one.
            _interstitialAd?.Destroy();
            _interstitialAd = null;

            // Debug.Log("#ads# Loading the interstitial ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            if (SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted)
            {
                adRequest.Extras.Add("npa", "1");
            }

            // send the request to load the ad.
            string adUnitId = Configs.AdsConfig.GetInterAdUnitId();
            InterstitialAd.Load(adUnitId, adRequest, (ad, error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        ReloadInter();
                        // Debug.LogError("#ads# interstitial ad failed to load an ad " + "with error : " + error);
                        return;
                    }

                    // Debug.Log("#ads# Interstitial ad loaded with response : " + ad.GetResponseInfo());

                    _interstitialAd = ad;
                    _interReloadCount = 0;
                    RegisterEventHandlers(_interstitialAd);
                });
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd)
        {
            // Raised when the ad is estimated to have earned money.
            interstitialAd.OnAdPaid += (AdValue adValue) =>
            {
                // Debug.Log(string.Format("#ads# Interstitial ad paid {0} {1}.",
                // adValue.Value,
                //     adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            interstitialAd.OnAdImpressionRecorded += () =>
            {
                // Debug.Log("#ads# Interstitial ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            interstitialAd.OnAdClicked += () =>
            {
                // Debug.Log("#ads# Interstitial ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            interstitialAd.OnAdFullScreenContentOpened += () =>
            {
                // Debug.Log("#ads# Interstitial ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            interstitialAd.OnAdFullScreenContentClosed += () =>
            {
                // Debug.Log("#ads# Interstitial ad full screen content closed.");

                _interCallback = true;

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
            };
            // Raised when the ad failed to open full screen content.
            interstitialAd.OnAdFullScreenContentFailed += (AdError error) =>
            {
                // Debug.LogError("#ads# Interstitial ad failed to open full screen content " + "with error : " + error);

                // Reload the ad so that we can show another as soon as possible.
                LoadInterstitialAd();
            };
        }

        private void ReloadInter()
        {
            Invoke(nameof(LoadInterstitialAd), 2f);
        }
        #endregion


        #region Rewarded
        public void ShowRewardedAd(Action<bool> callback)
        {
            if (!Configs.AdsConfig.UseRewardedAds)
            {
                callback?.Invoke(false);
                return;
            }

            if (IsRewardedAdLoaded)
            {
                _onRewardedClosed = callback;
                _rewardShowing = true;
                _rewardGranted = false;
                var shown = _rewardedAd;
                shown.Show(reward => OnUnityThread(() =>
                {
                    if (!_rewardShowing || shown != _rewardedAd || _rewardGranted) return;
                    _rewardGranted = true;
                    var completed = _onRewardedClosed;
                    _onRewardedClosed = null;
                    completed?.Invoke(true);
                    OnRewardedWathed?.Invoke();
                }));
            }
            else callback?.Invoke(false);
        }

        private void FinishRewarded(RewardedAd shown)
        {
            if (shown != _rewardedAd || !_rewardShowing) return;
            _rewardShowing = false;
            var completed = _onRewardedClosed;
            _onRewardedClosed = null;
            completed?.Invoke(false); // Closing/failure without the SDK reward event is not success.
            LoadRewardedAd();
        }

        private void LoadRewardedAd()
        {
            if (!Configs.AdsConfig.UseRewardedAds) return;

            _rewardedReloadCount++;
            if (_rewardedReloadCount > 3)
                return;

            if (!_isInitialized)
            {
                InitializeAds();
                ReloadRewarded();
                return;
            }

            // Clean up the old ad before loading a new one.
            _rewardedAd?.Destroy();
            _rewardedAd = null;

            // Debug.Log("#ads# Loading the rewarded ad.");

            // create our request used to load the ad.
            var adRequest = new AdRequest();
            if (SaveSystem.Inst.GeneralData.IsPrivacyPolicyAccepted)
            {
                adRequest.Extras.Add("npa", "1");
            }

            // send the request to load the ad.
            string adUnitId = Configs.AdsConfig.GetRewardedAdUnitId();
            RewardedAd.Load(adUnitId, adRequest, (RewardedAd ad, LoadAdError error) =>
                {
                    // if error is not null, the load request failed.
                    if (error != null || ad == null)
                    {
                        ReloadRewarded();
                        // Debug.LogError("#ads# Rewarded ad failed to load an ad " + "with error : " + error);
                        return;
                    }

                    // Debug.Log("#ads# Rewarded ad loaded with response : " + ad.GetResponseInfo());

                    _rewardLoadedCallback = true;
                    _rewardedAd = ad;
                    _rewardedReloadCount = 0;
                    RegisterEventHandlers(_rewardedAd);
                });
        }

        private void RegisterEventHandlers(RewardedAd ad)
        {
            // Raised when the ad is estimated to have earned money.
            ad.OnAdPaid += (AdValue adValue) =>
            {
                // Debug.Log(string.Format("#ads# Rewarded ad paid {0} {1}.",
                // adValue.Value,
                //     adValue.CurrencyCode));
            };
            // Raised when an impression is recorded for an ad.
            ad.OnAdImpressionRecorded += () =>
            {
                // Debug.Log("#ads# Rewarded ad recorded an impression.");
            };
            // Raised when a click is recorded for an ad.
            ad.OnAdClicked += () =>
            {
                // Debug.Log("#ads# Rewarded ad was clicked.");
            };
            // Raised when an ad opened full screen content.
            ad.OnAdFullScreenContentOpened += () =>
            {
                // Debug.Log("#ads# Rewarded ad full screen content opened.");
            };
            // Raised when the ad closed full screen content.
            ad.OnAdFullScreenContentClosed += () =>
            {
                // Debug.Log("#ads# Rewarded ad full screen content closed.");
                OnUnityThread(() => FinishRewarded(ad));
            };
            // Raised when the ad failed to open full screen content.
            ad.OnAdFullScreenContentFailed += (AdError error) =>
            {
                // Debug.LogError("#ads# Rewarded ad failed to open full screen content " + "with error : " + error);

                OnUnityThread(() => FinishRewarded(ad));
            };
        }

        private void ReloadRewarded()
        {
            Invoke(nameof(LoadRewardedAd), 2f);
        }
        #endregion
    }
}
