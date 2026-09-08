using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Florist.Merge
{
    public class RequirementItem : MonoBehaviour
    {
        [SerializeField] private Image itemIcon;
        [SerializeField] private TextMeshProUGUI requirementText;
        [SerializeField] private Image statusIcon;
        [SerializeField] private Sprite checkmarkSprite;
        [SerializeField] private Sprite crossSprite;
        private RecipeIngredient ingredient;
        private ProductionManager productionManager;
        private InventoryManager inventoryManager;
        public void Initialize(RecipeIngredient ingredientData, ProductionManager production, InventoryManager inventory)
        {
            ingredient = ingredientData;
            productionManager = production;
            inventoryManager = inventory;
            if (itemIcon != null && ingredient.itemData != null && ingredient.itemData.Sprites != null && ingredient.requiredLevel > 0 && ingredient.requiredLevel <= ingredient.itemData.Sprites.Length)
                itemIcon.sprite = ingredient.itemData.Sprites[ingredient.requiredLevel - 1];
            UpdateStatus();
        }
        public void UpdateStatus()
        {
            if (ingredient == null || inventoryManager == null || productionManager == null) return;
            int currentCount = 0;
            foreach (var item in inventoryManager.GetInventory())
                if (item.IsRegularItem && item.itemData == ingredient.itemData && item.level >= ingredient.requiredLevel)
                    currentCount += item.count;
            if (requirementText != null) requirementText.text = $"Sv.{ingredient.requiredLevel}+\n{currentCount}/{ingredient.requiredCount}";
            if (statusIcon != null)
            {
                bool hasEnough = productionManager.HasEnoughIngredient(ingredient);
                statusIcon.sprite = hasEnough ? checkmarkSprite : crossSprite;
                statusIcon.color = hasEnough ? Color.green : Color.red;
            }
        }
    }
}
