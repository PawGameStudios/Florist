using System;
using UnityEngine;
using TMPro;

public class MainPage : Page
{
    [SerializeField] private GameObject _shopButton;
    [SerializeField] private GameObject _pcButton;
    [SerializeField] private Transform _playButtonTransform;
    [SerializeField] private Popup _lifePopup;
    [SerializeField] private TextMeshProUGUI _dayText;
    [SerializeField] private TextMeshProUGUI _lifeText;
    [SerializeField] private TextMeshProUGUI _lifeDurationText;
    [SerializeField] private Tutorial _tutorial;
    private double _remainingSecsForLife;

    void OnEnable()
    {
        SaveSystem.Inst.GeneralData.SetLife();

        CheckSaveData();
        LifeChangedHandler();
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
    }

    public override void Open(PageParams pageData = null, Action onCompleted = null)
    {
        base.Open(pageData, onCompleted);
        gameObject.SetActive(true);

        _dayText.text = $"Day {SaveSystem.Inst.GeneralData.CurrentDayIndex + 1}";

        _remainingSecsForLife = SaveSystem.Inst.GeneralData.GetRemainingLifeTime();
        TimeSpan timeSpan = TimeSpan.FromSeconds(_remainingSecsForLife);
        _lifeDurationText.text = $"{GetTimeFormat(timeSpan)}";
        _lifeText.text = $"{SaveSystem.Inst.GeneralData.Life}";

        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _shopButton.SetActive(false);
            _pcButton.SetActive(false);
        }
        else
        {
            _shopButton.SetActive(true);
            _pcButton.SetActive(true);
        }
    }

    public void OnPlayClicked()
    {
        SaveSystem.Inst.GeneralData.ChangeLife(-1);

        if (SaveSystem.Inst.GeneralData.Life <= 0)
        {
            SaveSystem.Inst.GeneralData.Life = 0;
            _lifePopup.Open();
        }
        else
        {
            gameObject.SetActive(false);
            References.DukkanPage.Open();

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
        // TODO: ad
        SaveSystem.Inst.GeneralData.ChangeLife(1);
    }

    private void CheckSaveData()
    {
        if (!SaveSystem.Inst.SaveData.IsTutorialFinished)
        {
            _tutorial.Init()
                    .PointTo(_playButtonTransform.position, Tutorial.PointDirection.Right)
                    .SetObjectActivation(Tutorial.ObjectActivationOptions.Hand)
                    .SetClickableState(Tutorial.ClickableState.None)
                    .SetActivationDelay(2f)
                    .StartTutorial();
        }

        if (SaveSystem.Inst.SaveData.LastPage == PageType.MainPage)
        {
            // do nothing
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.Dukkan)
        {
            // TODO:
            // gameObject.SetActive(false);
            // PageData pageData = new()
            // {
            //     LoadFromSaveData = true,
            // };
            // References.DukkanPage.Open(pageData);
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.Workshop)
        {
            // TODO:
        }
        else if (SaveSystem.Inst.SaveData.LastPage == PageType.EndDay)
        {
            // TODO:
        }
    }

    private void LifeChangedHandler()
    {
        _remainingSecsForLife = SaveSystem.Inst.GeneralData.GetRemainingLifeTime();
        _lifeText.text = $"{SaveSystem.Inst.GeneralData.Life}";
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
                time = $"{msg}{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (limit == 2)
                time = $"{msg}{timeSpan.Days}d {timeSpan.Hours}h";
            else if (limit == 1)
                time = $"{msg}{timeSpan.Days}d";
            else
                time = $"{msg}{timeSpan.Days}d {timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Minutes}s";
        }
        else if (timeSpan.Hours > 0)
        {
            if (limit == 2)
                time = $"{msg}{timeSpan.Hours}h {timeSpan.Minutes}m";
            else if (limit == 1)
                time = $"{msg}{timeSpan.Hours}h";
            else
                time = $"{msg}{timeSpan.Hours}h {timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else if (timeSpan.Minutes > 0)
        {
            if (limit == 2)
                time = $"{msg}{timeSpan.Minutes}m";
            else
                time = $"{msg}{timeSpan.Minutes}m {timeSpan.Seconds}s";
        }
        else
        {
            time = $"{msg}{timeSpan.Seconds}s";
        }

        return time;
    }
}
