using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IAPConfig", menuName = "PlayerDuo/Configs/IAP")]
public class IAPConfig : ScriptableObject
{
    public string RemoveAdsProductID;
    public string PiggyProductID;

    public List<string> Products;
}

