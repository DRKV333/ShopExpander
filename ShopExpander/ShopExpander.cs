using Harmony;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using ShopExpander.Providers;

namespace ShopExpander
{
    public class ShopExpander : Mod
    {
        public static ShopExpander Instance { get; private set; }

        public ModItem ArrowLeft { get; private set; }
        public ModItem ArrowRight { get; private set; }

        public ShopAggregator ActiveShop { get; private set; }
        public readonly CircularBufferProvider Buyback = new CircularBufferProvider("Buyback", ProviderPriority.Buyback);

        private HarmonyInstance harmonyInstance;

        public readonly LazyObjectConfig<int> ProvisionOverrides = new LazyObjectConfig<int>(40);
        public readonly LazyObjectConfig<bool> ModifierOverrides = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> NoDistinctOverrides = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> IgnoreErrors = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> VanillaCopyOverrrides = new LazyObjectConfig<bool>(true);

        private bool textureSetupDone = false;

        public ShopExpander()
        {
            Properties = new ModProperties { Autoload = false };
        }

        public void ResetAndBindShop()
        {
            ActiveShop = new ShopAggregator();
            ActiveShop.AddPage(Buyback);
            Main.instance.shop[Main.npcShop].item = ActiveShop.CurrentFrame;
        }

        public override void Load()
        {
            if (Instance != null)
                throw new InvalidOperationException("An instance of ShopExpander is already loaded.");

            harmonyInstance = HarmonyInstance.Create(Name);

            ArrowLeft = new ArrowItem();
            AddItem("ArrowLeft", ArrowLeft);

            ArrowRight = new ArrowItem();
            AddItem("ArrowRight", ArrowRight);

            Instance = this;
        }

        public override void PostSetupContent()
        {
            Patches.SetupShopPatch.Load();
            harmonyInstance.PatchAll();

            ArrowLeft.DisplayName.SetDefault("Previous page");
            ArrowRight.DisplayName.SetDefault("Next page");

            if (!Main.dedServ)
            {
                Main.itemTexture[ArrowLeft.item.type] = CropTexture(Main.textGlyphTexture[0], new Rectangle(4 * 28, 0, 28, 28));
                Main.itemTexture[ArrowRight.item.type] = CropTexture(Main.textGlyphTexture[0], new Rectangle(5 * 28, 0, 28, 28));
                textureSetupDone = true;
            }
        }

        public override void Unload()
        {
            Patches.SetupShopPatch.Unload();
            if (harmonyInstance != null)
                harmonyInstance.UnpatchAll();

            if (textureSetupDone)
            {
                Main.itemTexture[ArrowLeft.item.type].Dispose();
                Main.itemTexture[ArrowRight.item.type].Dispose();
            }

            Instance = null;
        }

        public override object Call(params object[] args)
        {
            string command = args[0] as string;
            if (command == null)
                throw new ArgumentException("First argument must be string");

            switch (command)
            {
                case "SetProvisionSize":
                    if (!(args[2] is int))
                        throw new ArgumentException("Third argument must be int for SetProvisionSize");
                    ProvisionOverrides.SetValue(args[1], (int)args[2]);
                    break;

                case "SetModifier":
                    ModifierOverrides.SetValue(args[1], true);
                    break;

                case "SetNoDistinct":
                    NoDistinctOverrides.SetValue(args[1], true);
                    break;

                case "GetLastShopExpanded":
                    if (ActiveShop != null)
                        return ActiveShop.GetAllItems().ToArray();
                    break;

                default:
                    throw new ArgumentException(string.Format("Unknown command: {0}", command));
            }
            return null;
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (Main.npcShop == 0)
                ActiveShop = null;
        }

        private Texture2D CropTexture(Texture2D texture, Rectangle newBounds)
        {
            Texture2D newTexture = new Texture2D(Main.graphics.GraphicsDevice, newBounds.Width, newBounds.Height);
            int area = newBounds.Width * newBounds.Height;
            Color[] data = new Color[area];
            texture.GetData(0, newBounds, data, 0, area);
            newTexture.SetData(data);
            return newTexture;
        }
    }
}