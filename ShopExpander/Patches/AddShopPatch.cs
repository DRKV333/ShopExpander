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
            if (__instance != Main.instance.shop[Main.npcShop] || ShopExpander.Instance.ActiveShop == null)
                return true;

            Item insertItem = newItem.Clone();
            insertItem.favorited = false;
            insertItem.buyOnce = true;
            insertItem.value /= 5;
            if (insertItem.value < 1)
                insertItem.value = 1;

            ShopExpander.Instance.Buyback.AddItem(insertItem);
            ShopExpander.Instance.ActiveShop.RefreshFrame();

            return false;
        }
    }
}