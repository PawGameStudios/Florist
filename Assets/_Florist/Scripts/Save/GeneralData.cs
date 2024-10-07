using System;

[System.Serializable]
public class GeneralData
{
    public static Action MoneyAmountChanged, DiamondAmountChanged;
    public bool IsFirstSession;
    public long Money;
    public long Diamonds;

    public GeneralData()
    {
        IsFirstSession = true;
        Money = 10;
        Diamonds = 0;
    }

    public void AddMoney(long amount)
    {
        Money += amount;
        MoneyAmountChanged?.Invoke();
    }

    public void AddDiamonds(long amount)
    {
        Diamonds += amount;
        DiamondAmountChanged?.Invoke();
    }
}
