using UnityEngine;
using Config;

public class Configs : MonoBehaviour
{
    private static Configs s_instance;

    [SerializeField] private ShopConfig _shopConfig = null;
    [SerializeField] private LevelConfig _levelConfig = null;
    [SerializeField] private WorkshopConfig _workshopConfig = null;
    [SerializeField] private ProfileConfig _profileConfig = null;

    public static ShopConfig ShopConfig => s_instance._shopConfig;
    public static LevelConfig LevelConfig => s_instance._levelConfig;
    public static WorkshopConfig WorkshopConfig => s_instance._workshopConfig;
    public static ProfileConfig ProfileConfig => s_instance._profileConfig;

    private void Awake() => s_instance = this;
}
