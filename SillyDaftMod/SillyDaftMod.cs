using Terraria.ModLoader;

namespace SillyDaftMod
{
    public class SillyDaftMod : Mod
    {
        public SillyDaftMod()
        {
            Properties = ModProperties.AutoLoadAll;
        }

        public override void PostSetupContent()
        {
            Mod shopExpander = ModLoader.GetMod("ShopExpander");
            if (shopExpander != null)
            {
                shopExpander.Call("SetProvisionSize", ModContent.GetInstance<SillyDaftGlobalNpc>(), ItemLoader.ItemCount * 2);
                //shopExpander.Call("SetNoDistinct", GetGlobalNPC<SillyDaftGlobalNpc>());

                shopExpander.Call("SetModifier", ModContent.GetInstance<FreeloadGlobalNpc>());
            }
        }
    }
}