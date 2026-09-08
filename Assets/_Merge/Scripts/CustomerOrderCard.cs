using UnityEngine;
using UnityEngine.UI;
using TMPro;
namespace Florist.Merge
{
    public class CustomerOrderCard : MonoBehaviour
    {
        [SerializeField] private Image customerImage;
        [SerializeField] private Image itemImage;
        [SerializeField] private TextMeshProUGUI rewardText;
        [SerializeField] private Button completeButton;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image completeButtonImage;
        [SerializeField] private TextMeshProUGUI requestText;
        private Order currentOrder;
        private OrderManager orderManager;
        private InventoryManager inventoryManager;
        [SerializeField] private CanvasGroup panelGroup;
        private MergePanelMotion motion;
        private bool retiring;
        public Order CurrentOrder => currentOrder;
        public void PlayExit(System.Action completed)
        {
            retiring = true;
            if (completeButton != null) completeButton.interactable = false;
            motion.Hide(completed);
        }
        public void Initialize(Order order, OrderManager orders, InventoryManager inventory)
        {
            Unsubscribe();
            currentOrder = order;
            orderManager = orders;
            inventoryManager = inventory;
            if (isActiveAndEnabled) Subscribe();
            if (customerImage != null) customerImage.sprite = order.customer.customerSprite;
            if (itemImage != null) itemImage.sprite = order.requestedItem.itemSprite;
            if (requestText != null) requestText.text = $"{order.customer.displayName}\n{order.GetOrderDescription()}";
            if (rewardText != null) rewardText.text = $"+{order.reward}";
            UpdateCompleteButton();
        }
        private void OnEnable()
        {
            if (motion == null) motion = new MergePanelMotion(transform, panelGroup);
            retiring = false;
            motion.Show();
            if (completeButton != null) completeButton.onClick.AddListener(OnCompleteButtonClicked);
            Subscribe();
            UpdateCompleteButton();
        }
        private void OnDisable()
        {
            motion?.Reset();
            if (completeButton != null) completeButton.onClick.RemoveListener(OnCompleteButtonClicked);
            Unsubscribe();
        }
        private void Subscribe() { if (inventoryManager != null) inventoryManager.OnInventoryChanged += UpdateCompleteButton; }
        private void Unsubscribe() { if (inventoryManager != null) inventoryManager.OnInventoryChanged -= UpdateCompleteButton; }
        private void UpdateCompleteButton()
        {
            bool canComplete = !retiring && currentOrder != null && inventoryManager != null && currentOrder.CanBeCompleted(inventoryManager);
            if (completeButton != null) completeButton.interactable = canComplete;
            if (completeButtonImage != null) completeButtonImage.color = canComplete ? Color.green : Color.gray;
        }
        private void OnCompleteButtonClicked()
        {
            if (!retiring && orderManager != null && currentOrder != null && inventoryManager != null && currentOrder.CanBeCompleted(inventoryManager))
                orderManager.CompleteOrder(currentOrder);
        }
    }
}
