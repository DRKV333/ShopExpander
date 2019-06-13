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
            if (ShopExpander.Instance.ActiveShop == null)
                return true;

            if (inv[slot].type == ShopExpander.Instance.ArrowLeft.item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.ActiveShop.MoveFirst();
                    else
                        ShopExpander.Instance.ActiveShop.MoveLeft();
                else
                    inv[slot] = new Item();
                return false;
            }
            if (inv[slot].type == ShopExpander.Instance.ArrowRight.item.type)
            {
                if (context == 15)
                    if (skip)
                        ShopExpander.Instance.ActiveShop.MoveLast();
                    else
                        ShopExpander.Instance.ActiveShop.MoveRight();
                else
                    inv[slot] = new Item();
                return false;
            }

            return true;
        }
    }
}