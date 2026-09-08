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
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private InventoryManager inventoryManager;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private CanvasGroup panelGroup;
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
            EnsureSlots();
            UpdateInventoryDisplay();
        }
        private void OnDisable()
        {
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
            int targetCount = Mathf.Max(inventoryManager.Capacity, inventoryManager.GetInventory().Count);
            while (slots.Count < targetCount)
            {
                var slot = Instantiate(slotPrefab, itemContainer);
                slot.SetReturnHandler(ReturnToBoard);
                slots.Add(slot);
            }
        }
        public void ShowFullInventory()
        {
            OpenInventory();
            if (titleText != null) titleText.text = "Envanter dolu; malzemeleri kullan veya tahtaya koy";
        }
        private void ReturnToBoard(InventoryItem item)
        {
            if (item == null) return;
            if (item.IsProductionItem)
            {
                if (titleText != null) titleText.text = "Bu ürün müşteri siparişlerinde kullanılır";
                return;
            }
            if (gameManager == null)
            {
                if (titleText != null) titleText.text = "Envanterin tahta bağlantısı eksik";
                return;
            }
            var result = gameManager.ReturnInventoryItem(item);
            if (titleText == null) return;
            switch (result)
            {
                case GameManager.InventoryReturnResult.Success: titleText.text = "1 malzeme tahtaya döndü"; break;
                case GameManager.InventoryReturnResult.BoardFull: titleText.text = "Tahtada boş yer gerekli"; break;
                case GameManager.InventoryReturnResult.NotReady: titleText.text = "Tahta hazır değil; işlemini bitirip tekrar dene"; break;
                case GameManager.InventoryReturnResult.ItemUnavailable: titleText.text = "Bu malzeme artık envanterde değil"; break;
                default: titleText.text = "Bu ürün tahtaya yerleştirilemiyor"; break;
            }
        }

        private void UpdateInventoryDisplay()
        {
            if (inventoryManager == null) return;
            EnsureSlots();
            var inventory = inventoryManager.GetInventory();
            if (titleText != null) titleText.text = $"Envanter {inventory.Count}/{inventoryManager.Capacity}";
            for (int i = 0; i < slots.Count; i++)
                if (slots[i] != null) slots[i].SetItem(i < inventory.Count ? inventory[i] : null);
        }
    }
}
