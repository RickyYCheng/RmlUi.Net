using System;

namespace RmlUiNet
{
    internal sealed class NativeLifetime
    {
        public bool IsAlive { get; set; } = true;
    }

    public interface IRmlBase
    {
    }

    public abstract class RmlBase<T> : IRmlBase, IDisposable
        where T : class
    {
        #region Members

        private IntPtr _pointer;
        private bool _disposed;
        internal NativeLifetime? Lifetime { get; set; }
        internal IntPtr Identity => _pointer;

        public IntPtr NativePtr
        {
            get
            {
                Rml.VerifyAccess();
                ThrowExceptionIfDisposed();
                if (_pointer == IntPtr.Zero)
                    throw new InvalidOperationException($"No native object is attached to {GetType().Name}.");
                return _pointer;
            }
            protected set => _pointer = value;
        }

        #endregion

        #region Methods

        protected RmlBase(IntPtr ptr, bool automaticallyRegisterInCache = false)
        {
            NativePtr = ptr;

            if (automaticallyRegisterInCache) {
                ManuallyRegisterCache(ptr, this);
            }
        }

        protected static T? GetOrCreateCache(IntPtr ptr, RmlInstanceCache.CreateInstance createInstance)
        {
            if (ptr == IntPtr.Zero) {
                return null;
            }
            
            return RmlInstanceCache.Instance.GetOrCreate<T>(ptr, createInstance);
        }

        protected static void ManuallyRegisterCache(IntPtr ptr, IRmlBase instance)
        {
            RmlInstanceCache.Instance.ManuallyRegisterCache(ptr, instance);
        }

        #endregion

        #region IDisposable

        #region Properties

        public bool IsDisposed => _disposed || Lifetime?.IsAlive == false;

        #endregion

        public virtual void Dispose()
        {
            Rml.VerifyAccess();
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) {
                return;
            }

            RmlInstanceCache.Instance.Remove(_pointer, this);
            _disposed = true;
        }

        protected void ThrowExceptionIfDisposed()
        {
            if (IsDisposed) {
                throw new ObjectDisposedException(typeof(T).FullName);
            }
        }

        #endregion
    }
}
