using System;
using System.Numerics;
using static RmlUiNet.Native.RenderInterface;

namespace RmlUiNet
{
    public unsafe class RenderInterface : RmlBase<RenderInterface>
    {
        private Native.RenderInterface.OnCompileGeometry _onCompileGeometry = null!;
        private Native.RenderInterface.OnRenderGeometry _onRenderGeometry = null!;
        private Native.RenderInterface.OnReleaseGeometry _onReleaseGeometry = null!;
        private Native.RenderInterface.OnLoadTexture _onLoadTexture = null!;
        private Native.RenderInterface.OnGenerateTexture _onGenerateTexture = null!;
        private Native.RenderInterface.OnReleaseTexture _onReleaseTexture = null!;
        private Native.RenderInterface.OnEnableScissorRegion _onEnableScissorRegion = null!;
        private Native.RenderInterface.OnSetScissorRegion _onSetScissorRegion = null!;
        private Native.RenderInterface.OnSetTransform _onSetTransform = null!;

        internal static RenderInterface? Create(IntPtr nativePtr) {
            return GetOrCreateCache(nativePtr, ptr => new RenderInterface(ptr));
        }

        internal RenderInterface(IntPtr nativePtr) : base(nativePtr) {
        
        }

        public RenderInterface() : base(IntPtr.Zero)
        {
            Rml.CheckAbi();
            _onCompileGeometry = (vertices, vertexCount, indices, indexCount) =>
                Rml.InvokeCallback(() => CompileGeometry(vertices, vertexCount, indices, indexCount), IntPtr.Zero);
            _onRenderGeometry = (geometry, translation, texture) => Rml.InvokeCallback(() => RenderGeometry(geometry, translation, texture));
            _onReleaseGeometry = handle => Rml.InvokeCallback(() => ReleaseGeometry(handle));
            _onLoadTexture = LoadTextureCallback;
            _onGenerateTexture = (source, bytes, dimensions) =>
                Rml.InvokeCallback(() => GenerateTexture(source, bytes, dimensions), IntPtr.Zero);
            _onReleaseTexture = handle => Rml.InvokeCallback(() => ReleaseTexture(handle));
            _onEnableScissorRegion = enable => Rml.InvokeCallback(() => EnableScissorRegion(enable));
            _onSetScissorRegion = (x, y, w, h) => Rml.InvokeCallback(() => SetScissorRegion(x, y, w, h));
            _onSetTransform = transform => Rml.InvokeCallback(() => SetTransform(transform));

            NativePtr = Native.RenderInterface.Create(
                _onCompileGeometry,
                _onRenderGeometry,
                _onReleaseGeometry,
                _onLoadTexture,
                _onGenerateTexture,
                _onReleaseTexture,
                _onEnableScissorRegion,
                _onSetScissorRegion,
                _onSetTransform
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        private IntPtr LoadTextureCallback(ref Vector2i dimensions, string source)
        {
            using var scope = Rml.EnterCallback();
            try { return LoadTexture(ref dimensions, source); }
            catch (Exception error)
            {
                Rml.ReportCallbackError(error);
                dimensions = default;
                return IntPtr.Zero;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi before disposing a renderer used by it.");
            if (_onCompileGeometry is not null)
                Native.RenderInterface.Free(NativePtr);
            base.Dispose(disposing);
        }

        public virtual IntPtr CompileGeometry(Vertex* vertices, int vertexCount, int* indices, int indexCount) {
            return IntPtr.Zero;
        }

        public virtual void RenderGeometry(IntPtr geometry, Vector2f translation, IntPtr texture)
        {

        }

        public virtual void ReleaseGeometry(IntPtr geometry) {

        }

        public virtual IntPtr LoadTexture(ref Vector2i textureDimensions, string source)
        {
            textureDimensions = default;
            return IntPtr.Zero;
        }

        public virtual IntPtr GenerateTexture(byte* source, int numBytes, Vector2i dimensions)
        {
            return IntPtr.Zero;
        }

        public virtual void ReleaseTexture(IntPtr textureHandle)
        {

        }

        public virtual void EnableScissorRegion(bool enable)
        {

        }

        public virtual void SetScissorRegion(int x, int y, int width, int height)
        {

        }

        public virtual void SetTransform(IntPtr transform)
        {

        }

        public virtual void Test() {
            Native.RenderInterface.Test(NativePtr);
        }
    }
}
