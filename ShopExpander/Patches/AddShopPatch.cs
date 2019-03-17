using Harmony;
using Terraria;

namespace ShopExpander.Patches
{
    [HarmonyPatch(typeof(Chest), "AddShop")]
    [HarmonyPriority(Priority.Normal)]
    internal static class AddShopPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Chest __instance, Item newItem)
        {
            if (__instance != Main.instance.shop[Main.npcShop])
                return true;

            Item[] target = ShopExpander.Instance.LastShopExpanded.BuybackItems;

            Item insertItem = newItem.Clone();
            insertItem.favorited = false;
            insertItem.buyOnce = true;
            insertItem.value /= 5;
            if (insertItem.value < 1)
                insertItem.value = 1;

            for (int i = 0; i < target.Length; i++)
            {
                if (target[i].IsAir)
                {
                    target[i] = insertItem;
                    break;
                }
            }

            ShopExpander.Instance.LastShopExpanded.RefreshFrame();

            return false;
        }
    }
}