using System;
using System.Collections.Generic;

namespace RmlUiNet;

public class RmlInstanceCache
{
    private readonly Dictionary<IntPtr, IRmlBase> _cache = new();
    private readonly Dictionary<IntPtr, NativeLifetime> _elements = new();
    private readonly Dictionary<IntPtr, Element> _elementOwners = new();
    private readonly object _gate = new();

    public static RmlInstanceCache Instance { get; } = new();

    public delegate IRmlBase CreateInstance(IntPtr ptr);

    public T Get<T>(IntPtr ptr) where T : class
    {
        lock (_gate)
            return _cache[ptr] as T ?? throw new InvalidCastException();
    }

    public T GetOrCreate<T>(IntPtr ptr, CreateInstance createInstance) where T : class
    {
        if (ptr == IntPtr.Zero)
            return null!;
        lock (_gate)
        {
            if (_cache.TryGetValue(ptr, out var cached) && cached is T compatible)
                return compatible;
            var created = createInstance(ptr);
            T typed = created as T ?? throw new InvalidCastException($"Could not wrap native object as {typeof(T).Name}.");
            _cache[ptr] = created;
            return typed;
        }
    }

    public void ManuallyRegisterCache(IntPtr ptr, IRmlBase instance)
    {
        if (ptr == IntPtr.Zero)
            throw new ArgumentException("A native object must have a nonzero address.", nameof(ptr));
        lock (_gate)
            _cache.Add(ptr, instance);
    }

    public void Remove(IntPtr pointer)
    {
        lock (_gate)
            _cache.Remove(pointer);
    }

    internal void Remove(IntPtr pointer, IRmlBase instance)
    {
        lock (_gate)
            if (_cache.TryGetValue(pointer, out var cached) && ReferenceEquals(cached, instance))
                _cache.Remove(pointer);
    }

    internal NativeLifetime TrackElement(IntPtr pointer)
    {
        lock (_gate)
        {
            if (!_elements.TryGetValue(pointer, out var lifetime))
                _elements.Add(pointer, lifetime = new());
            return lifetime;
        }
    }

    internal void ElementDestroyed(IntPtr pointer)
    {
        lock (_gate)
        {
            if (_elements.Remove(pointer, out var lifetime))
                lifetime.IsAlive = false;
            _elementOwners.Remove(pointer);
            _cache.Remove(pointer);
        }
    }

    internal void RetainElementOwner(IntPtr pointer, Element owner)
    {
        lock (_gate)
            _elementOwners.Add(pointer, owner);
    }

    internal Element? GetElementOwner(IntPtr pointer)
    {
        lock (_gate)
            return _elementOwners.GetValueOrDefault(pointer);
    }

    internal void ContextDestroyed(IntPtr pointer)
    {
        lock (_gate)
            if (_cache.Remove(pointer, out var wrapper) && wrapper is Context context)
                context.OnNativeDestroyed();
    }
}
