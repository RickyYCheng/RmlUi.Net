using System;
using System.Collections.Generic;
using System.Threading;

namespace RmlUiNet
{
    public static class Rml
    {
        private static readonly object Gate = new();
        private static readonly Native.Rml.OnDestroyed ContextDestroyed = RmlInstanceCache.Instance.ContextDestroyed;
        private static readonly Native.Rml.OnDestroyed ElementDestroyed = RmlInstanceCache.Instance.ElementDestroyed;
        private static readonly Queue<Action> Retired = new();
        private static Thread? _owner;
        private static bool _initialised;
        private static bool _ending;
        private static NativeLifetime? _sessionLifetime;
        [ThreadStatic] private static int _callbackDepth;

        public static bool Initialise()
        {
            VerifyOutsideCallback();
            lock (Gate)
            {
                VerifyOutsideCallback();
                if (_initialised)
                    return false;
                CheckAbi();
                Volatile.Write(ref _owner, Thread.CurrentThread);
                try
                {
                    Native.Rml.ObserveLifetime(ContextDestroyed, ElementDestroyed);
                    _initialised = Native.Rml.Initialise();
                    if (_initialised)
                        _sessionLifetime = new();
                    return _initialised;
                }
                finally
                {
                    if (!_initialised)
                        Volatile.Write(ref _owner, null);
                }
            }
        }

        public static void Shutdown()
        {
            VerifyOutsideCallback();
            lock (Gate)
            {
                VerifyOutsideCallback();
                if (!_initialised)
                {
                    if (_system is not null) Native.Rml.SetSystemInterface(IntPtr.Zero);
                    if (_files is not null) Native.Rml.SetFileInterface(IntPtr.Zero);
                    if (_renderer is not null) Native.Rml.SetRenderInterface(IntPtr.Zero);
                    ReleaseRegistrations();
                    Volatile.Write(ref _owner, null);
                    return;
                }
                _ending = true;
                try
                {
                    Native.Rml.Shutdown();
                    DrainRetired();
                }
                finally
                {
                    if (_sessionLifetime is not null)
                        _sessionLifetime.IsAlive = false;
                    _sessionLifetime = null;
                    _ending = false;
                    _initialised = false;
                    ReleaseRegistrations();
                    Volatile.Write(ref _owner, null);
                }
            }
        }

        internal static void VerifyAccess()
        {
            if (Volatile.Read(ref _owner) is { } owner && owner != Thread.CurrentThread)
                throw new InvalidOperationException("RmlUi must be used on the thread that initialised it.");
        }

        internal static void VerifyOutsideCallback()
        {
            VerifyAccess();
            if (_callbackDepth != 0)
                throw new InvalidOperationException("This operation must wait until the RmlUi callback returns.");
        }

        internal static void RequireInitialised()
        {
            VerifyAccess();
            if (!_initialised || _ending)
                throw new InvalidOperationException("Initialise RmlUi before using it.");
        }

        internal static NativeLifetime SessionLifetime =>
            _sessionLifetime ?? throw new InvalidOperationException("RmlUi is not initialised.");

        internal static void CheckAbi()
        {
            VerifyAccess();
            try
            {
                int actual = Native.Rml.BindingAbiVersion();
                if (actual != 2)
                    throw new InvalidOperationException($"RmlUiNative ABI {actual} does not match managed ABI 2.");
            }
            catch (EntryPointNotFoundException error)
            {
                throw new InvalidOperationException("RmlUiNative does not expose an ABI version. Install the native library from the matching RmlUi.Net package.", error);
            }
        }

        internal static void Retire(Action action) => Retired.Enqueue(action);

        internal static void DrainRetired()
        {
            if (_callbackDepth != 0)
                return;
            while (Retired.TryDequeue(out var action))
                action();
        }

        internal static CallbackScope EnterCallback()
        {
            _callbackDepth++;
            return new();
        }

        internal readonly struct CallbackScope : IDisposable
        {
            public void Dispose() => _callbackDepth--;
        }

        public static event Action<Exception>? CallbackError;

        internal static void ReportCallbackError(Exception error)
        {
            try { CallbackError?.Invoke(error); }
            catch { }
        }

        internal static T InvokeCallback<T>(Func<T> callback, T fallback)
        {
            using var scope = EnterCallback();
            try { return callback(); }
            catch (Exception error) { ReportCallbackError(error); return fallback; }
        }

        internal static void InvokeCallback(Action callback)
        {
            using var scope = EnterCallback();
            try { callback(); }
            catch (Exception error) { ReportCallbackError(error); }
        }

        private static SystemInterface? _system;
        private static RenderInterface? _renderer;
        private static FileInterface? _files;
        private static readonly HashSet<RenderInterface> Renderers = new(ReferenceEqualityComparer.Instance);
        private static readonly HashSet<Plugin> Plugins = new(ReferenceEqualityComparer.Instance);
        private static readonly HashSet<IRmlBase> Instancers = new(ReferenceEqualityComparer.Instance);

        private static void ReleaseRegistrations()
        {
            _system = null;
            _renderer = null;
            _files = null;
            Renderers.Clear();
            Plugins.Clear();
            Instancers.Clear();
        }

        internal static bool IsRegistered(IRmlBase instance) =>
            ReferenceEquals(instance, _system) || ReferenceEquals(instance, _files) ||
            (instance is RenderInterface renderer && Renderers.Contains(renderer)) ||
            (instance is Plugin plugin && Plugins.Contains(plugin)) || Instancers.Contains(instance);

