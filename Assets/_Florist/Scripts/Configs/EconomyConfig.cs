using UnityEngine;

[CreateAssetMenu(fileName = "EconomyConfig", menuName = "Paw/Configs/Economy")]
public sealed class EconomyConfig : ScriptableObject
{
    [Header("New save only")]
    [Min(0)] public float StartingMoney = 10;
    [Min(0)] public float StartingDiamonds;
    [Min(0)] public int StartingLife = 5;
    [Min(0)] public int StartingMachineLevel;

    [Header("Day and rewarded ads")]
    [Min(0)] public int DayEntryLifeCost = 1;
    [Min(0)] public int RewardedAdLife = 1;
    [Min(0)] public int DailyRent;

    [Header("Cash handed to the cashier (not a tip)")]
    public Vector2 CustomerExtraCashFraction = new Vector2(0.05f, 0.25f);

    [Header("Generated orders after the story days")]
    [Range(2, 6)] public int RandomOrderFlowerCount = 3;
    [Range(0, 1)] public float LatestFlowerOrderChance = 0.5f;
}
