using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
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


    public void Load()
    {
        Debug.Log("#save# SaveSystem.Load");
        try
        {
            if (PlayerPrefs.GetInt("FirstTime") == 0)
            {
                FirstTime();
                return;
            }

            BinaryFormatter bf = new();
            GeneralData = LoadSingleFile<GeneralData>(_generalDataKey, bf);
            ShopData = LoadSingleFile<ShopData>(_shopDataKey, bf);
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
            BinaryFormatter bf = new();

            GeneralData.IsFirstSession = false;

            SaveSingleFile(_generalDataKey, bf, GeneralData);
            SaveSingleFile(_shopDataKey, bf, ShopData);
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

    private T LoadSingleFile<T>(string dataKey, BinaryFormatter bf)
    {
        T data = default;
        string path = $"{Application.persistentDataPath}/{dataKey}";
        bool fileExists = File.Exists(path);
        if (fileExists)
        {
            FileStream file = File.Open(path, FileMode.Open);
            data = (T)bf.Deserialize(file);
            file.Close();
        }
        return data;
    }

    private void SaveSingleFile<T>(string dataKey, BinaryFormatter bf, T data)
    {
        FileStream file = File.Create($"{Application.persistentDataPath}/{dataKey}");
        bf.Serialize(file, data);
        file.Close();
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
