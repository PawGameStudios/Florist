using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
namespace Florist.Merge
{
    public class RecipeCard : MonoBehaviour
    {
        [SerializeField] private Image recipeIcon;
        [SerializeField] private TextMeshProUGUI recipeName;
        [SerializeField] private Transform requirementsContainer;
        [SerializeField] private TextMeshProUGUI productionTime;
        [SerializeField] private Button produceButton;
        [SerializeField] private RequirementItem requirementItemPrefab;
        private RecipeData recipe;
        private readonly List<RequirementItem> requirementItems = new List<RequirementItem>();
        private ProductionKiosk kiosk;
        private RecipeModal modal;
        private ProductionManager productionManager;
        private InventoryManager inventoryManager;
        public void Initialize(RecipeData recipeData, ProductionKiosk kioskRef, RecipeModal modalRef, ProductionManager production, InventoryManager inventory)
        {
            Unsubscribe();
            recipe = recipeData;
            kiosk = kioskRef;
            modal = modalRef;
            productionManager = production;
            inventoryManager = inventory;
            if (isActiveAndEnabled) Subscribe();
            if (recipeIcon != null) recipeIcon.sprite = recipe.recipeIcon;
            if (recipeName != null) recipeName.text = recipe.recipeName;
            if (productionTime != null)
            {
                int seconds = Mathf.CeilToInt(recipe.productionTime);
                productionTime.text = seconds < 60 ? $"{seconds} sn" : seconds < 3600 ? $"{Mathf.CeilToInt(seconds / 60f)} dk" : $"{seconds / 3600f:0.#} saat";
            }
            foreach (var item in requirementItems) if (item != null) Destroy(item.gameObject);
            requirementItems.Clear();
            if (requirementItemPrefab != null && requirementsContainer != null)
                foreach (var ingredient in recipe.ingredients)
                {
                    var item = Instantiate(requirementItemPrefab, requirementsContainer);
                    item.Initialize(ingredient, productionManager, inventoryManager);
                    requirementItems.Add(item);
                }
            UpdateButtonState();
        }
        private void OnEnable()
        {
            if (produceButton != null) produceButton.onClick.AddListener(OnProduceClicked);
            Subscribe();
            UpdateButtonState();
        }
        private void OnDisable()
        {
            if (produceButton != null) produceButton.onClick.RemoveListener(OnProduceClicked);
            Unsubscribe();
        }
        private void Subscribe()
        {
            if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateButtonState;
            if (productionManager != null) productionManager.OnProductionStateChanged += UpdateButtonState;
        }
        private void Unsubscribe()
        {
            if (inventoryManager != null) inventoryManager.OnInventoryChanged -= UpdateButtonState;
            if (productionManager != null) productionManager.OnProductionStateChanged -= UpdateButtonState;
        }
        private void UpdateButtonState()
        {
            if (produceButton != null) produceButton.interactable = recipe != null && kiosk != null && productionManager != null && productionManager.CanStartProduction(recipe, kiosk.kioskId);
            foreach (var item in requirementItems) if (item != null) item.UpdateStatus();
        }
        private void OnProduceClicked()
        {
            if (recipe != null && kiosk != null && productionManager != null && productionManager.StartProduction(recipe, kiosk.kioskId))
                if (modal != null) modal.CloseModal();
        }
    }
}
