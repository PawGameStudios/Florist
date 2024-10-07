#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class Resetter : MonoBehaviour
{

    [MenuItem("Paw/Save Load/Reset Data", priority = 1)]
    private static void ResetData()
    {
        // reset in game data
        string dataPath = Path.Combine(Application.persistentDataPath, "Data");
        if (Directory.Exists(dataPath))
            Directory.Delete(dataPath, true);

        PlayerPrefs.DeleteAll();
    }

    [MenuItem("Paw/Save Load/Reset Backup Data")]
    private static void ResetBackupData()
    {
        // reset in game backup data
        string dataPath = Path.Combine(Application.persistentDataPath, "Data_backup");
        if (Directory.Exists(dataPath))
            Directory.Delete(dataPath, true);
    }

    [MenuItem("Paw/Save Load/Backup the Data")]
    private static void BackUpData()
    {
        // backup in game data
        string dataPath = Path.Combine(Application.persistentDataPath, "Data");
        string backupPath = Path.Combine(Application.persistentDataPath, "Data_backup");
        FileUtil.CopyFileOrDirectory(dataPath, backupPath);
    }

    [MenuItem("Paw/Save Load/Use Backup Data")]
    private static void UseBackUpData()
    {
        ResetData();
        string dataPath = Path.Combine(Application.persistentDataPath, "Data");
        string backupPath = Path.Combine(Application.persistentDataPath, "Data_backup");
        FileUtil.CopyFileOrDirectory(backupPath, dataPath);

    }
}
#endif