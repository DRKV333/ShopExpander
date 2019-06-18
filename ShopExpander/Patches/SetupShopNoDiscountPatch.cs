using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;
using System.Reflection;
using Terraria;
using Harmony;

namespace ShopExpander.Patches
{
    [HarmonyPatch(typeof(Chest), "SetupShop")]
    [HarmonyPriority(Priority.Normal)]
    internal static class SetupShopNoDiscountPatch
    {
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> original)
        {
            foreach (var item in original)
            {
                yield return item;

                if (item.opcode == OpCodes.Ldfld && ((FieldInfo)item.operand).Name == "discount")
                {
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                }
            }
        }
    }
}
