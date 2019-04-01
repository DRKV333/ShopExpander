using Harmony;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.ModLoader;

namespace ShopExpander.Patches
{
    [HarmonyPatch(typeof(NPCLoader), "SetupShop")]
    [HarmonyPriority(Priority.Low)]
    internal static class SetupShopPatch
    {
        private static GlobalNPC[] arr;
        private static int[] shopToNpcs;

        //I had trouble getting HarmonyPrepare to work
        public static void Load()
        {
            object hooklist = typeof(NPCLoader).GetField("HookSetupShop", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
            arr = (GlobalNPC[])hooklist.GetType().GetField("arr", BindingFlags.Public | BindingFlags.Instance).GetValue(hooklist);
            shopToNpcs = (int[])typeof(NPCLoader).GetField("shopToNPC", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);
        }

        public static void Unload()
        {
            arr = null;
            shopToNpcs = null;
        }

        [HarmonyPrefix]
        private static bool Prefix(int type, Chest shop)
        {
            ShoppingList list = new ShoppingList(ref shop.item);
            ShopExpander.Instance.LastShopExpanded = list;

            List<GlobalNPC> modifiers = new List<GlobalNPC>();

            if (type < shopToNpcs.Length)
            {
                type = shopToNpcs[type];
            }
            else
            {
                ModNPC npc = NPCLoader.GetNPC(type);
                if (npc != null)
                {
                    int zero = 0;
                    npc.SetupShop(ProvisionChest(list, npc), ref zero);
                }
            }
            foreach (GlobalNPC globalNPC in arr)
            {
                if (ShopExpander.Instance.ModifierOverrides.GetValue(globalNPC))
                {
                    modifiers.Add(globalNPC);
                }
                else
                {
                    int zero = 0;
                    globalNPC.SetupShop(type, ProvisionChest(list, globalNPC), ref zero);
                }
            }

            list.Compose();

            foreach (var item in modifiers)
            {
                int max = list.ExtendedItems.Length;
                item.SetupShop(type, MakeFakeChest(list.ExtendedItems), ref max);
            }

            return false;
        }

        private static Chest MakeFakeChest(Item[] items)
        {
            Chest fake = new Chest(false);
            fake.item = items;
            return fake;
        }

        private static Chest ProvisionChest(ShoppingList list, object target)
        {
            return MakeFakeChest(list.Provision(ShopExpander.Instance.ProvisionOverrides.GetValue(target), ShopExpander.Instance.NoDistinctOverrides.GetValue(target)));
        }
    }
}