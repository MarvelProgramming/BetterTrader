using Menthus15Mods.Valheim.BetterTraderLibrary.Interfaces;
using System.Collections.Generic;
using UnityEngine;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class ObjectDBExtensions
    {
        public static List<ITradableConfig> GetTradableItems(this ObjectDB objectDB)
        {
            var items = new List<ITradableConfig>();

            foreach (var item in objectDB.m_items)
            {
                if (item.TryGetComponent<ItemDrop>(out var itemDrop) && itemDrop.m_itemData.m_shared.m_name.StartsWith("$item") && itemDrop.m_itemData.GetIcon() != null && itemDrop.GetComponentInChildren<Collider>() != null && itemDrop.GetComponentInChildren<MeshRenderer>() != null)
                {
                    items.Add(new Item(itemDrop.name));
                }
            }

            return items;
        }
    }
}
