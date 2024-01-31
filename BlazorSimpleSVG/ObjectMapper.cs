using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSimpleSVG
{
    public class ObjectRef<T>
    {
        public string Id { get; set; }
        public T Instance { get; set; }
        public DateTime? Expiration { get; set; }
    }

    public class ObjectMapper<T>
    {
        public ConcurrentDictionary<string, ObjectRef<T>> m_objects = new();
        public TimeSpan m_expiration = TimeSpan.FromMinutes(120);
        public void PurgeExpired()
        {
            var now = DateTime.Now;
            var expired = m_objects.Where(o => o.Value.Expiration.HasValue && o.Value.Expiration.Value < now).Select(o => o.Key).ToList();
            foreach (var key in expired)
            {
                m_objects.TryRemove(key, out _);
            }
        }
        public void AddOrUpdate(ObjectRef<T> obj)
        {
            PurgeExpired();
            if (!obj.Expiration.HasValue)
                obj.Expiration = DateTime.Now + m_expiration;
            m_objects.AddOrUpdate(obj.Id, obj, (key, oldValue) => obj);
        }

        public T Get(string id)
        {
            if (m_objects.TryGetValue(id, out var obj))
                return obj.Instance;
            else
                return default(T);
        }
    }
}
