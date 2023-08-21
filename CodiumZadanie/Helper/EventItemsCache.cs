using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Caching;

namespace CodiumZadanie.Helper
{
    internal class EventItemsCache
    {
        private static ObjectCache Cache = MemoryCache.Default;

        internal static EventData GetAvailableEvent(EventData eventData)
        {        
            if (Cache.Contains(eventData.ProviderEventID.ToString()))
                return (EventData)Cache.Get(eventData.ProviderEventID.ToString());
            else
            {
                // Store data in the cache
                CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();
                cacheItemPolicy.AbsoluteExpiration = DateTime.Now.AddHours(1.0);
                Cache.Add(eventData.ProviderEventID.ToString(), eventData, cacheItemPolicy);

                return null;
            }
        }
    }
}
