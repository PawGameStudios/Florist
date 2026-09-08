namespace Florist.Merge
{
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "MergeGame/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    public ItemType ItemType;
    public Sprite[] Sprites; // Kademeli görseller (1,2,3...)
    public float[] SpawnChances;
    public Sprite ProducerSprite; // Üretici görseli
    public Sprite[] FixedSprites; // Sabit ürün kademeli görselleri
}
}
