using System.Collections.Generic;

namespace Fei.Is.Api.Features.Devices.Services;

public sealed class RecentDeviceIdCache
{
    private const int Capacity = 10000;
    private static readonly TimeSpan EntryTtl = TimeSpan.FromHours(1);

    private readonly object _lock = new();
    private readonly Dictionary<string, LinkedListNode<CacheEntry>> _entries = new(Capacity, StringComparer.Ordinal);
    private readonly LinkedList<CacheEntry> _lru = new();

    public bool TryGet(string deviceAccessToken, out Guid deviceId)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(deviceAccessToken, out var node))
            {
                deviceId = Guid.Empty;
                return false;
            }

            if (node.Value.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                _lru.Remove(node);
                _entries.Remove(deviceAccessToken);
                deviceId = Guid.Empty;
                return false;
            }

            _lru.Remove(node);
            _lru.AddFirst(node);
            deviceId = node.Value.DeviceId;
            return true;
        }
    }

    public void Set(string deviceAccessToken, Guid deviceId)
    {
        lock (_lock)
        {
            if (_entries.TryGetValue(deviceAccessToken, out var existingNode))
            {
                existingNode.Value = new CacheEntry(deviceAccessToken, deviceId, DateTimeOffset.UtcNow.Add(EntryTtl));
                _lru.Remove(existingNode);
                _lru.AddFirst(existingNode);
                return;
            }

            var node = new LinkedListNode<CacheEntry>(new CacheEntry(deviceAccessToken, deviceId, DateTimeOffset.UtcNow.Add(EntryTtl)));
            _lru.AddFirst(node);
            _entries[deviceAccessToken] = node;

            while (_entries.Count > Capacity)
            {
                var last = _lru.Last;
                if (last == null)
                {
                    break;
                }

                _lru.RemoveLast();
                _entries.Remove(last.Value.DeviceAccessToken);
            }
        }
    }

    public void Remove(string deviceAccessToken)
    {
        lock (_lock)
        {
            if (!_entries.TryGetValue(deviceAccessToken, out var node))
            {
                return;
            }

            _lru.Remove(node);
            _entries.Remove(deviceAccessToken);
        }
    }

    private readonly record struct CacheEntry(string DeviceAccessToken, Guid DeviceId, DateTimeOffset ExpiresAt);
}
