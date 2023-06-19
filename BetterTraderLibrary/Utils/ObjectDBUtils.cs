namespace Menthus15Mods.Valheim.BetterTraderLibrary.Utils
{
    public static class ObjectDBUtils
    {
        public static ItemDrop GetDropFromItemName(string itemName)
        {
            return ObjectDB.instance.m_itemByHash[itemName.GetStableHashCode()].GetComponent<ItemDrop>();
        }
    }
}
