using UnityEngine;

public enum PageType
{
    MainPage,
    Shop,
    Dukkan,
    Workshop,
    EndDay
}

public class SaveData
{
    public PageType LastPage;
    public EarningsInfo EarningsInfo;
    public DukkanState DukkanState;
    public int CurrentCustomerIndex;
    public float HappinessValue;
    public float TotalTimePassed;

    public SaveData()
    {
        LastPage = PageType.MainPage;
    }

    public void SaveGame()
    {
        if (LastPage == PageType.Dukkan)
        {
            EarningsInfo = References.DukkanPage.EarningsInfo;
            CurrentCustomerIndex = References.DukkanPage.CurrentCustomerIndex;
            DukkanState = References.DukkanPage.DukkanState;
            HappinessValue = References.HappinessMeter.HappinessValue;
            TotalTimePassed = References.DayTimeManager.TotalTimePassed;
        }
    }

    public void LoadGame()
    {

    }
}
