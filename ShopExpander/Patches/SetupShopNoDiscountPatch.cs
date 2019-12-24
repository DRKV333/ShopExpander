using Mono.Cecil.Cil;
using MonoMod.Cil;
using Terraria;

namespace ShopExpander.Patches
{
    internal static class SetupShopNoDiscountPatch
    {
        public static void Load()
        {
            IL.Terraria.Chest.SetupShop += Transpiler;
        }

        private static void Transpiler(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);

            while (cursor.TryGotoNext(MoveType.After, x => x.MatchLdfld<Player>("discount")))
            {
                cursor.Emit(OpCodes.Pop);
                cursor.Emit(OpCodes.Ldc_I4_0);
            }
        }
    }
}