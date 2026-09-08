using System.Collections.Generic;
using Config;
using UnityEngine;

// Runtime orders are separate from the authored customer assets.
public static class CustomerOrderFactory
{
    public static CustomerInfo Prepare(CustomerInfo source, CustomerConfig customers, WorkshopConfig workshop,
        ShopConfig shop, ShopData ownership, EconomyConfig economy)
    {
        if (source.CustomerType != CustomerType.Random && CanFulfill(source, workshop, ownership))
            return source.RuntimeCopy();

        var result = customers.GetCustomer(CustomerType.Random);
        result.Sprite = source.Sprite;
        result.ChoseOrderRandomly = false;
        result.CustomInitConvo = false;
        result.CustomGoodbyeConvo = false;
        var flowers = workshop.FlowerInfo.FindAll(f => Owns(ownership, f.Id));
        var papers = workshop.WrappingPaperInfo.FindAll(p => Owns(ownership, p.Id));
        var ribbons = workshop.RibbonInfo.FindAll(r => Owns(ownership, r.Id));
        if (flowers.Count == 0 || papers.Count == 0 || ribbons.Count == 0)
            throw new System.InvalidOperationException("The starter flower, paper and ribbon must be owned.");

        FlowerInfo main = flowers[Random.Range(0, flowers.Count)];
        if (Random.value < economy.LatestFlowerOrderChance)
        {
            int newestDay = -1;
            foreach (var flower in flowers)
            {
                var item = shop.GetItemById(ItemType.Flower, flower.Id);
                if (item.UnlockDay > newestDay) { newestDay = item.UnlockDay; main = flower; }
            }
        }
        // Two flower types keeps the existing conversation/order format readable.
        var fillers = flowers.FindAll(f => f.FlowerType != main.FlowerType);
        FlowerInfo filler = fillers.Count > 0 ? fillers[Random.Range(0, fillers.Count)] : main;
        int count = Mathf.Clamp(economy.RandomOrderFlowerCount, 2, 6);
        var content = new List<BouquetFlowerInfo>
        {
            new BouquetFlowerInfo { FlowerType = main.FlowerType, FlowerColor = main.Color, Count = count - 1 }
        };
        if (filler == main) content[0].Count++;
        else content.Add(new BouquetFlowerInfo { FlowerType = filler.FlowerType, FlowerColor = filler.Color, Count = 1 });
        result.Orders = new List<Order>
        {
            new Order { BouquetType = BouquetType.Custom, CustomFlowers = content,
                WrappingPaperType = papers[Random.Range(0, papers.Count)].WrappingPaperType,
                RibbonType = ribbons[Random.Range(0, ribbons.Count)].RibbonType }
        };
        return result;
    }

    private static bool Owns(ShopData data, string id) =>
        !string.IsNullOrEmpty(id) && data.Items.ContainsKey(id) && data.HasItem(id);

    private static bool CanFulfill(CustomerInfo customer, WorkshopConfig workshop, ShopData ownership)
    {
        if (customer.Orders == null || customer.Orders.Count == 0) return false;
        foreach (var order in customer.Orders)
        {
            if (!Owns(ownership, workshop.GetWrappingPaperId(order.WrappingPaperType)) ||
                !Owns(ownership, workshop.GetRibbonId(order.RibbonType))) return false;
            List<BouquetFlowerInfo> flowers;
            if (order.BouquetType == BouquetType.Custom) flowers = order.CustomFlowers;
            else if (workshop.BouquetRecipes.TryGetValue(order.BouquetType, out var recipe)) flowers = recipe.Bouquet.Flowers;
            else return false;
            if (flowers == null || flowers.Count == 0) return false;
            foreach (var flower in flowers)
                if (!Owns(ownership, workshop.GetFlowerId(flower.FlowerType, flower.FlowerColor))) return false;
        }
        return true;
    }
}
