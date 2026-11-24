using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet {
    public abstract class Plugin : RmlBase<Plugin> {
        private Native.Plugin.OnInitialise _onInitialise;
        private Native.Plugin.OnShutdown _onShutdown;
        private Native.Plugin.OnDocumentOpen _onDocumentOpen;
        private Native.Plugin.OnDocumentLoad _onDocumentLoad;
        private Native.Plugin.OnDocumentUnload _onDocumentUnload;
        private Native.Plugin.OnContextCreate _onContextCreate;
        private Native.Plugin.OnContextDestroy _onContextDestroy;
        private Native.Plugin.OnElementCreate _onElementCreate;
        private Native.Plugin.OnElementDestroy _onElementDestroy;

        public Plugin() : base(IntPtr.Zero) {
            _onInitialise = () => Rml.InvokeCallback(OnInitialise);
            _onShutdown = () => Rml.InvokeCallback(OnShutdown);
            _onDocumentOpen = (ptr, path) => Rml.InvokeCallback(() => OnDocumentOpenInternal(ptr, path));
            _onDocumentLoad = ptr => Rml.InvokeCallback(() => OnDocumentLoadInternal(ptr));
            _onDocumentUnload = ptr => Rml.InvokeCallback(() => OnDocumentUnloadInternal(ptr));
            _onContextCreate = ptr => Rml.InvokeCallback(() => OnContextCreateInternal(ptr));
            _onContextDestroy = ptr => Rml.InvokeCallback(() => OnContextDestroyInternal(ptr));
            _onElementCreate = ptr => Rml.InvokeCallback(() => OnElementCreateInternal(ptr));
            _onElementDestroy = ptr => Rml.InvokeCallback(() => OnElementDestroyInternal(ptr));

            NativePtr = Native.Plugin.Create(
                _onInitialise,
                _onShutdown,
                _onDocumentOpen,
                _onDocumentLoad,
                _onDocumentUnload,
                _onContextCreate,
                _onContextDestroy,
                _onElementCreate,
                _onElementDestroy
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        public virtual void OnInitialise() {

        }

        public virtual void OnShutdown() {

        }

        public virtual void OnDocumentOpen(Context context, string documentPath) {

        }

        public virtual void OnDocumentLoad(ElementDocument context) {

        }

        public virtual void OnDocumentUnload(ElementDocument context) {

        }

        public virtual void OnContextCreate(Context context) {

        }

        public virtual void OnContextDestroy(Context context) {

        }

        public virtual void OnElementCreate(Element context) {

        }

        public virtual void OnElementDestroy(Element context) {

        }

        internal virtual void OnDocumentOpenInternal(IntPtr contextPtr, string documentPath) {
            OnDocumentOpen(RmlInstanceCache.Instance.GetOrCreate<Context>(contextPtr, contextPtr => new Context(contextPtr)), documentPath);
        }

        internal virtual void OnDocumentLoadInternal(IntPtr documentPtr) {
            OnDocumentLoad(ElementDocument.Create(documentPtr) ?? throw new Exception("Failed to create document wrapper"));
        }

        internal virtual void OnDocumentUnloadInternal(IntPtr documentPtr) {
            OnDocumentUnload(ElementDocument.Create(documentPtr) ?? throw new Exception("Failed to create document wrapper"));
        }

        internal virtual void OnContextCreateInternal(IntPtr contextPtr) {
            OnContextCreate(RmlInstanceCache.Instance.GetOrCreate<Context>(contextPtr, contextPtr => new Context(contextPtr)));
        }

        internal virtual void OnContextDestroyInternal(IntPtr contextPtr) {
            var lifetime = new NativeLifetime();
            try { OnContextDestroy(new Context(contextPtr) { Lifetime = lifetime }); }
            finally { lifetime.IsAlive = false; }
        }

        internal virtual void OnElementCreateInternal(IntPtr elementPtr) {
            OnElementCreate(new ElementGeneric(elementPtr, false));
        }

        internal virtual void OnElementDestroyInternal(IntPtr elementPtr) {
            var lifetime = new NativeLifetime();
            try { OnElementDestroy(new ElementGeneric(elementPtr, false, track: false) { Lifetime = lifetime }); }
            finally { lifetime.IsAlive = false; }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi before disposing a registered plugin.");
            Native.Plugin.Free(NativePtr);
            base.Dispose(disposing);
        }
    }
}
