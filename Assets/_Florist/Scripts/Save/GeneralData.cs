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
        Life += amount;
        Life = Mathf.Clamp(Life, 0, Configs.ProfileConfig.MaxLife);
        LifeAmountChanged?.Invoke();

        if (Life < Configs.ProfileConfig.MaxLife)
        {
            LifeRefreshTime = Timer.CurrentTotalSeconds;
        }
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
        Debug.Log($"Life set to {Life} with maxlife {Configs.ProfileConfig.MaxLife} and refresh time {LifeRefreshTime}");
        if (Life == Configs.ProfileConfig.MaxLife)
        {
            return;
        }

        if (LifeRefreshTime <= 0)
        {
            LifeRefreshTime = Timer.CurrentTotalSeconds;
            Debug.Log("LifeRefreshTime initialized to current time.");
        }

        int safe = 0;
        float lifeGainSeconds = Configs.ProfileConfig.LifeGainMinutes * 60;
        while (LifeRefreshTime + lifeGainSeconds < Timer.CurrentTotalSeconds)
        {
            if (safe++ > 500)
            {
                LifeRefreshTime = Timer.CurrentTotalSeconds;
                Life = Configs.ProfileConfig.MaxLife;
                Debug.LogError("Infinite loop detected in SetLife method.");
                break;
            }

            LifeRefreshTime += lifeGainSeconds;
            Life++;
            Life = Mathf.Clamp(Life, 0, Configs.ProfileConfig.MaxLife);
        }

        if (Life < Configs.ProfileConfig.MaxLife)
        {
            _currentRefreshTime = Timer.CurrentTotalSeconds;
            Timer.TimeTickSeconds += TimeTickHandler;
        }
    }

    public void StopTimeTick()
    {
        Timer.TimeTickSeconds -= TimeTickHandler;
    }

    public double GetRemainingLifeTime()
    {
        if (Life == Configs.ProfileConfig.MaxLife)
        {
            return 0;
        }

        double nextRefreshTime = LifeRefreshTime + (Configs.ProfileConfig.LifeGainMinutes * 60);
        return nextRefreshTime - Timer.CurrentTotalSeconds;
    }

    private void TimeTickHandler()
    {
        float lifeGainSeconds = Configs.ProfileConfig.LifeGainMinutes * 60;
        double nextRefreshTime = LifeRefreshTime + lifeGainSeconds;

        _currentRefreshTime++;
        if (_currentRefreshTime >= nextRefreshTime)
        {
            Life++;
            Life = Mathf.Clamp(Life, 0, Configs.ProfileConfig.MaxLife);
            LifeRefreshTime = _currentRefreshTime;

            if (Life == Configs.ProfileConfig.MaxLife)
            {
                Timer.TimeTickSeconds -= TimeTickHandler;
                Debug.Log("Life is full, stopping time tick.");
            }
            else
            {
                nextRefreshTime = _currentRefreshTime + lifeGainSeconds;
            }

            LifeAmountChanged?.Invoke();
        }
    }
}
