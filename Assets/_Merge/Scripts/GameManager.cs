namespace Florist.Merge
{
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System.Linq;
using System;
using System.Collections;
using System.Globalization;
using Random = UnityEngine.Random;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private GridManager m_GridManager;
    [SerializeField] private MergeEconomyConfig m_EconomyConfig;
    public Color MaxLevelCellColor => m_EconomyConfig != null ? m_EconomyConfig.MaxLevelCellColor : Color.clear;
    public Color MergeTargetCellColor => m_EconomyConfig != null ? m_EconomyConfig.MergeTargetCellColor : Color.clear;
    [SerializeField] private InventoryManager m_InventoryManager;
    [SerializeField] private InventoryWindow m_InventoryWindow;
    [SerializeField] private ProductionManager m_ProductionManager;
    [SerializeField] private OrderManager m_OrderManager;
    [SerializeField] private RectTransform m_DragRoot;
    [SerializeField] private TextMeshProUGUI m_EnergyText;
    [SerializeField] private List<ItemData> m_ItemDatas;
    [SerializeField] private Image m_DragImage;
    [SerializeField] private TextMeshProUGUI m_EnergyTimerText;
    [SerializeField] private Image m_FeedbackImagePrefab;
    [SerializeField] private RectTransform m_InventoryFeedbackTarget;
    [SerializeField] private TextMeshProUGUI m_FeedbackText;
    private readonly List<Image> m_FlyingImages = new List<Image>();
    private readonly List<Tween> m_FlightTweens = new List<Tween>();
    private Tween m_DragTween, m_NoticeTween, m_EnergyTween, m_InventoryTween;
    private Vector3 m_DragScale, m_EnergyScale, m_InventoryScale;
    private Cell m_HoverCell;

    private int m_Energy;
    public int Energy => m_Energy;
    public int MaxEnergy => c_MaxEnergy;
    public bool IsDragging => m_DragSourceCell != null;
    public event Action OnEnergyChanged;
    private int c_MaxEnergy => Mathf.Max(1, m_EconomyConfig.MaxEnergy);
    private Cell m_DragSourceCell;
    private int m_DragPointerId;

    private float m_EnergyTimer = 0f;
    private float c_EnergyRechargeTime => Mathf.Max(1f, m_EconomyConfig.EnergyRechargeSeconds);

    private const string SavePrefix = "Florist.Merge.";
    private bool m_Initialized;
    private bool m_SaveQueued;
    private bool m_Collecting;
    private DateTime m_LastEnergyTick;

    private void Awake()
    {
        Instance = this;
        m_DragImage.enabled = false;
        m_DragImage.raycastTarget = false;
        m_DragScale = m_DragImage.transform.localScale;
        m_EnergyScale = m_EnergyText.transform.localScale;
        if (m_InventoryFeedbackTarget != null) m_InventoryScale = m_InventoryFeedbackTarget.localScale;
        if (m_FeedbackText != null) m_FeedbackText.gameObject.SetActive(false);
    }

    private void Start()
    {
        m_GridManager.Initialize(this);
        m_ProductionManager.Initialize();
        LoadGameData();
        LoadGridData();
        m_OrderManager.LoadOrders();
        m_OrderManager.Initialize();
        m_LastEnergyTick = DateTime.UtcNow;
        m_Initialized = true;
        m_InventoryManager.OnInventoryChanged += RequestSave;
        m_InventoryManager.OnInventoryFull += ShowInventoryFullWarning;
        m_ProductionManager.OnProductionStateChanged += RequestSave;
        m_OrderManager.OnOrdersChanged += RequestSave;
        UpdateEnergyUI();
        StartCoroutine(EnergyClock());
        SaveGameData();
    }

    private IEnumerator EnergyClock()
    {
        var interval = new WaitForSecondsRealtime(1f);
        while (true)
        {
            RefreshEnergy();
            yield return interval;
        }
    }

    private void RefreshEnergy()
    {
        DateTime now = DateTime.UtcNow;
        double elapsed = Math.Max(0d, (now - m_LastEnergyTick).TotalSeconds);
        m_LastEnergyTick = now;
        int previousEnergy = m_Energy;
        ApplyEnergyElapsed(elapsed);
        UpdateEnergyUI();
        if (previousEnergy != m_Energy) RequestSave();
    }

    private void ApplyEnergyElapsed(double elapsed)
    {
        if (m_Energy >= c_MaxEnergy)
        {
            m_EnergyTimer = 0f;
            return;
        }
        double total = m_EnergyTimer + elapsed;
        double recharges = Math.Floor(total / c_EnergyRechargeTime);
        m_Energy += (int)Math.Min(c_MaxEnergy - m_Energy, recharges);
        m_EnergyTimer = m_Energy >= c_MaxEnergy ? 0f : (float)(total % c_EnergyRechargeTime);
    }

    private void RequestSave()
    {
        if (!m_Initialized || m_SaveQueued || !isActiveAndEnabled) return;
        m_SaveQueued = true;
        StartCoroutine(SaveAfterTransaction());
    }

    private IEnumerator SaveAfterTransaction()
    {
        yield return null;
        m_SaveQueued = false;
        SaveGameData();
    }

    private void OnDisable()
    {
        ClearFeedback();
        SaveGameData();
        m_SaveQueued = false;
    }

    private void OnDestroy()
    {
        if (m_InventoryManager != null)
        {
            m_InventoryManager.OnInventoryChanged -= RequestSave;
            m_InventoryManager.OnInventoryFull -= ShowInventoryFullWarning;
        }
        if (m_ProductionManager != null) m_ProductionManager.OnProductionStateChanged -= RequestSave;
        if (m_OrderManager != null) m_OrderManager.OnOrdersChanged -= RequestSave;
        if (Instance == this) Instance = null;
    }

    private void UpdateEnergyUI()
    {
        m_EnergyText.text = $"Enerji: {m_Energy}/{c_MaxEnergy}";
        OnEnergyChanged?.Invoke();
        int remaining = Mathf.CeilToInt(c_EnergyRechargeTime - m_EnergyTimer);
        m_EnergyTimerText.text = m_Energy >= c_MaxEnergy ? "" : $"{remaining / 60:00}:{remaining % 60:00}";
    }

    public void GrantRewardedEnergy(int amount)
    {
        RefreshEnergy();
        m_Energy = (int)Math.Min(c_MaxEnergy, (long)m_Energy + Math.Max(0, amount));
        UpdateEnergyUI();
        SaveGameData();
    }

    private void FillGridOnStart()
    {
        var allCells = new List<Cell>(m_GridManager.GetAllCells());
        var rnd = new System.Random();
        foreach (var itemData in m_ItemDatas)
        {
            if (itemData == null || itemData.Sprites == null || itemData.Sprites.Length == 0 || itemData.ProducerSprite == null) continue;
            if (allCells.Count == 0) break;
            int idx = rnd.Next(allCells.Count);
            allCells[idx].SetProducer(itemData);
            allCells.RemoveAt(idx);
        }
        AddFixedItems();
    }

    private void AddFixedItems()
    {
        var allCells = new List<Cell>(m_GridManager.GetAllCells());
        var emptyCells = allCells.Where(cell => cell.IsEmpty).ToList();

        if (emptyCells.Count < 5) return;

        int fixedItemCount = Random.Range(4, 6);
        var selectedCells = emptyCells.OrderBy(x => Random.value).Take(fixedItemCount).ToList();

        foreach (var cell in selectedCells)
        {
            var availableItems = m_ItemDatas.Where(item => item != null && item.Sprites != null && item.Sprites.Length > 0).ToArray();
            if (availableItems.Length == 0) return;
            var itemData = availableItems[Random.Range(0, availableItems.Length)];
            int level = Random.Range(1, Mathf.Min(2, itemData.Sprites.Length) + 1);
            cell.SetFixedItem(itemData, level);
        }
    }



    public void OnCellBeginDrag(Cell cell, PointerEventData eventData)
    {
        if (!m_Initialized || m_DragSourceCell != null || cell.IsEmpty || cell.IsFixed()) return;
        cell.ResetMotion();
        m_DragTween?.Kill();
        m_DragImage.transform.localScale = m_DragScale;
        m_DragSourceCell = cell;
        m_DragPointerId = eventData.pointerId;

        if (cell.IsProducer())
        {
            if (cell.GetItemData().ProducerSprite != null)
                m_DragImage.sprite = cell.GetItemData().ProducerSprite;
            else
            {
                CancelDrag(cell);
                return;
            }
        }
        else
            m_DragImage.sprite = cell.GetItemData().Sprites[cell.GetLevel() - 1];

        m_DragImage.rectTransform.sizeDelta = cell.RectTransform.rect.size;
        m_DragImage.enabled = true;
        m_DragImage.transform.SetAsLastSibling();
        m_DragTween = m_DragImage.transform.DOScale(m_DragScale * 1.04f, .08f).SetUpdate(true);
        cell.SetItemImageAlpha(0.3f);
        OnCellDrag(eventData);
    }

    public void OnCellDrag(PointerEventData eventData)
    {
        if (m_DragSourceCell == null || eventData.pointerId != m_DragPointerId || !m_DragImage.enabled) return;
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            m_DragRoot,
            eventData.position, eventData.pressEventCamera, out pos);
        m_DragImage.rectTransform.anchoredPosition = pos;
    }

    public void OnCellEndDrag(Cell sourceCell, PointerEventData eventData)
    {
        if (eventData.pointerId != m_DragPointerId || m_DragSourceCell != sourceCell) return;
        Vector3 origin = m_DragImage.rectTransform.position;
        CancelDrag(sourceCell);
        sourceCell.PlayArrival(origin);
    }

    public void CancelDrag(Cell sourceCell)
    {
        if (m_DragSourceCell == null || sourceCell != m_DragSourceCell) return;
        m_DragSourceCell.SetItemImageAlpha(1f);
        m_DragSourceCell = null;
        ClearHover();
        m_DragTween?.Kill();
        m_DragImage.transform.localScale = m_DragScale;
        if (m_DragImage != null) m_DragImage.enabled = false;
    }

    public void OnCellDrop(Cell targetCell, PointerEventData eventData)
    {
        if (m_DragSourceCell == null || eventData.pointerId != m_DragPointerId || targetCell == m_DragSourceCell) return;
        Cell source = m_DragSourceCell;
        Vector3 origin = m_DragImage.rectTransform.position;
        CancelDrag(source);
        if (!TryMoveOrMerge(source, targetCell, origin))
        {
            source.PlayArrival(origin);
            targetCell.PlayRejected();
        }
        SaveGameData();
    }

    public void PrepareForExit()
    {
        SaveGameData();
        ClearFeedback();
        m_Initialized = false;
        m_SaveQueued = false;
        StopAllCoroutines();
    }

    private bool TryMoveOrMerge(Cell from, Cell to, Vector3 origin)
    {
        if (from.IsEmpty || from.IsFixed()) return false;
        if (!to.IsEmpty && !from.IsProducer() && !to.IsProducer() &&
            from.GetItemData() == to.GetItemData() && from.GetLevel() == to.GetLevel())
        {
            int newLevel = to.GetLevel() + 1;
            if (newLevel <= to.GetItemData().Sprites.Length)
            {
                bool unlocked = to.IsFixed();
                to.SetItem(to.GetItemData(), newLevel);
                from.Clear();
                to.PlayArrival(origin, true, unlocked);
                return true;
            }
            else
            {
                from.SetItemImageAlpha(1f);
            }
            return false;
        }

        if (from.IsFixed() || to.IsFixed())
        {
            return false;
        }

        if (!to.IsEmpty && !from.IsEmpty)
        {
            to.ResetMotion();
            Vector3 otherOrigin = to.ItemWorldPosition;
            var fromData = from.GetItemData();
            var fromLevel = from.GetLevel();
            var fromIsProducer = from.IsProducer();

            var toData = to.GetItemData();
            var toLevel = to.GetLevel();
            var toIsProducer = to.IsProducer();

            if (fromIsProducer)
                to.SetProducer(fromData);
            else
                to.SetItem(fromData, fromLevel);

            if (toIsProducer)
                from.SetProducer(toData);
            else
                from.SetItem(toData, toLevel);

            to.PlayArrival(origin);
            from.PlayArrival(otherOrigin);
            return true;
        }

        if (!to.IsEmpty)
        {
            return false;
        }
        if (from.IsProducer())
            to.SetProducer(from.GetItemData());
        else
            to.SetItem(from.GetItemData(), from.GetLevel());
        from.Clear();
        to.PlayArrival(origin);
        return true;
    }

    public void OnProducerClicked(Cell producerCell)
    {
        if (!m_Initialized || IsDragging) return;
        RefreshEnergy();
        if (!producerCell.IsProducer()) return;
        if (m_Energy < Mathf.Max(0, m_EconomyConfig.ProducerEnergyCost))
        {
            ShowNotice("Enerji yetersiz");
            m_EnergyTween?.Kill();
            m_EnergyText.transform.localScale = m_EnergyScale;
            m_EnergyTween = m_EnergyText.transform.DOPunchScale(m_EnergyScale * .06f, .20f, 1, .1f).SetUpdate(true);
            return;
        }
        ItemData itemData = producerCell.GetItemData();
        var (x, y) = producerCell.GetPosition();
        var closestEmpties = m_GridManager.GetClosestEmptyCells(x, y);

        var availableCells = closestEmpties.Where(cell => !cell.IsFixed()).ToList();

        if (availableCells.Count == 0)
        {
            ShowNotice("Tahtada boş yer yok");
            return;
        }

        Cell targetCell = availableCells[Random.Range(0, availableCells.Count)];
        int level = m_EconomyConfig.ChooseSpawnLevel(itemData);
        targetCell.SetItem(itemData, level);
        targetCell.PlaySpawn(producerCell.ItemWorldPosition, m_DragRoot);
        m_Energy -= Mathf.Max(0, m_EconomyConfig.ProducerEnergyCost);
        UpdateEnergyUI();
        SaveGameData();
    }

    public enum InventoryReturnResult { Success, BoardFull, NotReady, InvalidItem, ItemUnavailable }

    public bool TryReturnInventoryItem(InventoryItem item) => ReturnInventoryItem(item) == InventoryReturnResult.Success;

    public InventoryReturnResult ReturnInventoryItem(InventoryItem item)
    {
        if (!m_Initialized || IsDragging) return InventoryReturnResult.NotReady;
        if (item == null || !item.IsRegularItem || item.itemData.Sprites == null ||
            item.level < 1 || item.level > item.itemData.Sprites.Length) return InventoryReturnResult.InvalidItem;
        Cell target = m_GridManager.GetAllCells().FirstOrDefault(x => x.IsEmpty && !x.IsFixed());
        if (target == null) return InventoryReturnResult.BoardFull;
        if (!m_InventoryManager.TryTakeRegular(item)) return InventoryReturnResult.ItemUnavailable;
        target.SetItem(item.itemData, item.level);
        m_InventoryManager.NotifyInventoryChanged();
        SaveGameData();
        return InventoryReturnResult.Success;
    }

    public void OnCollectClicked(Cell cell)
    {
        if (cell != null) TryCollectItem(cell, cell.ItemWorldPosition, m_InventoryFeedbackTarget);
    }

    public void OnInventoryDrop(PointerEventData eventData, RectTransform destination)
    {
        if (m_DragSourceCell == null || eventData.pointerId != m_DragPointerId) return;
        Cell source = m_DragSourceCell;
        Vector3 origin = m_DragImage.rectTransform.position;
        CancelDrag(source);
        if (!TryCollectItem(source, origin, destination)) source.PlayArrival(origin);
    }

    private bool TryCollectItem(Cell cell, Vector3 origin, RectTransform destination)
    {
        if (!m_Initialized || IsDragging || m_Collecting || !cell.IsCollectable()) return false;
        m_Collecting = true;

        ItemData itemData = cell.GetItemData();
        int level = cell.GetLevel();

        bool added = m_InventoryManager.AddItem(itemData, level);

        if (added)
        {
            FlyToInventory(cell, origin, destination);
            cell.Clear();
            SaveGameData();
        }
        m_Collecting = false;
        return added;
    }

    private bool CanMerge(Cell from, Cell to)
    {
        return from != null && to != null && from != to && !from.IsEmpty && !to.IsEmpty &&
            !from.IsFixed() && !from.IsProducer() && !to.IsProducer() &&
            from.GetItemData() == to.GetItemData() && from.GetLevel() == to.GetLevel() &&
            to.GetLevel() < to.GetItemData().Sprites.Length;
    }

    public void OnCellHover(Cell cell, PointerEventData eventData)
    {
        if (m_DragSourceCell == null || eventData.pointerId != m_DragPointerId) return;
        ClearHover();
        if (!CanMerge(m_DragSourceCell, cell)) return;
        m_HoverCell = cell;
        cell.SetMergeHighlight(true);
    }

    public void OnCellHoverExit(Cell cell, PointerEventData eventData)
    {
        if (eventData.pointerId == m_DragPointerId && cell == m_HoverCell) ClearHover();
    }

    private void ClearHover()
    {
        if (m_HoverCell != null) m_HoverCell.SetMergeHighlight(false);
        m_HoverCell = null;
    }

    private void ShowNotice(string message)
    {
        if (m_FeedbackText == null) return;
        m_NoticeTween?.Kill();
        m_FeedbackText.gameObject.SetActive(true);
        m_FeedbackText.text = message;
        m_FeedbackText.alpha = 1f;
        m_NoticeTween = m_FeedbackText.DOFade(0f, .18f).SetDelay(1.1f).SetUpdate(true)
            .OnComplete(() => m_FeedbackText.gameObject.SetActive(false));
    }

    private void FlyToInventory(Cell cell, Vector3 origin, RectTransform destination)
    {
        if (m_FeedbackImagePrefab == null || destination == null) return;
        var visual = Instantiate(m_FeedbackImagePrefab, m_DragRoot);
        visual.sprite = cell.ItemSprite;
        visual.rectTransform.sizeDelta = cell.RectTransform.rect.size;
        visual.rectTransform.position = origin;
        m_FlyingImages.Add(visual);
        var flight = DOTween.Sequence().SetUpdate(true);
        m_FlightTweens.Add(flight);
        flight.Append(visual.rectTransform.DOMove(destination.position, .22f).SetEase(Ease.InOutSine));
        flight.Join(visual.transform.DOScale(.35f, .22f));
        flight.Join(visual.DOFade(.3f, .22f));
        flight.OnComplete(() =>
        {
            m_FlightTweens.Remove(flight);
            m_FlyingImages.Remove(visual);
            if (visual != null) Destroy(visual.gameObject);
            m_InventoryTween?.Kill();
            m_InventoryFeedbackTarget.localScale = m_InventoryScale;
            m_InventoryTween = m_InventoryFeedbackTarget.DOPunchScale(m_InventoryScale * .05f, .16f, 1, .1f).SetUpdate(true);
        });
    }

    public void ClearFeedback()
    {
        CancelDrag(m_DragSourceCell);
        ClearHover();
        m_DragTween?.Kill();
        m_NoticeTween?.Kill();
        m_EnergyTween?.Kill();
        m_InventoryTween?.Kill();
        if (m_EnergyText != null) m_EnergyText.transform.localScale = m_EnergyScale;
        if (m_InventoryFeedbackTarget != null) m_InventoryFeedbackTarget.localScale = m_InventoryScale;
        if (m_FeedbackText != null) m_FeedbackText.gameObject.SetActive(false);
        foreach (var tween in m_FlightTweens) tween.Kill();
        m_FlightTweens.Clear();
        foreach (var visual in m_FlyingImages) if (visual != null) Destroy(visual.gameObject);
        m_FlyingImages.Clear();
    }

    private void ShowInventoryFullWarning()
    {
        if (m_InventoryWindow != null) m_InventoryWindow.ShowFullInventory();
    }


    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveGameData();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            SaveGameData();
    }

    public void SaveGameData()
    {
        if (!m_Initialized) return;
        RefreshEnergy();
        PlayerPrefs.SetInt(SavePrefix + "Energy", m_Energy);
        PlayerPrefs.SetFloat(SavePrefix + "EnergyTimer", m_EnergyTimer);
        PlayerPrefs.SetString(SavePrefix + "LastSaveTime", DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture));

        SaveGridData();
        SaveInventoryData();
        SaveProductionData();
        m_OrderManager.SaveOrders();

        PlayerPrefs.Save();
    }

    private void LoadGameData()
    {
        m_Energy = PlayerPrefs.GetInt(SavePrefix + "Energy", Mathf.Clamp(m_EconomyConfig.StartingEnergy, 0, c_MaxEnergy));
        m_EnergyTimer = PlayerPrefs.GetFloat(SavePrefix + "EnergyTimer", 0f);

        m_Energy = Mathf.Clamp(m_Energy, 0, c_MaxEnergy);
        m_EnergyTimer = Mathf.Clamp(m_EnergyTimer, 0f, c_EnergyRechargeTime);
        string lastSaveTimeStr = PlayerPrefs.GetString(SavePrefix + "LastSaveTime", "");
        if (DateTime.TryParse(lastSaveTimeStr, CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind, out DateTime lastSaveTime))
            ApplyEnergyElapsed(Math.Max(0d, (DateTime.UtcNow - lastSaveTime.ToUniversalTime()).TotalSeconds));

        LoadInventoryData();
        LoadProductionData();
    }

    private void LoadProductionData()
    {
        if (m_ProductionManager == null)
        {
            Debug.LogError("m_ProductionManager is null! Cannot load production data.");
            return;
        }

        int count = PlayerPrefs.GetInt(SavePrefix + "Production_Count", 0);

        for (int i = 0; i < count; i++)
        {
            string baseKey = SavePrefix + $"Production_{i}";
            string recipeName = PlayerPrefs.GetString($"{baseKey}_RecipeName", "");
            string startTimeStr = PlayerPrefs.GetString($"{baseKey}_StartTime", "");
            int isCollected = PlayerPrefs.GetInt($"{baseKey}_IsCollected", 0);
            string kioskId = PlayerPrefs.GetString($"{baseKey}_KioskId", "Kiosk_1");


            if (string.IsNullOrEmpty(recipeName))
            {
                Debug.LogError($"Production {i}: RecipeName is null or empty!");
                continue;
            }

            if (string.IsNullOrEmpty(startTimeStr))
            {
                Debug.LogError($"Production {i}: StartTime is null or empty!");
                continue;
            }

            var recipe = m_ProductionManager.GetRecipeByName(recipeName);
            if (recipe == null)
            {
                Debug.LogError($"Production {i}: Recipe '{recipeName}' not found in ProductionManager!");
                continue;
            }

            if (!DateTime.TryParse(startTimeStr, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTime startTime))
            {
                Debug.LogError($"Production {i}: Could not parse startTime '{startTimeStr}'!");
                continue;
            }

            var job = new ProductionJob(recipe, kioskId);
            job.startTime = startTime.ToUniversalTime();
            job.endTime = job.startTime.AddSeconds(recipe.productionTime);
            job.isCollected = isCollected == 1;

            m_ProductionManager.AddProductionJob(job);
        }
    }

    private void SaveGridData()
    {
        var allCells = m_GridManager.GetAllCells();
        int cellIndex = 0;

        foreach (var cell in allCells)
        {
            string baseKey = SavePrefix + $"Grid_{cellIndex}";

            if (cell.IsEmpty)
            {
                PlayerPrefs.SetInt($"{baseKey}_Type", -1);
            }
            else if (cell.IsProducer())
            {
                PlayerPrefs.SetInt($"{baseKey}_Type", -2);
                PlayerPrefs.SetInt($"{baseKey}_ItemType", (int)cell.GetItemData().ItemType);
            }
            else
            {
                PlayerPrefs.SetInt($"{baseKey}_Type", (int)cell.GetItemData().ItemType);
                PlayerPrefs.SetInt($"{baseKey}_Level", cell.GetLevel());

                PlayerPrefs.SetInt($"{baseKey}_IsFixed", cell.IsFixed() ? 1 : 0);
            }

            cellIndex++;
        }

        PlayerPrefs.SetInt(SavePrefix + "Grid_CellCount", cellIndex);
    }

    private void LoadGridData()
    {
        int cellCount = PlayerPrefs.GetInt(SavePrefix + "Grid_CellCount", 0);
        if (cellCount == 0)
        {
            FillGridOnStart();
            return;
        }

        var allCells = m_GridManager.GetAllCells().ToArray();

        for (int i = 0; i < cellCount && i < allCells.Length; i++)
        {
            string baseKey = SavePrefix + $"Grid_{i}";
            int cellType = PlayerPrefs.GetInt($"{baseKey}_Type", -1);

            if (cellType == -1)
            {
                allCells[i].Clear();
            }
            else if (cellType == -2)
            {
                int itemType = PlayerPrefs.GetInt($"{baseKey}_ItemType", 0);
                var itemData = GetItemDataByType((ItemType)itemType);
                if (itemData != null)
                {
                    allCells[i].SetProducer(itemData);
                }
            }
            else
            {
                int level = PlayerPrefs.GetInt($"{baseKey}_Level", 1);
                int isFixed = PlayerPrefs.GetInt($"{baseKey}_IsFixed", 0);
                var itemData = GetItemDataByType((ItemType)cellType);

                if (itemData != null && itemData.Sprites != null && level >= 1 && level <= itemData.Sprites.Length)
                {
                    if (isFixed == 1)
                    {
                        allCells[i].SetFixedItem(itemData, level);
                    }
                    else
                    {
                        allCells[i].SetItem(itemData, level);
                    }
                }
            }
        }
    }

    private ItemData GetItemDataByType(ItemType _type)
    {
        foreach (var itemData in m_ItemDatas)
        {
            if (itemData != null && itemData.ItemType == _type)
                return itemData;
        }
        return null;
    }

    private void SaveInventoryData()
    {
        if (m_InventoryManager == null) return;

        var inventory = m_InventoryManager.GetInventory();
        PlayerPrefs.SetInt(SavePrefix + "Inventory_Count", inventory.Count);

        for (int i = 0; i < inventory.Count; i++)
        {
            string baseKey = SavePrefix + $"Inventory_{i}";

            if (inventory[i].IsRegularItem)
            {
                PlayerPrefs.SetInt($"{baseKey}_ItemType", 0); // 0 = Regular Item
                PlayerPrefs.SetInt($"{baseKey}_RegularItemType", (int)inventory[i].itemData.ItemType);
            }
            else if (inventory[i].IsProductionItem)
            {
                PlayerPrefs.SetInt($"{baseKey}_ItemType", 1); // 1 = Production Item
                PlayerPrefs.SetString($"{baseKey}_ProductionItemName", inventory[i].productionItemData.itemName);
            }

            PlayerPrefs.SetInt($"{baseKey}_Level", inventory[i].level);
            PlayerPrefs.SetInt($"{baseKey}_Count", inventory[i].count);
            PlayerPrefs.SetString($"{baseKey}_CollectedTime", inventory[i].collectedTime.ToString("O"));
        }
    }

    private void SaveProductionData()
    {
        if (m_ProductionManager == null)
        {
            Debug.LogError("m_ProductionManager is null! Cannot save production data.");
            return;
        }

        var productions = m_ProductionManager.GetActiveProductions();
        PlayerPrefs.SetInt(SavePrefix + "Production_Count", productions.Count);

        for (int i = 0; i < productions.Count; i++)
        {
            string baseKey = SavePrefix + $"Production_{i}";
            PlayerPrefs.SetString($"{baseKey}_RecipeName", productions[i].recipe.recipeName);
            PlayerPrefs.SetString($"{baseKey}_StartTime", productions[i].startTime.ToString("O"));
            PlayerPrefs.SetInt($"{baseKey}_IsCollected", productions[i].isCollected ? 1 : 0);
            PlayerPrefs.SetString($"{baseKey}_KioskId", productions[i].kioskId);
        }
    }

    private void LoadInventoryData()
    {
        if (m_InventoryManager == null) return;

        int count = PlayerPrefs.GetInt(SavePrefix + "Inventory_Count", 0);

        for (int i = 0; i < count; i++)
        {
            string baseKey = SavePrefix + $"Inventory_{i}";
            int itemType = PlayerPrefs.GetInt($"{baseKey}_ItemType", 0);
            int level = PlayerPrefs.GetInt($"{baseKey}_Level", 1);
            int itemCount = PlayerPrefs.GetInt($"{baseKey}_Count", 1);

            if (itemType == 0) // Regular Item
            {
                int regularItemType = PlayerPrefs.GetInt($"{baseKey}_RegularItemType", 0);
                var itemData = GetItemDataByType((ItemType)regularItemType);
                if (itemData != null && itemData.Sprites != null && level >= 1 && level <= itemData.Sprites.Length)
                {
                    m_InventoryManager.AddItem(itemData, level, itemCount, restoring: true);
                }
            }
            else if (itemType == 1) // Production Item
            {
                string productionItemName = PlayerPrefs.GetString($"{baseKey}_ProductionItemName", "");
                if (!string.IsNullOrEmpty(productionItemName))
                {
                    var productionItemData = m_ProductionManager.GetProductionItemByName(productionItemName);
                    if (productionItemData != null && level >= 1 && level <= productionItemData.maxLevel)
                    {
                        m_InventoryManager.AddProductionItem(productionItemData, level, itemCount, restoring: true);
                    }
                    else
                    {
                        Debug.LogError($"Production item not found: {productionItemName}");
                    }
                }
            }
        }
    }

}
}
