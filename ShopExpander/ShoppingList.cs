using System.Collections.Generic;
using System.Linq;
using Terraria;

namespace ShopExpander
{
    public class ShoppingList
    {
        private struct ProvisionedSegment
        {
            public Item[] items;
            public bool noDistinct;

            public ProvisionedSegment(int size, bool noDistinct)
            {
                items = new Item[size];
                for (int i = 0; i < size; i++)
                {
                    items[i] = new Item();
                }
                this.noDistinct = noDistinct;
            }
        }

        private class ItemSameType : IEqualityComparer<Item>
        {
            public bool Equals(Item x, Item y)
            {
                return x.type == y.type;
            }

            public int GetHashCode(Item obj)
            {
                return obj.type;
            }
        }

        public Item[] CurrentFrame { get; private set; }
        public Item[] ExtendedItems { get; private set; }
        public Item[] BuybackItems { get; private set; }

        private int currentFrameNum;
        private int numFrames;
        private bool displayBuyback = false;

        private List<ProvisionedSegment> provisions = new List<ProvisionedSegment>();

        private const int FrameCapacity = 38;

        public ShoppingList(ref Item[] vanillaShop)
        {
            currentFrameNum = 0;
            numFrames = 1;

            ExtendedItems = vanillaShop.Where(x => !x.IsAir).ToArray();

            BuybackItems = new Item[FrameCapacity];
            for (int i = 0; i < FrameCapacity; i++)
            {
                BuybackItems[i] = new Item();
            }

            CurrentFrame = new Item[Chest.maxItems];
            CurrentFrame[0] = new Item();
            CurrentFrame[Chest.maxItems - 1] = new Item();
            RefreshFrame();

            vanillaShop = CurrentFrame;
        }

        public void RefreshFrame()
        {
            if (!displayBuyback && BuybackItems.Any(x => !x.IsAir))
                displayBuyback = true;

            int offset = 0;
            Item[] source;
            if (currentFrameNum < numFrames)
            {
                source = ExtendedItems;
                offset = currentFrameNum * FrameCapacity;
            }
            else
            {
                source = BuybackItems;
            }

            for (int i = 1; i <= FrameCapacity; i++)
            {
                int sourceIndex = i + offset - 1;
                if (sourceIndex < source.Length)
                    CurrentFrame[i] = source[sourceIndex];
                else
                    CurrentFrame[i] = new Item();
            }

            if (currentFrameNum == 0)
                CurrentFrame[0].type = 0;
            else
                CurrentFrame[0].SetDefaults(ShopExpander.Instance.ArrowLeft.item.type);

            if (currentFrameNum == numFrames || (!displayBuyback && currentFrameNum == numFrames - 1))
                CurrentFrame[39].type = 0;
            else
                CurrentFrame[39].SetDefaults(ShopExpander.Instance.ArrowRight.item.type);
        }

        public Item[] Provision(int capacity, bool noDistinct)
        {
            ProvisionedSegment items = new ProvisionedSegment(capacity, noDistinct);
            provisions.Add(items);
            return items.items;
        }

        public void Compose()
        {
            ExtendedItems = ExtendedItems.Concat(
                                provisions.Where(x => !x.noDistinct)
                                .SelectMany(x => x.items.Where(y => !y.IsAir)))
                            .Distinct(new ItemSameType())
                            .Concat(
                                provisions.Where(x => x.noDistinct)
                                .SelectMany(x => x.items.Where(y => !y.IsAir)))
                            .ToArray();

            numFrames = (ExtendedItems.Length - 1) / FrameCapacity + 1;
            provisions = new List<ProvisionedSegment>();
            RefreshFrame();
        }

        public void MoveLeft()
        {
            if (currentFrameNum > 0)
                currentFrameNum--;
            RefreshFrame();
        }

        public void MoveRight()
        {
            if (currentFrameNum < numFrames - 1 || (displayBuyback && currentFrameNum < numFrames))
                currentFrameNum++;
            RefreshFrame();
        }

        public void MoveFirst()
        {
            currentFrameNum = 0;
            RefreshFrame();
        }

        public void MoveLast()
        {
            if (displayBuyback)
                currentFrameNum = numFrames;
            else
                currentFrameNum = numFrames - 1;
            RefreshFrame();
        }
    }
}