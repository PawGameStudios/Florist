using UnityEngine;

public class Configs : MonoBehaviour
{
    private static Configs s_instance;

    [SerializeField] private ShopConfig _shopConfig = null;
    [SerializeField] private LevelConfig _levelConfig = null;

    public static ShopConfig ShopConfig => s_instance._shopConfig;
    public static LevelConfig LevelConfig => s_instance._levelConfig;


    private void Awake() => s_instance = this;
}
