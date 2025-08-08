using System;
using UnityEngine;
using TMPro;
using Ads;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

public class MainPage : Page
{
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _pcButton;
    [SerializeField] private Transform _playButtonTransform;
    [SerializeField] private Popup _lifePopup;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private List<GameObject> _energyObjects;
    [SerializeField] private TextMeshProUGUI _lifeDurationText;
    [SerializeField] private PrivacyController _privacyController;
    [SerializeField] private Tutorial _tutorial;
    private RateUsController _rateUsController;
    private double _remainingSecsForLife;

    void OnEnable()
    {
        GeneralData.LifeAmountChanged += LifeChangedHandler;
        Timer.TimeTickSeconds += TimeTickHandler;
    }

    void OnDisable()
    {
        GeneralData.LifeAmountChanged -= LifeChangedHandler;
        Timer.TimeTickSeconds -= TimeTickHandler;
    }

    public override void Close(PageParams pageData = null, Action onCompleted = null)
    {
        gameObject.SetActive(false);
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        Debug.LogError("MainPage Opened");
        base.Open(pageData, onCompleted);
        gameObject.SetActive(true);

        _dayText.text = $"{LocalizationManager.GetLocalizedText("day", LocalizationManager.TextType.TITLE)} {SaveSystem.Inst.GeneralData.CurrentDayIndex + 1}";

        _remainingSecsForLife = SaveSystem.Inst.GeneralData.GetRemainingLifeTime();
        TimeSpan timeSpan = TimeSpan.FromSeconds(_remainingSecsForLife);

        if (SaveSystem.Inst.GeneralData.Life == Configs.ProfileConfig.MaxLife)
        {
            _lifeDurationText.text = $"{LocalizationManager.GetLocalizedText("full_life", LocalizationManager.TextType.UPPER)}";
        }
        else
        {
            _lifeDurationText.text = $"{GetTimeFormat(timeSpan)}";
        }

        for (int i = 0; i < _energyObjects.Count; i++)
        {
            _energyObjects[i].SetActive(i < SaveSystem.Inst.GeneralData.Life);
        }

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _shopButton.SetActive(false);
            _pcButton.SetActive(false);

            _tutorial.Init()
                    .PointTo(_playButtonTransform.position, Tutorial.PointDirection.Right)
                    .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                    .SetClickableState(Tutorial.ClickableState.None)
                    .SetActivationDelay(2f)
                    .StartTutorial();
        }
        else
        {
            _privacyController.ShowPrivacyScreen();
            _shopButton.SetActive(true);
            _pcButton.SetActive(true);
        }

        if (pageData != null)
        {
            if (pageData.PreviousPage == PageType.Bootstrapper)
            {
                _rateUsController = new();
                _rateUsController.Initialize();
            }
            else
            {
                _rateUsController.TryShowRateUs();
            }
        }
    }

    public void OnPlayClicked()
    {
        SaveSystem.Inst.GeneralData.ChangeLife(-1);

        if (SaveSystem.Inst.GeneralData.Life <= 0)
        {
            SaveSystem.Inst.GeneralData.Life = 0;
            _lifePopup.SetPositiveButtonListener(() =>
                    {
                        AdManager.Instance.ShowRewardedAd(isWatched =>
                        {
                            if (isWatched)
                            {
                                SaveSystem.Inst.GeneralData.ChangeLife(1);
                            }
                        });
                        _lifePopup.Close();
                    })
                    .SetNegaiveButtonListener(() =>
                    {
                        _lifePopup.Close();
                    })
                    .Open();
        }
        else
        {
            gameObject.SetActive(false);
            References.DukkanPage.Open();
            // Close();

            HapticsController.PlayButtonHaptic();

            if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
            {
                _tutorial.FinishTutorial();
            }
        }
    }

    public void OnShopClicked()
    {
        References.ShopPage.Open();
        HapticsController.PlayButtonHaptic();
    }

    public void OnPcClicked()
    {
        HapticsController.PlayButtonHaptic();
    }

    public void OnLifeAdClicked()
    {
        AdManager.Instance.ShowRewardedAd(isWatched =>
        {
            if (isWatched)
            {
                SaveSystem.Inst.GeneralData.ChangeLife(1);
            }
        });
    }

    private void LifeChangedHandler()
    {
        _remainingSecsForLife = SaveSystem.Inst.GeneralData.GetRemainingLifeTime();
        for (int i = 0; i < _energyObjects.Count; i++)
        {
            _energyObjects[i].SetActive(i < SaveSystem.Inst.GeneralData.Life);
        }
    }

    private void TimeTickHandler()
    {
        if (_remainingSecsForLife <= 0)
        {
            return;
        }

        _remainingSecsForLife--;
        TimeSpan timeSpan = TimeSpan.FromSeconds(_remainingSecsForLife);
        _lifeDurationText.text = $"{GetTimeFormat(timeSpan)}";
    }

    public static string GetTimeFormat(TimeSpan timeSpan, string msg = "", int limit = 3)
    {
        string time;

        if (timeSpan.Days > 0)
        {
            if (limit == 3)
                time = $"{msg}{timeSpan.Days}{LocalizationManager.GetLocalizedText("day_short")} {timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")} {timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")}";
            else if (limit == 2)
                time = $"{msg}{timeSpan.Days}{LocalizationManager.GetLocalizedText("day_short")} {timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")}";
            else if (limit == 1)
                time = $"{msg}{timeSpan.Days}{LocalizationManager.GetLocalizedText("day_short")}";
            else
                time = $"{msg}{timeSpan.Days}{LocalizationManager.GetLocalizedText("day_short")} {timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")} {timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")} {timeSpan.Seconds}{LocalizationManager.GetLocalizedText("second_short")}";
        }
        else if (timeSpan.Hours > 0)
        {
            if (limit == 2)
                time = $"{msg}{timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")} {timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")}";
            else if (limit == 1)
                time = $"{msg}{timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")}";
            else
                time = $"{msg}{timeSpan.Hours}{LocalizationManager.GetLocalizedText("hour_short")} {timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")} {timeSpan.Seconds}{LocalizationManager.GetLocalizedText("second_short")}";
        }
        else if (timeSpan.Minutes > 0)
        {
            if (limit == 2)
                time = $"{msg}{timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")} {timeSpan.Seconds}{LocalizationManager.GetLocalizedText("second_short")}";
            else
                time = $"{msg}{timeSpan.Minutes}{LocalizationManager.GetLocalizedText("minute_short")} {timeSpan.Seconds}{LocalizationManager.GetLocalizedText("second_short")}";
        }
        else
        {
            time = $"{msg}{timeSpan.Seconds}{LocalizationManager.GetLocalizedText("second_short")}";
        }

        return time;
    }
}
