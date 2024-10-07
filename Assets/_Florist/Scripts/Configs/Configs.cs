using UnityEngine;

public class Configs : MonoBehaviour
{
    private static Configs s_instance;

    [SerializeField] private ShopConfig _shopConfig = null;

    public static ShopConfig ShopConfig => s_instance._shopConfig;


    private void Awake() => s_instance = this;
}
