using System;
using Deadzone.Data;
using Deadzone.SaveSystem;

namespace Deadzone.Economy
{
    /// <summary>
    /// The only place resources are changed. Rejects invalid amounts, never goes negative,
    /// clamps to capacity and raises Changed. Backed directly by PlayerData so it is always saved.
    /// </summary>
    public sealed class ResourceWallet
    {
        private readonly PlayerData data;
        private readonly Func<ResourceType, long> capacityProvider;

        public event Action<ResourceType, long> Changed;

        public ResourceWallet(PlayerData data, Func<ResourceType, long> capacityProvider)
        {
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.capacityProvider = capacityProvider ?? throw new ArgumentNullException(nameof(capacityProvider));
        }

        public long Get(ResourceType type)
        {
            ResourceAmount entry = Find(type);
            return entry == null ? 0 : entry.amount;
        }

        public long GetCapacity(ResourceType type) => capacityProvider(type);

        public bool CanAfford(ResourceType type, long amount) => amount > 0 && Get(type) >= amount;

        /// <summary>Returns the amount actually added (0 for invalid input or when full).</summary>
        public long Add(ResourceType type, long amount)
        {
            if (amount <= 0) return 0;

            ResourceAmount entry = GetOrCreate(type);
            long room = Math.Max(0, capacityProvider(type) - entry.amount);
            long added = Math.Min(amount, room);
            if (added <= 0) return 0;

            entry.amount += added;
            Changed?.Invoke(type, entry.amount);
            return added;
        }

        /// <summary>Spends the full amount or nothing. False for invalid amounts or insufficient balance.</summary>
        public bool TrySpend(ResourceType type, long amount)
        {
            if (amount <= 0) return false;

            ResourceAmount entry = Find(type);
            if (entry == null || entry.amount < amount) return false;

            entry.amount -= amount;
            Changed?.Invoke(type, entry.amount);
            return true;
        }

        private ResourceAmount Find(ResourceType type)
        {
            foreach (ResourceAmount r in data.resources)
            {
                if (r.type == type) return r;
            }
            return null;
        }

        private ResourceAmount GetOrCreate(ResourceType type)
        {
            ResourceAmount entry = Find(type);
            if (entry == null)
            {
                entry = new ResourceAmount { type = type, amount = 0 };
                data.resources.Add(entry);
            }
            return entry;
        }
    }
}
