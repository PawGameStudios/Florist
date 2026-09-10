namespace Florist.Merge
{
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "RecipeData", menuName = "MergeGame/RecipeData", order = 1)]
public class RecipeData : ScriptableObject
{
    public string recipeName;
    public string localizationKey;
    public string DisplayName => MergeLocalization.Name(localizationKey, recipeName);
    public Sprite recipeIcon;
    public List<RecipeIngredient> ingredients;
    public ProductionItemData resultItem;
    public int resultLevel = 1;
    public int resultCount = 1;
    public float productionTime = 14400f; // 4 saat (saniye cinsinden)
    public bool isUnlocked = true;

    // Each entry pays for advancing from the matching visible growth stage.
    public List<GardenStage> growthStages = new List<GardenStage>();

    // Üretim aşamaları için görseller
    public Sprite productionStage1Sprite; // İlk aşama görseli
    public Sprite productionStage2Sprite; // İkinci aşama görseli
    public Sprite productionStage3Sprite; // Üçüncü aşama görseli
}

[System.Serializable]
public class GardenStage
{
    public List<RecipeIngredient> ingredients = new List<RecipeIngredient>();
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemData itemData;
    public int requiredLevel;
    public int requiredCount;
}

}
