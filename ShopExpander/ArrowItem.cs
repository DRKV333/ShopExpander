using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ShopExpander
{
    internal class ArrowItem : ModItem
    {
        public override void AutoStaticDefaults()
        {
            //Stop automatic texture and display name assignment
        }

        public override void SetDefaults()
        {
            item.width = 28;
            item.height = 28;
            item.maxStack = 1;
            item.value = 0;
            item.rare = ItemRarityID.White;
        }

        public override bool OnPickup(Player player)
        {
            return false;
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            tooltips.RemoveAll(x => !(x.mod == "Terraria" && x.Name == "ItemName"));
        }
    }
}