        internal static void RetainInstancer(IRmlBase instancer) => Instancers.Add(instancer);

        internal static void RetainRenderer(RenderInterface? renderer)
        {
            if (renderer is not null)
                Renderers.Add(renderer);
        }

        public static void SetSystemInterface(SystemInterface systemInterface)
        {
            VerifyOutsideCallback();
            lock (Gate)
            {
                VerifyOutsideCallback();
                ArgumentNullException.ThrowIfNull(systemInterface);
                CheckAbi();
                Native.Rml.SetSystemInterface(systemInterface.NativePtr);
                _system = systemInterface;
                Volatile.Write(ref _owner, Thread.CurrentThread);
            }
        }

        public static void SetRenderInterface(RenderInterface renderInterface)
        {
            VerifyOutsideCallback();
            lock (Gate)
            {
                VerifyOutsideCallback();
                ArgumentNullException.ThrowIfNull(renderInterface);
                CheckAbi();
                Native.Rml.SetRenderInterface(renderInterface.NativePtr);
                if (!_initialised && _renderer is not null)
                    Renderers.Remove(_renderer);
                _renderer = renderInterface;
                RetainRenderer(renderInterface);
                Volatile.Write(ref _owner, Thread.CurrentThread);
            }
        }

        public static void SetFileInterface(FileInterface fileInterface)
        {
            VerifyOutsideCallback();
            lock (Gate)
            {
                VerifyOutsideCallback();
                ArgumentNullException.ThrowIfNull(fileInterface);
                CheckAbi();
                Native.Rml.SetFileInterface(fileInterface.NativePtr);
                _files = fileInterface;
                Volatile.Write(ref _owner, Thread.CurrentThread);
            }
        }

        /// <summary>
        /// Adds a new font face to the font engine. The face's family, style and weight will be determined from the face itself.
        /// </summary>
        /// <param name="fileName">The file to load the face from.</param>
        /// <param name="fallbackFace">True to use this font face for unknown characters in other font faces.</param>
        /// <param name="weight">The weight to load when the font face contains multiple weights, otherwise the weight to register the font as. By default it loads all found font weights.</param>
        /// <returns>True if the face was loaded successfully, false otherwise.</returns>
        public static bool LoadFontFace(string fileName, bool fallbackFace = false, FontWeight weight = FontWeight.Auto)
        {
            RequireInitialised();
            ArgumentNullException.ThrowIfNull(fileName);
            return Native.Rml.LoadFontFace(fileName, fallbackFace, weight);
        }

        /// <summary>
        /// Adds a new font face to the font engine and registers it under an explicit family,
        /// style and weight instead of the ones read from the face itself.
        /// </summary>
        /// <param name="fileName">The file to load the face from.</param>
        /// <param name="family">The family to register the font as.</param>
        /// <param name="style">The style to register the font as.</param>
        /// <param name="weight">The weight to load, or to register the face as. <see cref="FontWeight.Auto"/> loads all found weights.</param>
        /// <param name="fallbackFace">True to use this font face for unknown characters in other font faces.</param>
        /// <param name="faceIndex">The index of the font face within a font collection.</param>
        /// <returns>True if the face was loaded successfully, false otherwise.</returns>
        public static bool LoadFontFace(string fileName, string family, FontStyle style,
            FontWeight weight = FontWeight.Auto, bool fallbackFace = false, int faceIndex = 0)
        {
            RequireInitialised();
            ArgumentNullException.ThrowIfNull(fileName);
            ArgumentNullException.ThrowIfNull(family);
            return Native.Rml.LoadFontFaceEx(fileName, family, style, weight, fallbackFace, faceIndex);
        }

        public static EventId RegisterEventType(string type, bool interruptible, bool bubbles, DefaultActionPhase defaultActionPhase) {
            RequireInitialised();
            ArgumentException.ThrowIfNullOrEmpty(type);
            return Native.Rml.RegisterEventType(type, interruptible, bubbles, defaultActionPhase);
        }

        public static Context? CreateContext(string name, Vector2i dimensions, RenderInterface? renderInterface = null)
        {
            VerifyOutsideCallback();
            RequireInitialised();
            ArgumentException.ThrowIfNullOrEmpty(name);
            return Context.Create(name, dimensions, renderInterface);
        }

        public static bool RemoveContext(string name) {
            VerifyOutsideCallback();
            RequireInitialised();
            ArgumentException.ThrowIfNullOrEmpty(name);
            bool removed = Native.Rml.RemoveContext(name);
            DrainRetired();
            return removed;
        }

        public static bool DestroyDetachedElement(Element element)
        {
            VerifyOutsideCallback();
            RequireInitialised();
            ArgumentNullException.ThrowIfNull(element);
            bool destroyed = Native.Element.DestroyDetached(element.NativePtr);
            DrainRetired();
            return destroyed;
        }

        public static void RegisterPlugin(Plugin plugin) {
            VerifyOutsideCallback();
            RequireInitialised();
            ArgumentNullException.ThrowIfNull(plugin);
            var pointer = plugin.NativePtr;
            if (Plugins.Add(plugin))
                Native.Rml.RegisterPlugin(pointer);
        }

        public static void Log(string message, LogType type = LogType.Always) {
            VerifyAccess();
            ArgumentNullException.ThrowIfNull(message);
            Native.Rml.Log(type, message);
        }

        public static RenderInterface GetRenderInterface() {
            RequireInitialised();
            return RenderInterface.Create(Native.Rml.GetRenderInterface()) ?? throw new Exception("Failed to create render interface wrapper");
        }
    }
}
