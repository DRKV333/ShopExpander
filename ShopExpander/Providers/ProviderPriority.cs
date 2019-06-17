﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopExpander.Providers
{
    public static class ProviderPriority
    {
        public const int BeforeVanilla = -500;
        public const int Vanilla = 0;
        public const int AfterVanilla = 500;
        public const int Buyback = 1000;
        public const int AfterBuyback = 1500;
    }
}
