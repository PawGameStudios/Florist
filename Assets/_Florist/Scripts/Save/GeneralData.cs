using System;

[Serializable]
public class GeneralData
{
    public static Action MoneyAmountChanged, DiamondAmountChanged;
    public bool IsFirstSession;
    public float Money;
    public float Diamonds;
    public int CurrentDayIndex;

    public GeneralData()
    {
        IsFirstSession = true;
        Money = 10;
        Diamonds = 0;
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
}
