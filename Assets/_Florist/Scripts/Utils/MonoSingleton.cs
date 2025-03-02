using UnityEngine;

public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
{
    private static T s_instance;

    public static T Instance
    {
        get
        {
            if (s_instance != null) return s_instance;
            return null;
        }
    }

    private void Awake()
    {
        if (s_instance == null)
        {
            DontDestroyOnLoad(gameObject);
            s_instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}