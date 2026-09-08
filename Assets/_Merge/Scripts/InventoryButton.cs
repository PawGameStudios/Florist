using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace Florist.Merge
{
    public class InventoryButton : MonoBehaviour, IDropHandler
    {
        [SerializeField] private Button inventoryButton;
        [SerializeField] private InventoryWindow inventoryWindow;
        [SerializeField] private GameManager gameManager;
        public void OnDrop(PointerEventData eventData)
        {
            if (gameManager != null && inventoryButton != null && inventoryButton.IsInteractable())
                gameManager.OnInventoryDrop(eventData, (RectTransform)inventoryButton.transform);
        }
        private void OnEnable() { if (inventoryButton != null) inventoryButton.onClick.AddListener(ToggleInventory); }
        private void OnDisable() { if (inventoryButton != null) inventoryButton.onClick.RemoveListener(ToggleInventory); }
        private void ToggleInventory() { if (inventoryWindow != null) inventoryWindow.ToggleInventory(); }
    }
}
