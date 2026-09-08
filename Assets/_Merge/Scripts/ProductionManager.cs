namespace Florist.Merge
{
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ProductionManager : MonoBehaviour
{
    public static ProductionManager Instance { get; private set; }
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private OrderManager orderManager;
    [SerializeField] private List<RecipeData> availableRecipes;
    [SerializeField] private List<ProductionItemData> availableProductionItems;
    public List<ProductionJob> activeProductions = new List<ProductionJob>();
    public event System.Action OnProductionChanged;
    public event System.Action OnProductionStateChanged;
    public event System.Action<string> OnKioskProductionChanged;
    private Coroutine progressRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void OnDisable() { StopAllCoroutines(); progressRoutine = null; }
    private void OnEnable() { EnsureProgressTimer(); }
    public void Initialize() { EnsureProgressTimer(); }
    private void EnsureProgressTimer()
    {
        if (isActiveAndEnabled && progressRoutine == null && activeProductions.Any(x => !x.IsCompleted))
            progressRoutine = StartCoroutine(ProgressTimer());
    }
    private IEnumerator ProgressTimer()
    {
        while (activeProductions.Any(x => !x.IsCompleted))
        {
            yield return new WaitForSecondsRealtime(1f);
            OnProductionChanged?.Invoke();
        }
        progressRoutine = null;
    }
    public bool IsRecipeUnlocked(RecipeData recipe) => recipe != null && recipe.isUnlocked && recipe.resultItem != null &&
        recipe.resultItem.isUnlocked && recipe.resultItem.minimumCompletedOrders <= (orderManager != null ? orderManager.CompletedOrders : 0);
    public bool CanProduceItem(ProductionItemData item) => availableRecipes != null &&
        availableRecipes.Any(recipe => IsRecipeUnlocked(recipe) && recipe.resultItem == item);

    public bool CanStartProduction(RecipeData recipe, string kioskId)
    {
        return IsRecipeUnlocked(recipe) && recipe.resultCount > 0 &&
            recipe.resultLevel > 0 && !string.IsNullOrEmpty(kioskId) && inventoryManager != null &&
            !HasActiveProductionInKiosk(kioskId) && inventoryManager.HasIngredients(recipe.ingredients);
    }
    public bool HasEnoughIngredient(RecipeIngredient ingredient)
    {
        return inventoryManager != null && inventoryManager.HasIngredients(new List<RecipeIngredient> { ingredient });
    }
    public bool StartProduction(RecipeData recipe, string kioskId)
    {
        if (!CanStartProduction(recipe, kioskId) || !inventoryManager.TryConsumeIngredients(recipe.ingredients, false)) return false;
        activeProductions.Add(new ProductionJob(recipe, kioskId));
        inventoryManager.NotifyInventoryChanged();
        NotifyChanged(kioskId);
        return true;
    }
    private void NotifyChanged(string kioskId)
    {
        OnProductionStateChanged?.Invoke();
        OnProductionChanged?.Invoke();
        OnKioskProductionChanged?.Invoke(kioskId);
        EnsureProgressTimer();
    }
    public void CompleteProduction(ProductionJob job)
    {
        if (job == null || job.recipe == null || !activeProductions.Contains(job) || !job.IsReadyToCollect || inventoryManager == null) return;
        if (!inventoryManager.AddProductionItem(job.recipe.resultItem, job.recipe.resultLevel, job.recipe.resultCount, false)) return;
        job.isCollected = true;
        activeProductions.Remove(job);
        inventoryManager.NotifyInventoryChanged();
        NotifyChanged(job.kioskId);
    }
    public void CollectProduction(ProductionJob job) => CompleteProduction(job);
    public List<ProductionJob> GetActiveProductions() => new List<ProductionJob>(activeProductions);
    public List<RecipeData> GetAvailableRecipes() => availableRecipes == null ? new List<RecipeData>() : availableRecipes.Where(IsRecipeUnlocked).ToList();
    public RecipeData GetRecipeByName(string recipeName) => availableRecipes?.FirstOrDefault(x => x != null && x.recipeName == recipeName);
    public bool HasActiveProduction() => activeProductions.Count > 0;
    public ProductionJob GetCurrentProduction() => activeProductions.Count > 0 ? activeProductions[0] : null;
    public void AddProductionJob(ProductionJob job)
    {
        if (job == null || job.recipe == null || job.isCollected || HasActiveProductionInKiosk(job.kioskId)) return;
        activeProductions.Add(job);
        NotifyChanged(job.kioskId);
    }
    public ProductionJob GetProductionInKiosk(string kioskId) => activeProductions.FirstOrDefault(x => x.kioskId == kioskId);
    public bool HasActiveProductionInKiosk(string kioskId) => activeProductions.Any(x => x.kioskId == kioskId);
    public ProductionItemData GetProductionItemByName(string itemName) => availableProductionItems?.FirstOrDefault(x => x != null && x.itemName == itemName);
}
}

