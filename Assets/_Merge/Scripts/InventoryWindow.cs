using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
namespace Florist.Merge
{
    public class InventoryWindow : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryPanel;
        [SerializeField] private Transform itemContainer;
        [SerializeField] private InventorySlot slotPrefab;
        [SerializeField] private Button closeButton;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CanvasGroup panelGroup;
        [SerializeField] private Transform seedContainer;
        [SerializeField] private Button previousButton;
        [SerializeField] private Button nextButton;
        [SerializeField] private TextMeshProUGUI pageText;
        private int page;
        private MergePanelMotion motion;
        private bool closing;
        private void EnsureMotion()
        {
            if (motion == null && inventoryPanel != null) motion = new MergePanelMotion(inventoryPanel.transform, panelGroup);
        }
        private readonly List<InventorySlot> slots = new List<InventorySlot>();
        private void OnEnable()
        {
            EnsureMotion();
            closing = false;
            motion?.Show();
            if (closeButton != null) closeButton.onClick.AddListener(CloseInventory);
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged += UpdateInventoryDisplay;
            }
            if (previousButton != null) previousButton.onClick.AddListener(PreviousPage);
            if (nextButton != null) nextButton.onClick.AddListener(NextPage);
            EnsureSlots();
            UpdateInventoryDisplay();
        }
        private void OnDisable()
        {
            if (previousButton != null) previousButton.onClick.RemoveListener(PreviousPage);
            if (nextButton != null) nextButton.onClick.RemoveListener(NextPage);
            motion?.Reset();
            closing = false;
            if (closeButton != null) closeButton.onClick.RemoveListener(CloseInventory);
            if (inventoryManager != null)
            {
                inventoryManager.OnInventoryChanged -= UpdateInventoryDisplay;
            }
        }
        public void ToggleInventory()
        {
            if (inventoryPanel == null) return;
            if (inventoryPanel.activeSelf && !closing) CloseInventory(); else OpenInventory();
        }
        public void OpenInventory()
        {
            if (inventoryPanel == null) return;
            EnsureMotion();
            bool animate = !inventoryPanel.activeSelf || closing;
            closing = false;
            inventoryPanel.SetActive(true);
            if (animate) motion?.Show();
            EnsureSlots();
            UpdateInventoryDisplay();
        }
        public void CloseInventory()
        {
            if (inventoryPanel == null || closing || !inventoryPanel.activeSelf) return;
            EnsureMotion();
            closing = true;
            motion.Hide(() => { closing = false; inventoryPanel.SetActive(false); });
        }
        private void EnsureSlots()
        {
            if (slotPrefab == null || itemContainer == null || inventoryManager == null) return;
            int targetCount = seedContainer != null ? 9 : Mathf.Max(inventoryManager.Capacity, inventoryManager.GetInventory().Count);
            while (slots.Count < targetCount)
            {
                var slot = Instantiate(slotPrefab, seedContainer != null && slots.Count >= 3 ? seedContainer : itemContainer);
                slot.SetReturnHandler(ReturnToBoard);
                slots.Add(slot);
            }
        }
        public void ShowFullInventory()
        {
            OpenInventory();
        }
        private void ReturnToBoard(InventoryItem item)
        {
            if (item == null || item.IsProductionItem || gameManager == null) return;
            gameManager.ReturnInventoryItem(item);
        }

        private void PreviousPage() { page = Mathf.Max(0, page - 1); UpdateInventoryDisplay(); }
        private void NextPage() { page++; UpdateInventoryDisplay(); }
        private void UpdateInventoryDisplay()
        {
            if (inventoryManager == null) return;
            EnsureSlots();
            var inventory = inventoryManager.GetInventory();
            if (seedContainer != null)
            {
                var materials = new List<InventoryItem>();
                var seeds = new List<InventoryItem>();
                foreach (var item in inventory)
                    if (item.IsProductionItem || (item.IsRegularItem && (item.itemData.ItemType == ItemType.Papatya || item.itemData.ItemType == ItemType.Starlice))) seeds.Add(item);
                    else materials.Add(item);
                int maxPage = Mathf.Max(0, Mathf.Max((materials.Count - 1) / 3, (seeds.Count - 1) / 6));
                page = Mathf.Clamp(page, 0, maxPage);
                if (previousButton != null) previousButton.interactable = page > 0;
                if (nextButton != null) nextButton.interactable = page < maxPage;
                if (pageText != null) pageText.text = $"{page + 1}/{maxPage + 1}";
                for (int i = 0; i < 9; i++)
                {
                    var items = i < 3 ? materials : seeds;
                    int index = i < 3 ? page * 3 + i : page * 6 + i - 3;
                    slots[i].SetItem(index < items.Count ? items[index] : null);
                }
                return;
            }
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null) slots[i].SetItem(i < inventory.Count ? inventory[i] : null);
        }
    }
}
