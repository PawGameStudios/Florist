using System;
using UnityEngine;

[DefaultExecutionOrder(-500)]
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Inst => s_instance;
    private static SaveSystem s_instance;

    public GeneralData GeneralData;
    public ShopData ShopData;
    private const string _generalDataKey = "HTndkrl.dat";
    private const string _shopDataKey = "YUCmas2.dat";

    private void Awake()
    {
        if (Inst == null)
        {
            s_instance = this;
            DontDestroyOnLoad(gameObject);
            Load();
        }
        else
        {
            DestroyImmediate(gameObject);
        }

        // Screen.sleepTimeout = SleepTimeout.NeverSleep;
        // Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
    }

    private void Start()
    {
        ShopData.Initialize();
    }


    public void Load()
    {
        Debug.Log("#save# SaveSystem.Load");
        try
        {
            if (PlayerPrefs.GetInt("FirstTime") == 0)
            {
                FileManager.FirstTime();
                FirstTime();
                return;
            }

            GeneralData = FileManager.Load<GeneralData>(_generalDataKey);
            ShopData = FileManager.Load<ShopData>(_shopDataKey);
        }
        catch (Exception ex)
        {
            PrintErrorLog(ex, "Load");
        }
    }

    public void Save()
    {
        Debug.Log("#save# SaveSystem.Save");
        try
        {
            GeneralData.IsFirstSession = false;

            FileManager.Save(_generalDataKey, GeneralData);
            FileManager.Save(_shopDataKey, ShopData);
        }
        catch (Exception ex)
        {
            PrintErrorLog(ex, "Save");
        }
    }

    private void FirstTime()
    {
        Debug.Log("FirstTime");
        GeneralData = new();
        ShopData = new();
        PlayerPrefs.SetInt("FirstTime", 1);
    }

    private void PrintErrorLog(Exception ex, string msg)
    {
        string errLog = $"{msg}\n";
        errLog += $"FirstTime: {PlayerPrefs.GetInt("FirstTime")}\n";
        errLog += $"GeneralData: {GeneralData != null}\n";
        errLog += $"ShopData: {ShopData != null}\n";
        errLog += $"ex: {ex}";

        // if (FirebaseController.Instance != null)
        // {
        //     FirebaseController.Instance.SetCrashlyticsLog(errLog);
        //     FirebaseController.Instance.SendException(ex);
        // }
        Debug.LogError(errLog);
    }


#if UNITY_EDITOR
    void OnApplicationQuit()
    {
        if (s_instance == this)
        {
            Save();
        }
    }
#else
    public void OnApplicationPause(bool paused)
    {
        if (paused && s_instance == this)
        {
            Save();
        }
    }
#endif
}
