using Autofac;
using Autofac.Core;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Shield
{
    /// <summary>
    /// Replaces AutoFac's IIndex for resolving keyed parameters.
    /// Provides loose-coupling for classes that normally would need to use IIndex.<br />
    /// In classes using <see cref="Autofac.Features.Indexed.IIndex{TKey, TValue}"/> You can replace it with <see cref="IReadOnlyDictionary{TKey, TValue}"/>
    /// </summary>
    /// <typeparam name="TKey">AutoFac Keyed parameter "key" value</typeparam>
    /// <typeparam name="TValue">Resolved object based on provided "key"</typeparam>
    public class DependencyDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    {
        private readonly IComponentContext _context;

        public DependencyDictionary(IComponentContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        private static KeyedService GetService(TKey key) => new KeyedService(key, typeof(TValue));

        private IEnumerable<KeyedService> GetServices() =>
            _context.ComponentRegistry.Registrations
                                      .SelectMany(r => r.Services)
                                      .OfType<KeyedService>()
                                      .Where(ks => ks.ServiceKey.GetType() == typeof(TKey) && ks.ServiceType == typeof(TValue));

        public TValue this[TKey key] => (TValue)_context.ResolveService(GetService(key));

        public bool ContainsKey(TKey key) => _context.IsRegisteredWithKey<TValue>(key);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (_context.TryResolveService(GetService(key), out object result))
            {
                value = (TValue)result;
                return true;
            }
            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var service in GetServices())
            {
                yield return new KeyValuePair<TKey, TValue>((TKey)service.ServiceKey, (TValue)_context.ResolveService(GetService((TKey)service.ServiceKey)));
            }
        }

        public IEnumerable<TKey> Keys => GetServices().Select(ks => ks.ServiceKey).Cast<TKey>();
        public IEnumerable<TValue> Values => GetServices().Select(ks => (TValue)_context.ResolveService(GetService((TKey)ks.ServiceKey)));
        public int Count => GetServices().Count();
    }
}