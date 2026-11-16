using System;
using System.Collections.Generic;

namespace RmlUiNet
{
    /// <summary>
    /// Maps native element/event pointers to the wrapper handed out for them, so repeated
    /// lookups return the same managed instance.
    /// </summary>
    /// <remarks>
    /// Entries are keyed by address only, so the cache must treat a hit as a hint rather than a
    /// fact: RmlUi frees elements and can hand the same address to a different type later.
    /// <see cref="GetOrCreate{T}"/> therefore rebuilds a wrapper whose cached type does not
    /// match the requested one. Callers that destroy a whole tree (removing a context) should
    /// call <see cref="Clear"/> so stale wrappers do not outlive their elements.
    /// </remarks>
    public class RmlInstanceCache
    {
        #region Members

        private readonly Dictionary<IntPtr, IRmlBase> _cache = new Dictionary<IntPtr, IRmlBase>();

        #endregion

        #region Methods

        public T Get<T>(IntPtr ptr)
            where T : class 
        {
            if (ptr == IntPtr.Zero || !_cache.ContainsKey(ptr)) {
                throw new Exception("Pointer not found in cache");
            }

            return (_cache[ptr] as T) ?? throw new InvalidCastException($"Cached instance of type {_cache[ptr].GetType()} is not assignable to {typeof(T)}");
        }

        public T GetOrCreate<T>(IntPtr ptr, CreateInstance createInstance)
            where T : class 
        {
            if (ptr == IntPtr.Zero) {
                return null!;
            }

            if (!_cache.TryGetValue(ptr, out IRmlBase? value)) {
                value = createInstance(ptr) ?? throw new Exception($"Failed to create instance of {typeof(T)} for pointer {ptr:X}");
                _cache.Add(ptr, value);
            }
            // The cache is keyed by address alone, and RmlUi recycles freed elements. When a
            // new element of a different type lands on an address a previous wrapper is still
            // cached for, that wrapper cannot represent it; rebuilding it with the factory for
            // the requested type is the only way to get a correct wrapper back. Checking for
            // "not assignable" rather than "is ElementGeneric" also covers the case where the
            // stale entry was a concrete wrapper (e.g. a Select address reused by a TextArea).
            else if (value is not T) {
                _cache.Remove(ptr);
                value = createInstance(ptr) ?? throw new Exception($"Failed to create instance of {typeof(T)} for pointer {ptr:X}");
                _cache.Add(ptr, value);
            }

            return (value as T) ?? throw new InvalidCastException($"Cached instance of type {value.GetType()} is not assignable to {typeof(T)}");
        }

        public void ManuallyRegisterCache(IntPtr ptr, IRmlBase instance)
        {
            if (_cache.ContainsKey(ptr)) {
                throw new InvalidOperationException("Instance already registered");
            }

            _cache.Add(ptr, instance);
        }

        public void Remove(IntPtr cameraPtr)
        {
            if (_cache.ContainsKey(cameraPtr)) {
                _cache.Remove(cameraPtr);
            }
        }

        internal void Clear()
        {
            _cache.Clear();
        }

        #endregion

        public delegate IRmlBase CreateInstance(IntPtr ptr);

        #region Singleton

        public static RmlInstanceCache Instance { get; }

        static RmlInstanceCache()
        {
            Instance = new RmlInstanceCache();
        }

        #endregion
    }
}
