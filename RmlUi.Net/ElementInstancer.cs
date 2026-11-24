using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet {
    public abstract class ElementInstancer : RmlBase<ElementInstancer> {
        private Native.ElementInstancer.OnInstanceElement _onInstanceElement;
        private Native.ElementInstancer.OnReleaseElement _onReleaseElement;

        public ElementInstancer(string tag) : base(IntPtr.Zero) {
            Rml.RequireInitialised();
            Rml.VerifyOutsideCallback();
            ArgumentException.ThrowIfNullOrEmpty(tag);
            _onInstanceElement = (parent, name, attributes) => Rml.InvokeCallback(() => OnInstanceElementInternal(parent, name, attributes), IntPtr.Zero);
            _onReleaseElement = ptr => Rml.InvokeCallback(() => OnReleaseElementInternal(ptr));

            NativePtr = Native.ElementInstancer.Create(tag, _onInstanceElement, _onReleaseElement);

            ManuallyRegisterCache(NativePtr, this);
            Rml.RetainInstancer(this);
        }

        public abstract IntPtr OnInstanceElement(Element parent, string tag, XMLAttributes attributes);

        public abstract void OnReleaseElement(Element element);

        internal virtual IntPtr OnInstanceElementInternal(IntPtr parentElement, string tag, IntPtr xmlAttributes) {
            using var attributes = new XMLAttributes(xmlAttributes, false);
            return OnInstanceElement(new ElementGeneric(parentElement, false), tag, attributes);
        }

        internal virtual void OnReleaseElementInternal(IntPtr elementPtr) {
            using var element = new ElementGeneric(elementPtr, false, track: false);
            OnReleaseElement(element);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi before disposing a registered instancer.");
            Native.ElementInstancer.Free(NativePtr);
            base.Dispose(disposing);
        }
    }
}
