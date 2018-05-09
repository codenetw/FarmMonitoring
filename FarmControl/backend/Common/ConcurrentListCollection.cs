using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace FarmMonitoring.backend.Common
{
    public class SpecificConcurrentDictionary<TValue> : ConcurrentDictionary<string, TValue> , IEnumerable<KeyValuePair<string, TValue>>
    {
        public void Add(string key, TValue value) => TryAdd(key, value);
        public new IEnumerable<TValue> this[string key] => this.Where(x => x.Key.StartsWith(key, true, CultureInfo.InvariantCulture)).Select(x => x.Value);
        public void Remove(string key) => TryRemove(key, out TValue _);
    }
}
