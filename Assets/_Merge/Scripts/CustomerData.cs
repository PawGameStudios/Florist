namespace Florist.Merge
{
using UnityEngine;

[CreateAssetMenu(fileName = "CustomerData", menuName = "MergeGame/CustomerData", order = 3)]
public class CustomerData : ScriptableObject
{
    public Sprite customerSprite;

    public ProductionItemData[] preferredItems;

    public int baseReward = 50;
    public string displayName;
    [Min(0)] public int minimumCompletedOrders;
    [Min(1)] public int selectionWeight = 1;
    [Range(1, 2)] public int maxProductTypes = 1;

    public enum CustomerType
    {
        Regular,    // Normal müşteri
        VIP,        // VIP müşteri (daha yüksek ödül)
        Rare        // Nadir müşteri (özel ürünler ister)
    }

    public CustomerType customerType = CustomerType.Regular;

    // Müşterinin rastgele bir ürün istemesini sağla
    public ProductionItemData GetRandomPreferredItem()
    {
        if (preferredItems == null || preferredItems.Length == 0)
            return null;

        return preferredItems[Random.Range(0, preferredItems.Length)];
    }
}

}
