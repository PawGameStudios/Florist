using System;
using System.Collections.Generic;
using UnityEngine;
using Config;

public enum PageType
{
    Bootstrapper,
    MainPage,
    Shop,
    Dukkan,
    Workshop,
    EndDay,
    OpeningPage,
    TopCanvas
}

public enum DukkanSaveState
{
    None,
    CustomerProgress,
    InWorkshop,
    FlowerReady,
    Payment,
    Done
}

[Serializable]
public class DukkanParams
{
    public CustomerInfo CurrentCustomerInfo;
    public List<BouquetModel> CurrentOrder;
    public BouquetModel DeliveredBouquet;
    public List<string> ConvoHistory;
    public OrderInfo OrderInfo;
    public EarningsInfo EarningsInfo;
    public DayInfo DayInfo;
    public DukkanSaveState DukkanState;
    public int NextCustomerIndex;
    public int CurrentTick;
    public float HappinessValue;
    public float TotalTimePassed;
}

[Serializable]
public class WorkshopParams
{
    public PaperArea.State PaperState;
    public BouquetModel CurrentFlowers;
}

[Serializable]
public class SaveData
{
    public bool IsFirstSession;
    public bool IsTutorialFinished;
    public bool IsDukkanTutorialFinished;
    public bool IsPosTutorialFinished;
    public PageType LastPage;
    public DukkanParams DukkanParams;
    public List<WorkshopParams> WorkshopParams;
    public EarningsInfo EarningsInfo;

    public SaveData()
    {
        LastPage = PageType.MainPage;
        IsFirstSession = true;
        IsTutorialFinished = false;
        IsDukkanTutorialFinished = false;
        IsPosTutorialFinished = false;
    }

    public void SaveGame()
    {
        if (LastPage == PageType.Dukkan)
        {
            EarningsInfo = null;
            SetDukkanParams();
            WorkshopParams = References.WorkshopPage.GetPaperSaveInfo();
        }
        else if (LastPage == PageType.EndDay)
        {
            DukkanParams = null;
            EarningsInfo = References.DukkanPage.EarningsInfo;
        }
        else if (LastPage == PageType.Workshop)
        {
            SetDukkanParams();
            WorkshopParams = References.WorkshopPage.GetPaperSaveInfo();
        }
    }

    public void LoadGame()
    {
        Debug.LogError("LoadGame, LastPage: " + LastPage);
        if (LastPage == PageType.MainPage)
        {
            References.MainPage.Open();
        }
        else if (LastPage == PageType.Dukkan)
        {
            References.MainPage.gameObject.SetActive(false);
            References.DukkanPage.Open(new PageParams
            {
                LoadFromSaveData = true,
                PreviousPage = PageType.MainPage
            });
        }
        else if (LastPage == PageType.EndDay)
        {
            // References.EndDayPage.SetData(EarningsInfo);
            References.MainPage.gameObject.SetActive(false);
            References.EndDayPage.Open();
        }
        else if (LastPage == PageType.Workshop)
        {
            References.MainPage.gameObject.SetActive(false);
            References.DukkanPage.Open(new PageParams
            {
                LoadFromSaveData = true,
                PreviousPage = PageType.MainPage
            });
        }
        else
        {
            References.MainPage.Open();
        }
    }

    private void SetDukkanParams()
    {
        DukkanParams = new DukkanParams
        {
            CurrentCustomerInfo = References.DukkanPage.CurrentCustomerInfo,
            CurrentOrder = References.DukkanPage.CurrentOrder,
            DeliveredBouquet = References.DukkanPage.DeliveredBouquet,
            ConvoHistory = References.DukkanPage.ConvoHistory,
            OrderInfo = References.DukkanPage.OrderInfo,
            EarningsInfo = References.DukkanPage.EarningsInfo,
            DayInfo = References.DukkanPage.DayInfo,
            NextCustomerIndex = References.DukkanPage.CurrentCustomerIndex,
            DukkanState = References.DukkanPage.DukkanState,
            HappinessValue = References.HappinessMeter.HappinessValue,
            CurrentTick = References.HappinessMeter.CurrentTick,
            TotalTimePassed = References.DayTimeManager.TotalTimePassed
        };
    }
}
