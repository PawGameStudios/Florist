namespace Florist.Merge
{
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GameManager gameManager;
    [SerializeField] private ProductionManager productionManager;
    [SerializeField] private List<CustomerData> availableCustomers;
    [SerializeField, HideInInspector] private int maxActiveOrders = 3;
    [SerializeField] private MergeEconomyConfig economyConfig;
    [SerializeField] private MergeProgressionConfig progressionConfig;
    public int CompletedOrders { get; private set; }
    public event Action OnProgressChanged;
    private int OrderLimit => economyConfig != null ? Mathf.Max(1, economyConfig.MaxActiveOrders) : maxActiveOrders;
    [SerializeField, HideInInspector] private float orderGenerationInterval = 30f;
    private readonly List<Order> activeOrders = new List<Order>();
    private const string SaveKey = "Florist.Merge.Orders";
    private bool initialized;
    private bool restored;
    private Coroutine generationRoutine;
    public event Action OnOrdersChanged;

    [Serializable] private class SavedOrders { public List<SavedOrder> orders = new List<SavedOrder>(); }
    [Serializable] private class SavedOrder
    {
        public List<SavedRequest> requests;
        public string customerName;
        public string itemName;
        public int count;
        public int reward;
        public string orderTime;
    }
    [Serializable] private class SavedRequest { public string itemName; public int count; }
    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        // Existing saves already had mature-pot requests. Keep their recipes reachable during migration.
        int initialProgress = !PlayerPrefs.HasKey("Florist.Merge.CompletedOrders") && PlayerPrefs.HasKey(SaveKey) ? 4 : 0;
        CompletedOrders = Mathf.Max(0, PlayerPrefs.GetInt("Florist.Merge.CompletedOrders", initialProgress));
        PlayerPrefs.SetInt("Florist.Merge.CompletedOrders", CompletedOrders);
    }
    private void OnDestroy() { if (Instance == this) Instance = null; }
    private void OnDisable() { StopAllCoroutines(); generationRoutine = null; }
    private void OnEnable() { if (initialized) StartGenerationTimer(); }
    public void Initialize()
    {
        if (initialized) return;
        initialized = true;
        if (!restored)
            for (int i = 0; i < Mathf.Min(economyConfig != null ? Mathf.Max(0, economyConfig.InitialOrders) : 2, OrderLimit); i++) TryGenerateNewOrder();
        StartGenerationTimer();
        OnOrdersChanged?.Invoke();
    }
    private void StartGenerationTimer()
    {
        if (isActiveAndEnabled && generationRoutine == null) generationRoutine = StartCoroutine(GeneratePeriodically());
    }
    private IEnumerator GeneratePeriodically()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(Mathf.Max(1f, economyConfig != null ? economyConfig.OrderGenerationSeconds : orderGenerationInterval));
            TryGenerateNewOrder();
        }
    }
    private void TryGenerateNewOrder()
    {
        if (activeOrders.Count >= OrderLimit || availableCustomers == null || availableCustomers.Count == 0) return;
        var eligible = availableCustomers.Where(x => x != null && x.minimumCompletedOrders <= CompletedOrders &&
            x.preferredItems != null && x.preferredItems.Any(IsRequestable)).ToList();
        if (eligible.Count == 0) return;
        int totalWeight = eligible.Sum(x => Mathf.Max(1, x.selectionWeight));
        int roll = UnityEngine.Random.Range(0, totalWeight);
        var customer = eligible[0];
        foreach (var candidate in eligible)
        {
            roll -= Mathf.Max(1, candidate.selectionWeight);
            if (roll < 0) { customer = candidate; break; }
        }
        var pool = customer.preferredItems.Where(IsRequestable).Distinct().ToList();
        var stage = progressionConfig != null ? progressionConfig.GetStage(CompletedOrders) : new MergeOrderStage();
        int typeCount = Mathf.Min(pool.Count, Mathf.Min(Mathf.Clamp(customer.maxProductTypes, 1, 2), Mathf.Clamp(stage.MaxProductTypes, 1, 2)));
        int budget = Mathf.Clamp(stage.MaxTotalProducts, 1, 4);
        var requests = new List<OrderRequest>();
        for (int i = 0; i < typeCount && budget > 0; i++)
        {
            var item = pool[UnityEngine.Random.Range(0, pool.Count)];
            if (i == 0 && progressionConfig != null && UnityEngine.Random.value < progressionConfig.LatestProductChance)
                {
                int latest = pool.Max(x => x.minimumCompletedOrders);
                var newest = pool.Where(x => x.minimumCompletedOrders == latest).ToList();
                item = newest[UnityEngine.Random.Range(0, newest.Count)];
            }
            int maximum = Mathf.Min(Mathf.Clamp(stage.MaxCountPerProduct, 1, 3), budget - (typeCount - i - 1));
            int count = UnityEngine.Random.Range(1, Mathf.Max(1, maximum) + 1);
            requests.Add(new OrderRequest { item = item, count = count });
            pool.Remove(item);
            budget -= count;
        }
        activeOrders.Add(new Order(customer, requests, economyConfig != null ? economyConfig.GetRewardMultiplier(customer.customerType) : 1));
        OnOrdersChanged?.Invoke();
    }
    private bool IsRequestable(ProductionItemData item) => item != null && item.isUnlocked &&
        item.minimumCompletedOrders <= CompletedOrders && productionManager != null && productionManager.CanProduceItem(item);

    public void CompleteOrder(Order order)
    {
        var host = global::SaveSystem.Inst;
        if (order == null || order.isCompleted || !activeOrders.Contains(order) || inventoryManager == null ||
            gameManager == null || host == null || host.GeneralData == null || host.SaveData == null || host.ShopData == null ||
            !inventoryManager.TryConsumeProducts(order.GetRequests())) return;

        // Close the order before publishing any events so repeated clicks cannot award twice.
        order.isCompleted = true;
        activeOrders.Remove(order);
        CompletedOrders = Math.Min(int.MaxValue - 1, CompletedOrders) + 1;
        PlayerPrefs.SetInt("Florist.Merge.CompletedOrders", CompletedOrders);
        OnProgressChanged?.Invoke();
        host.GeneralData.ChangeMoney((long)Mathf.Max(0, order.reward));
        inventoryManager.NotifyInventoryChanged();
        OnOrdersChanged?.Invoke();
        gameManager.SaveGameData();
        host.Save();
    }
    public List<Order> GetActiveOrders() => new List<Order>(activeOrders);
    public bool HasActiveOrders() => activeOrders.Count > 0;
    public void ForceGenerateOrder() => TryGenerateNewOrder();
    public void SaveOrders()
    {
        var saved = new SavedOrders();
        foreach (var order in activeOrders)
        {
            if (order.isCompleted || order.customer == null || order.requestedItem == null) continue;
            saved.orders.Add(new SavedOrder { requests = order.GetRequests().Select(x => new SavedRequest { itemName = x.item.itemName, count = x.count }).ToList(), customerName = order.customer.name, itemName = order.requestedItem.itemName,
                count = order.requestedCount, reward = order.reward, orderTime = order.orderTime.ToString("O") });
        }
        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(saved));
    }
    public void LoadOrders()
    {
        activeOrders.Clear();
        restored = PlayerPrefs.HasKey(SaveKey);
        if (!restored) return;
        try
        {
            var saved = JsonUtility.FromJson<SavedOrders>(PlayerPrefs.GetString(SaveKey));
            if (saved?.orders == null) return;
            foreach (var record in saved.orders)
            {
                if (record == null || record.count <= 0) continue;
                var customer = availableCustomers?.FirstOrDefault(x => x != null && x.name == record.customerName);
                var item = productionManager != null ? productionManager.GetProductionItemByName(record.itemName) : null;
                if (customer == null || item == null) continue;
                var requests = new List<OrderRequest>();
                if (record.requests != null && record.requests.Count > 0)
                {
                    foreach (var request in record.requests)
                    {
                        var requested = productionManager.GetProductionItemByName(request.itemName);
                        if (requested == null || request.count <= 0) { requests.Clear(); break; }
                        requests.Add(new OrderRequest { item = requested, count = request.count });
                    }
                    if (requests.Count == 0) continue;
                }
                else requests.Add(new OrderRequest { item = item, count = record.count });
                var order = new Order(customer, requests) { reward = Mathf.Max(0, record.reward) };
                if (DateTime.TryParse(record.orderTime, System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.RoundtripKind, out var time)) order.orderTime = time;
                activeOrders.Add(order);
            }
        }
        catch (ArgumentException ex) { Debug.LogWarning("Merge orders could not be loaded: " + ex.Message); }
        OnOrdersChanged?.Invoke();
    }
    public void ClearSavedOrders()
    {
        PlayerPrefs.DeleteKey(SaveKey);
        activeOrders.Clear();
        restored = false;
        OnOrdersChanged?.Invoke();
    }
}
}

