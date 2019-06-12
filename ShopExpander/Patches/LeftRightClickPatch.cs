using Harmony;
using Terraria;
using Terraria.UI;

namespace ShopExpander.Patches
{
    [HarmonyPatch(typeof(ItemSlot), "LeftClick", typeof(Item[]), typeof(int), typeof(int))]
    [HarmonyPriority(Priority.Normal)]
    internal static class LeftClickPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Item[] inv, int context, int slot)
        {
            return ClickHelper.Prefix(inv, context, slot, false);
        }
    }

    [HarmonyPatch(typeof(ItemSlot), "RightClick", typeof(Item[]), typeof(int), typeof(int))]
    [HarmonyPriority(Priority.Normal)]
    internal static class RightClickPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Item[] inv, int context, int slot)
        {
            if (Main.mouseRight)
            {
                return ClickHelper.Prefix(inv, context, slot, true);
            }
            else
            {
                return true;
            }
        }
    }

    internal static class ClickHelper
    {
        public static bool Prefix(Item[] inv, int context, int slot, bool skip)
        {
            if (ShopExpander.Instance.LastShopExpanded == null)
                return true;

            if (inv[slot].type == ShopExpander.Instance.ArrowLeft.item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.LastShopExpanded.MoveFirst();
                    else
                        ShopExpander.Instance.LastShopExpanded.MoveLeft();
                else
                    inv[slot] = new Item();
                return false;
            }
            if (inv[slot].type == ShopExpander.Instance.ArrowRight.item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.LastShopExpanded.MoveLast();
                    else
                        ShopExpander.Instance.LastShopExpanded.MoveRight();
                else
                    inv[slot] = new Item();
                return false;
            }

            return true;
        }
    }
}