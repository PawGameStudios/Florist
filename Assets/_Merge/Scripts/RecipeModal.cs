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
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        private ProductionKiosk kiosk;
        private ProductionManager productionManager;
        private InventoryManager inventoryManager;
        private int page;
        [SerializeField] private CanvasGroup panelGroup;
        private MergePanelMotion motion;
        private bool closing;
        private void OnEnable()
        {
            if (motion == null) motion = new MergePanelMotion(transform, panelGroup);
            closing = false;
            motion.Show();
            if (closeButton != null) closeButton.onClick.AddListener(CloseModal);
            if (previousButton != null) previousButton.onClick.AddListener(Previous);
            if (nextButton != null) nextButton.onClick.AddListener(Next);
        }
        private void OnDisable()
        {
            motion?.Reset();
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseModal);
            if (previousButton != null) previousButton.onClick.RemoveListener(Previous);
            if (nextButton != null) nextButton.onClick.RemoveListener(Next);
        }
        private void Previous() { page = Mathf.Max(0, page - 1); Rebuild(); }
        private void Next() { page++; Rebuild(); }
        public void Initialize(ProductionKiosk kioskRef, ProductionManager production, InventoryManager inventory)
        {
            kiosk = kioskRef;
            productionManager = production;
            inventoryManager = inventory;
            page = 0;
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
            int maxPage = Mathf.Max(0, (recipes.Count - 1) / 3);
            page = Mathf.Clamp(page, 0, maxPage);
            if (previousButton != null) previousButton.interactable = page > 0;
            if (nextButton != null) nextButton.interactable = page < maxPage;
            for (int i = page * 3; i < Mathf.Min(recipes.Count, page * 3 + 3); i++)
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
