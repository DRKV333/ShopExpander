using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopExpander
{
    public class LazyObjectConfig<T>
    {
        private readonly Dictionary<object, T> config = new Dictionary<object, T>();
        private readonly T defConfig;

        public LazyObjectConfig(T defConfig = default(T))
        {
            this.defConfig = defConfig;
        }

        public void SetValue(object obj, T value)
        {
            config[obj] = value;
        }

        public T GetValue(object obj)
        {
            T value;
            if (config.TryGetValue(obj, out value))
                return value;
            else
                return defConfig;
        }
            
        
    }
}
