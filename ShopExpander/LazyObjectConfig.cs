﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using Terraria;

namespace ShopExpander
{
    public class LazyObjectConfig<T>
    {
        private readonly ConditionalWeakTable<object, Ref<T>> config = new ConditionalWeakTable<object, Ref<T>>();
        private readonly T defConfig;

        public LazyObjectConfig(T defConfig = default(T))
        {
            this.defConfig = defConfig;
        }

        public void SetValue(object obj, T value)
        {
            Ref<T> valueRef = config.GetOrCreateValue(obj);
            valueRef.Value = value;
        }

        public T GetValue(object obj)
        {
            Ref<T> value;
            if (config.TryGetValue(obj, out value))
                return value.Value;
            else
                return defConfig;
        }
            
        
    }
}
