using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
namespace Florist.Merge
{
    public class RecipeModal : MonoBehaviour
    {
        [SerializeField] private Transform recipeContainer;
        [SerializeField] private RecipeCard recipeCardPrefab;
        [SerializeField] private Button closeButton;
        private readonly List<RecipeCard> recipeCards = new List<RecipeCard>();
        private ProductionKiosk kiosk;
        private ProductionManager productionManager;
        private InventoryManager inventoryManager;
        [SerializeField] private CanvasGroup panelGroup;
        private MergePanelMotion motion;
        private bool closing;
        private void OnEnable()
        {
            if (motion == null) motion = new MergePanelMotion(transform, panelGroup);
            closing = false;
            motion.Show();
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
        }
        private void OnDisable()
        {
            motion?.Reset();
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
        }
        public void Initialize(ProductionKiosk kioskRef, ProductionManager production, InventoryManager inventory)
        {
            kiosk = kioskRef;
            productionManager = production;
            inventoryManager = inventory;
            Rebuild();
        }
        private void Rebuild()
        {
            var production = productionManager;
            var inventory = inventoryManager;
            foreach (var card in recipeCards)
            {
                if (card == null) continue;
                card.gameObject.SetActive(false);
                Destroy(card.gameObject);
            }
            recipeCards.Clear();
            if (production == null || recipeCardPrefab == null || recipeContainer == null) return;
            var recipes = production.GetAvailableRecipes();
            for (int i = 0; i < recipes.Count; i++)
            {
                var card = Instantiate(recipeCardPrefab, recipeContainer);
                card.Initialize(recipes[i], kiosk, this, production, inventory);
                recipeCards.Add(card);
            }
        }
        public void CloseModal()
        {
            if (closing || kiosk == null) return;
            closing = true;
            motion.Hide(() => { if (kiosk != null) kiosk.CloseModal(); });
        }
    }
}
