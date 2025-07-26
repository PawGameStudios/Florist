using System;
using UnityEngine;
using MEC;
using System.Collections.Generic;

[Serializable]
public class GeneralData
{
    public static Action MoneyAmountChanged, DiamondAmountChanged, LifeAmountChanged;
    public static Action AvatarChanged, PlayerNameChanged;
    public float Money;
    public float Diamonds;
    public int CurrentDayIndex;
    public int SelectedAvatarIndexInConfig;
    public int SelectedFrameIndexInConfig;
    public int Life;
    public int MachineLevel;
    public bool IsMusicOn;
    public bool IsSoundOn;
    public bool IsVibrationOn;
    public string PlayerName = "Player";
    public double LifeRefreshTime;
    private double _currentRefreshTime;

    public GeneralData()
    {
        Money = 10;
        Diamonds = 0;
        IsMusicOn = true;
        IsSoundOn = true;
        IsVibrationOn = true;
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

        // TODO:
        if (CurrentDayIndex >= Configs.LevelConfig.Days.Count)
            CurrentDayIndex = 0;
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
