
using UnityEngine;
using System.IO;
using System.Text;

public static class FileManager
{
    private const int XORKey = 96;
    private static readonly string s_dataPath = Path.Combine(Application.persistentDataPath, "Data");

    public static void FirstTime()
    {
        Directory.CreateDirectory(s_dataPath);
    }

    public static void Save(string path, object objectToSave)
    {
        string newPath = Path.Combine(s_dataPath, path);
        File.WriteAllBytes(newPath, Encrypt(JsonUtility.ToJson(objectToSave)));
    }

    public static T Load<T>(string newPath, string oldPath = null)
    {
        string path;
        path = Path.Combine(s_dataPath, newPath);
        if (File.Exists(path))
            return JsonUtility.FromJson<T>(Decrypt(File.ReadAllBytes(path)));

        if (oldPath != null)
        {
            path = Path.Combine(s_dataPath, oldPath);
            if (File.Exists(path))
                return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }
        return default;
    }

    private static string XOREncryptDecrypt(string textToEncrypt)
    {
        StringBuilder outSb = new(textToEncrypt.Length);
        for (int i = 0; i < textToEncrypt.Length; i++)
            outSb.Append((char)(textToEncrypt[i] ^ XORKey));
        return outSb.ToString();
    }

    private static byte[] Encrypt(string str)
    {
        return Encoding.ASCII.GetBytes(XOREncryptDecrypt(str));
    }

    private static string Decrypt(byte[] bytes)
    {
        return XOREncryptDecrypt(Encoding.ASCII.GetString(bytes));
    }
}