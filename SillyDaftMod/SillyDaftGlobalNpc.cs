using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SillyDaftMod
{
    internal class SillyDaftGlobalNpc : GlobalNPC
    {
        public override void SetupShop(int type, Chest shop, ref int nextSlot)
        {
            if (type == NPCID.Dryad)
            {
                for (int i = 0; i < ItemLoader.ItemCount; i++)
                {
                    shop.item[nextSlot++].SetDefaults(i);
                    shop.item[nextSlot++].SetDefaults(i);
                }
            }
        }
    }
}