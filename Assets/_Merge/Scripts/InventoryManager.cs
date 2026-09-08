namespace Florist.Merge
{
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }
    [SerializeField, HideInInspector] private int maxInventorySize = 24;
    [SerializeField] private MergeEconomyConfig economyConfig;
    public int Capacity => economyConfig != null ? Mathf.Clamp(economyConfig.InventoryCapacity, 1, 24) : maxInventorySize;
    private readonly List<InventoryItem> inventory = new List<InventoryItem>();
    public event System.Action OnInventoryChanged;
    public event System.Action OnInventoryFull;
    private bool RejectFullInventory() { OnInventoryFull?.Invoke(); return false; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }

    public bool AddItem(ItemData data, int level, int count = 1, bool restoring = false)
    {
        if (data == null || data.Sprites == null || level < 1 || level > data.Sprites.Length || count <= 0) return false;
        var stack = inventory.FirstOrDefault(x => x.itemData == data && x.level == level);
        if (stack != null)
        {
            if ((long)stack.count + count > int.MaxValue) return false;
            stack.AddToStack(count);
        }
        else
        {
            if (!restoring && inventory.Count >= Capacity) return RejectFullInventory();
            inventory.Add(new InventoryItem(data, level, count));
        }
        NotifyInventoryChanged();
        return true;
    }
    public bool AddProductionItem(ProductionItemData data, int level, int count = 1, bool notify = true, bool restoring = false)
    {
        if (data == null || level < 1 || level > data.maxLevel || count <= 0) return false;
        var stack = inventory.FirstOrDefault(x => x.productionItemData == data && x.level == level);
        if (stack != null)
        {
            if ((long)stack.count + count > int.MaxValue) return false;
            stack.AddToStack(count);
        }
        else
        {
            if (!restoring && inventory.Count >= Capacity) return RejectFullInventory();
            inventory.Add(new InventoryItem(data, level, count));
        }
        if (notify) NotifyInventoryChanged();
        return true;
    }
    public bool HasProducts(List<OrderRequest> requests)
    {
        if (requests == null || requests.Count == 0 || requests.Any(x => x == null || x.item == null || x.count <= 0)) return false;
        return requests.GroupBy(x => x.item).All(group =>
            inventory.Where(x => x.productionItemData == group.Key).Sum(x => (long)x.count) >= group.Sum(x => (long)x.count));
    }
    public bool TryConsumeProducts(List<OrderRequest> requests)
    {
        if (!HasProducts(requests)) return false;
        foreach (var request in requests) TryRemoveProductionItem(request.item, request.count, false);
        return true; // Caller publishes once after the whole order is settled.
    }
    public bool TryTakeRegular(InventoryItem item)
    {
        if (item == null || !item.IsRegularItem || item.count <= 0 || !inventory.Contains(item)) return false;
        item.count--;
        if (item.count == 0) inventory.Remove(item);
        return true;
    }

    public bool HasProductionItem(ProductionItemData data, int count)
    {
        return data != null && count > 0 && inventory.Where(x => x.productionItemData == data).Sum(x => (long)x.count) >= count;
    }
    public bool TryRemoveProductionItem(ProductionItemData data, int count, bool notify = true)
    {
        if (!HasProductionItem(data, count)) return false;
        foreach (var stack in inventory.Where(x => x.productionItemData == data))
        {
            int take = System.Math.Min(stack.count, count);
            stack.count -= take;
            count -= take;
            if (count == 0) break;
        }
        inventory.RemoveAll(x => x.count <= 0);
        if (notify) NotifyInventoryChanged();
        return true;
    }
    // Reserve the highest minimum levels first so overlapping recipe entries cannot double-spend.
    private Dictionary<InventoryItem, int> PlanIngredients(List<RecipeIngredient> ingredients)
    {
        if (ingredients == null || ingredients.Count == 0 || ingredients.Any(x => x == null || x.itemData == null || x.requiredCount <= 0 || x.requiredLevel < 1)) return null;
        var remaining = inventory.ToDictionary(x => x, x => x.count);
        foreach (var ingredient in ingredients.OrderByDescending(x => x.requiredLevel))
        {
            int needed = ingredient.requiredCount;
            foreach (var stack in inventory.Where(x => x.itemData == ingredient.itemData && x.level >= ingredient.requiredLevel).OrderBy(x => x.level))
            {
                int take = System.Math.Min(remaining[stack], needed);
                remaining[stack] -= take;
                needed -= take;
                if (needed == 0) break;
            }
            if (needed > 0) return null;
        }
        return remaining;
    }
    public bool HasIngredients(List<RecipeIngredient> ingredients) => PlanIngredients(ingredients) != null;
    public bool TryConsumeIngredients(List<RecipeIngredient> ingredients, bool notify = true)
    {
        var remaining = PlanIngredients(ingredients);
        if (remaining == null) return false;
        foreach (var stack in inventory) stack.count = remaining[stack];
        inventory.RemoveAll(x => x.count <= 0);
        if (notify) NotifyInventoryChanged();
        return true;
    }
    public List<InventoryItem> GetInventory() => new List<InventoryItem>(inventory);
    public void ClearInventory() { inventory.Clear(); NotifyInventoryChanged(); }
    public void NotifyInventoryChanged() => OnInventoryChanged?.Invoke();
}
}
