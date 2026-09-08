using System;
using UnityEngine;
using MEC;
using System.Collections.Generic;

[Serializable]
public class GeneralData
{
    public static Action MoneyAmountChanged, DiamondAmountChanged, LifeAmountChanged;
    public static Action AvatarChanged, PlayerNameChanged;
    public static Action OnNotificationSettingsChanged;
    public float Money;
    public float Diamonds;
    public int CurrentDayIndex;
    public int LastSettledDayNumber;
    public int CurrentDayConfigIndex;
    public int SelectedAvatarIndexInConfig;
    public int SelectedFrameIndexInConfig;
    public int Life;
    public int MachineLevel;
    public bool IsMusicOn;
    public bool IsSoundOn;
    public bool IsVibrationOn;
    public bool IsNotificationsOn;
    public string PlayerName = "Player";
    public double LifeRefreshTime;
    public bool IsUserConsentAsked;
    public bool IsPrivacyPolicyAccepted;
    public bool IsPrivacyPolicyShown;
    public bool IsUserConsentForAds;
    public bool AskRateUs;
    private double _currentRefreshTime;

    public GeneralData()
    {
        Money = 10;
        Diamonds = 0;
        IsMusicOn = true;
        IsSoundOn = true;
        IsVibrationOn = true;
        IsUserConsentAsked = false;
        IsPrivacyPolicyAccepted = false;
        IsUserConsentForAds = true;
        IsPrivacyPolicyShown = false;
        SelectedAvatarIndexInConfig = 0;
        SelectedFrameIndexInConfig = 0;
        Life = 5;
        LifeRefreshTime = -1;
    }

    public void ChangeLife(int amount)
    {
        SetLife();
        bool wasFull = Life >= Configs.ProfileConfig.MaxLife;
        Life = Mathf.Clamp(Life + amount, 0, Configs.ProfileConfig.MaxLife);
        if (wasFull && Life < Configs.ProfileConfig.MaxLife)
            LifeRefreshTime = Timer.CurrentTotalSeconds;
        SetLife();
        LifeAmountChanged?.Invoke();
    }

    public void ChangeMoney(long amount)
    {
        Money += amount;
        Debug.LogError($"ChangeMoney called with amount: {amount}, new Money value: {Money}");
        MoneyAmountChanged?.Invoke();
    }

    public void ChangeMoney(float amount)
    {
        Money += amount;
        MoneyAmountChanged?.Invoke();
    }

    public void ChangeDiamonds(long amount)
    {
        Diamonds += amount;
        DiamondAmountChanged?.Invoke();
    }

    public void SwitchMusic()
    {
        IsMusicOn = !IsMusicOn;
    }

    public void SwitchSound()
    {
        IsSoundOn = !IsSoundOn;
    }

    public void SwitchVibration()
    {
        IsVibrationOn = !IsVibrationOn;
    }

    public void SwitchNotifications()
    {
        IsNotificationsOn = !IsNotificationsOn;
        OnNotificationSettingsChanged?.Invoke();
    }

    public void ChangeAvatar(int index)
    {
        SelectedAvatarIndexInConfig = index;
        AvatarChanged?.Invoke();
    }

    public void ChangePlayerName(string name)
    {
        PlayerName = name;
        PlayerNameChanged?.Invoke();
    }

    public void IncreaseDayIndex()
    {
        CurrentDayIndex++;
        CurrentDayConfigIndex++;

        if (CurrentDayConfigIndex >= Configs.LevelConfig.Days.Count)
            CurrentDayConfigIndex = Configs.LevelConfig.Days.Count;

        FirebaseController.Instance.SendCustomEvent($"day_reached_{CurrentDayIndex}");
    }

    public void SetLife()
    {
        Timer.TimeTickSeconds -= TimeTickHandler;
        int maxLife = Configs.ProfileConfig.MaxLife;
        Life = Mathf.Clamp(Life, 0, maxLife);
        double now = Timer.CurrentTotalSeconds;
        double interval = Math.Max(1, Configs.ProfileConfig.LifeGainMinutes * 60);
        if (Life < maxLife)
        {
            if (LifeRefreshTime <= 0 || LifeRefreshTime > now) LifeRefreshTime = now;
            int gained = (int)Math.Min(maxLife - Life, Math.Floor((now - LifeRefreshTime) / interval));
            if (gained > 0)
            {
                Life += gained;
                LifeRefreshTime += gained * interval;
                LifeAmountChanged?.Invoke();
            }
        }
        if (Life >= maxLife) LifeRefreshTime = -1;
        else Timer.TimeTickSeconds += TimeTickHandler;
    }

    public void StopTimeTick() => Timer.TimeTickSeconds -= TimeTickHandler;

    public double GetRemainingLifeTime() => Life >= Configs.ProfileConfig.MaxLife ? 0 :
        Math.Max(0, LifeRefreshTime + Math.Max(1, Configs.ProfileConfig.LifeGainMinutes * 60) - Timer.CurrentTotalSeconds);

    private void TimeTickHandler() => SetLife();
}
