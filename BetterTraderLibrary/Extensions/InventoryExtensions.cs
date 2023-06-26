using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class InventoryExtensions
    {
        public static bool CanAddItemStack(this Inventory inventory, ItemDrop.ItemData item, int quantity)
        {
            int availableSpace = inventory.GetEmptySlots() * item.m_shared.m_maxStackSize;
            availableSpace += inventory.FindFreeStackSpace(item.m_shared.m_name);

            return availableSpace >= quantity;
        }
    }
}
