namespace Florist.Merge
{
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class InventorySlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI countText;
    [SerializeField] private Image backgroundImage;

    private InventoryItem currentItem;
    private System.Action<InventoryItem> returnHandler;
    public void SetReturnHandler(System.Action<InventoryItem> handler) => returnHandler = handler;

    public void SetItem(InventoryItem item)
    {
        currentItem = item;
        if (item != null)
        {
            if (item.IsRegularItem && item.itemData != null && item.itemData.Sprites != null && item.level > 0 && item.level <= item.itemData.Sprites.Length)
            {
                // Normal ItemData için
                itemImage.sprite = item.itemData.Sprites[item.level - 1];
                itemImage.enabled = true;
                countText.text = item.IsRegularItem ? MergeLocalization.Format("merge_inventory_stack", item.level, item.count) : $"×{item.count}";
                countText.enabled = true;
            }
            else if (item.IsProductionItem && item.productionItemData != null)
            {
                // ProductionItemData için
                itemImage.sprite = item.productionItemData.itemSprite;
                itemImage.enabled = true;
                countText.text = item.IsRegularItem ? MergeLocalization.Format("merge_inventory_stack", item.level, item.count) : $"×{item.count}";
                countText.enabled = true;
            }
            else
            {
                ClearSlot();
            }
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        itemImage.enabled = false;
        countText.enabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left) returnHandler?.Invoke(currentItem);
    }
}

}
