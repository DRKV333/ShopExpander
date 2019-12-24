﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ModLoader;
using ShopExpander.Providers;
using ShopExpander.Patches;

namespace ShopExpander
{
    public class ShopExpander : Mod
    {
        public static class CallApi
        {
            public const string SetProvisionSize = "SetProvisionSize";
            public const string SetModifier = "SetModifier";
            public const string SetNoDistinct = "SetNoDistinct";
            public const string SetVanillaNoCopy = "SetVanillaNoCopy";
            public const string AddLegacyMultipageSetupMethods = "AddLegacyMultipageSetupMethods";
            public const string AddPageFromArray = "AddPageFromArray";
            public const string ResetAndBindShop = "ResetAndBindShop";
            public const string GetLastShopExpanded = "GetLastShopExpanded";
        }

        public static ShopExpander Instance { get; private set; }

        public ModItem ArrowLeft { get; private set; }
        public ModItem ArrowRight { get; private set; }

        public ShopAggregator ActiveShop { get; private set; }
        public readonly CircularBufferProvider Buyback = new CircularBufferProvider("Buyback", ProviderPriority.Buyback);

        public readonly LazyObjectConfig<int> ProvisionOverrides = new LazyObjectConfig<int>(40);
        public readonly LazyObjectConfig<bool> ModifierOverrides = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> NoDistinctOverrides = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> IgnoreErrors = new LazyObjectConfig<bool>(false);
        public readonly LazyObjectConfig<bool> VanillaCopyOverrrides = new LazyObjectConfig<bool>(true);
        public readonly LazyObjectConfig<(string name, int priority, Action setup)[]> LegacyMultipageSetupMethods = new LazyObjectConfig<(string, int, Action)[]>();

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

            ArrowLeft = new ArrowItem();
            AddItem("ArrowLeft", ArrowLeft);

            ArrowRight = new ArrowItem();
            AddItem("ArrowRight", ArrowRight);

            Instance = this;
        }

        public override void PostSetupContent()
        {
            SetupShopPatch.Load();
            AddShopPatch.Load();
            SetupShopNoDiscountPatch.Load();
            LeftRightClickPatch.Load();

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
                throw new ArgumentException("first argument must be string");

            switch (command)
            {
                case CallApi.SetProvisionSize:
                    ProvisionOverrides.SetValue(args[1], AssertAndCast<int>(args, 2, CallApi.SetProvisionSize));
                    break;

                case CallApi.SetModifier:
                    ModifierOverrides.SetValue(args[1], true);
                    break;

                case CallApi.SetNoDistinct:
                    NoDistinctOverrides.SetValue(args[1], true);
                    break;

                case CallApi.SetVanillaNoCopy:
                    VanillaCopyOverrrides.SetValue(args[1], false);
                    break;

                case CallApi.AddLegacyMultipageSetupMethods:
                    if (args.Length % 3 != 2)
                        throw new ArgumentException("The number of arguments is incorrect (args.Length % 3 != 1) for " + CallApi.AddLegacyMultipageSetupMethods);

                    var methods = new (string name, int priority, Action setup)[args.Length / 3]; 
                    for (int i = 0; i < methods.Length; i++)
                    {
                        int offset = i * 3 + 2;
                        methods[i].name = AssertAndCast<string>(args, offset, CallApi.AddLegacyMultipageSetupMethods);
                        methods[i].priority = AssertAndCast<int>(args, offset + 1, CallApi.AddLegacyMultipageSetupMethods);
                        methods[i].setup = AssertAndCast<Action>(args, offset + 2, CallApi.AddLegacyMultipageSetupMethods);
                    }

                    LegacyMultipageSetupMethods.SetValue(args[1], methods);
                    break;

                case CallApi.AddPageFromArray:
                    if (ActiveShop == null)
                        throw new InvalidOperationException($"No active shop, try calling {CallApi.ResetAndBindShop} first");
                    ActiveShop.AddPage(new ArrayProvider(AssertAndCast<string>(args, 1, CallApi.AddPageFromArray),
                                                         AssertAndCast<int>(args, 2, CallApi.AddPageFromArray), 
                                                         AssertAndCast<Item[]>(args, 3, CallApi.AddPageFromArray)));
                    break;

                case CallApi.ResetAndBindShop:
                    ResetAndBindShop();
                    break;

                case CallApi.GetLastShopExpanded:
                    if (ActiveShop != null)
                        return ActiveShop.GetAllItems().ToArray();
                    break;

                default:
                    throw new ArgumentException($"Unknown command: {command}");
            }
            return null;
        }

        private T AssertAndCast<T>(object[] args, int index, string site, bool checkForNull = false)
        {
            if (checkForNull && args[index] == null)
                throw new ArgumentNullException($"args[{index}] cannot be null for {site}");
            if (!(args[index] is T casted))
                throw new ArgumentException($"args[{index}] must be {typeof(T).Name} for {site}");
            return casted;
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