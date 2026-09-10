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
        [SerializeField] private Image backgroundImage;
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
            if (requirementText != null) requirementText.text = $"{currentCount}/{ingredient.requiredCount}";
            bool hasEnough = productionManager.HasEnoughIngredient(ingredient);
            if (backgroundImage != null) backgroundImage.color = hasEnough ? new Color(.78f, .93f, .47f) : new Color(1f, .89f, .85f);
            if (statusIcon != null)
            {
                statusIcon.sprite = hasEnough ? checkmarkSprite : crossSprite;
                statusIcon.enabled = statusIcon.sprite != null;
                statusIcon.color = Color.white;
            }
        }
    }
}
