using System;

[Serializable]
public class GeneralData
{
    public static Action MoneyAmountChanged, DiamondAmountChanged;
    public static Action AvatarChanged, PlayerNameChanged;
    public bool IsFirstSession;
    public float Money;
    public float Diamonds;
    public int CurrentDayIndex;
    public int SelectedAvatarIndexInConfig;
    public int SelectedFrameIndexInConfig;
    public int MachineLevel;
    public bool IsMusicOn;
    public bool IsSoundOn;
    public bool IsVibrationOn;
    public string PlayerName = "Player";

    public GeneralData()
    {
        IsFirstSession = true;
        Money = 10;
        Diamonds = 0;
        IsMusicOn = true;
        IsSoundOn = true;
        IsVibrationOn = true;
        SelectedAvatarIndexInConfig = 0;
        SelectedFrameIndexInConfig = 0;
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
}
