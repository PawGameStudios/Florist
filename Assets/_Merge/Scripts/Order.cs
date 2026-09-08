using System;
using System.Collections.Generic;
using System.Linq;

namespace Florist.Merge
{
    [Serializable]
    public class OrderRequest
    {
        public ProductionItemData item;
        public int count = 1;
    }

    [Serializable]
    public class Order
    {
        public CustomerData customer;
        // Legacy fields remain readable for existing saves/cards.
        public ProductionItemData requestedItem;
        public int requestedCount = 1;
        public List<OrderRequest> requests = new List<OrderRequest>();
        public int reward;
        public DateTime orderTime;
        public bool isCompleted;

        public Order(CustomerData customerData, ProductionItemData item, int count = 1, int rewardMultiplier = -1)
            : this(customerData, new List<OrderRequest> { new OrderRequest { item = item, count = count } }, rewardMultiplier) { }

        public Order(CustomerData customerData, List<OrderRequest> items, int rewardMultiplier = 1)
        {
            customer = customerData;
            requests = items;
            requestedItem = items[0].item;
            requestedCount = items[0].count;
            orderTime = DateTime.UtcNow;
            int multiplier = rewardMultiplier < 0 ? 1 : rewardMultiplier;
            long value = items.Sum(x => (long)Math.Max(0, x.item.baseValue) * x.count);
            reward = (int)Math.Min(int.MaxValue, Math.Max(0L, value * multiplier));
        }
        public List<OrderRequest> GetRequests() => requests != null && requests.Count > 0 ? requests :
            new List<OrderRequest> { new OrderRequest { item = requestedItem, count = requestedCount } };
        public string GetOrderDescription() => string.Join("\n", GetRequests().Select(x => $"{x.count} × {x.item.itemName}"));
        public bool CanBeCompleted() => CanBeCompleted(InventoryManager.Instance);
        public bool CanBeCompleted(InventoryManager inventory) => !isCompleted && inventory != null && inventory.HasProducts(GetRequests());
    }
}
