using System;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

namespace RmlUiNet {
    public abstract class ElementCustom : Element<ElementCustom> {
        private Native.ElementCustom.OnRender? _onRender;

        public ElementCustom(string tagName) : base(IntPtr.Zero, false) {
            Rml.RequireInitialised();
            ArgumentException.ThrowIfNullOrEmpty(tagName);
            _onRender = IsImplemented(nameof(OnRender)) ? () => Rml.InvokeCallback(OnRender) : null;

            NativePtr = Native.ElementCustom.Create(tagName, _onRender);
            Lifetime = RmlInstanceCache.Instance.TrackElement(NativePtr);
            RmlInstanceCache.Instance.RetainElementOwner(NativePtr, this);
        }

        public virtual void OnRender() {

        }

        /// <summary>
        /// check if this method is implemented by the inheriting class
        /// </summary>
        private bool IsImplemented(string methodName) {
            return GetType().GetMethod(methodName)?.DeclaringType != typeof(ElementCustom);
        }

        public override void Dispose() {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (GetParentNode() is not null)
                throw new InvalidOperationException("Do not dispose an element owned by a parent.");
            Native.ElementCustom.Free(NativePtr);
            base.Dispose();
        }
    }
}
