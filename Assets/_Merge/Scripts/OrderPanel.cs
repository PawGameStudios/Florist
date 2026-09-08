using UnityEngine;
using System.Collections.Generic;
namespace Florist.Merge
{
    public class OrderPanel : MonoBehaviour
    {
        [SerializeField] private Transform orderContainer;
        [SerializeField] private CustomerOrderCard orderCardPrefab;
        [SerializeField] private OrderManager orderManager;
        [SerializeField] private InventoryManager inventoryManager;
        private readonly List<CustomerOrderCard> activeCards = new List<CustomerOrderCard>();
        private readonly List<CustomerOrderCard> retiringCards = new List<CustomerOrderCard>();
        private void OnEnable()
        {
            if (orderManager != null) orderManager.OnOrdersChanged += UpdateOrderDisplay;
            UpdateOrderDisplay();
        }
        private void OnDisable()
        {
            if (orderManager != null) orderManager.OnOrdersChanged -= UpdateOrderDisplay;
            foreach (var card in retiringCards)
                if (card != null) { card.gameObject.SetActive(false); Destroy(card.gameObject); }
            retiringCards.Clear();
        }
        private void UpdateOrderDisplay()
        {
            if (orderManager == null || orderCardPrefab == null || orderContainer == null) return;
            var orders = orderManager.GetActiveOrders();
            for (int i = activeCards.Count - 1; i >= 0; i--)
            {
                var card = activeCards[i];
                if (card != null && orders.Contains(card.CurrentOrder)) continue;
                activeCards.RemoveAt(i);
                if (card == null) continue;
                retiringCards.Add(card);
                card.PlayExit(() =>
                {
                    retiringCards.Remove(card);
                    if (card != null) { card.gameObject.SetActive(false); Destroy(card.gameObject); }
                });
            }
            foreach (var order in orders)
            {
                if (activeCards.Exists(card => card != null && card.CurrentOrder == order)) continue;
                var card = Instantiate(orderCardPrefab, orderContainer);
                card.Initialize(order, orderManager, inventoryManager);
                activeCards.Add(card);
            }
        }
    }
}
