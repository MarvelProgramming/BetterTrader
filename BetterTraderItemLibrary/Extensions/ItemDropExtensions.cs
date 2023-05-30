namespace Menthus15Mods.Valheim.BetterTraderLibrary.Extensions
{
    public static class ItemDropExtensions
    {
        public static string GetCustomCategory(this ItemDrop itemDrop)
        {
            var category = itemDrop.m_itemData.m_shared.m_itemType.ToString();

            switch (category.ToLower())
            {
                case "helmet":
                case "shoulder":
                case "chest":
                case "legs":
                    return "Equipment";
                case "bow":
                case "onehandedweapon":
                case "twohandedweapon":
                case "shield":
                    return "Combat";
                default:
                    return category;
            }
        }
    }
}
