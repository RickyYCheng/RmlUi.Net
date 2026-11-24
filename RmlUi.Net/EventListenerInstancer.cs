using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet
{
    public abstract class EventListenerInstancer : RmlBase<EventListenerInstancer>
    {
        private Native.EventListenerInstancer.OnEventListenerInstancer _onEventListenerInstancer;
        private readonly HashSet<EventListener> _eventListeners = new(ReferenceEqualityComparer.Instance);

        public EventListenerInstancer() : base(IntPtr.Zero)
        {
            Rml.RequireInitialised();
            Rml.VerifyOutsideCallback();
            _onEventListenerInstancer = (value, element) => Rml.InvokeCallback(() => OnEventListenerInstancerInternal(value, element), IntPtr.Zero);

            NativePtr = Native.EventListenerInstancer.Create(_onEventListenerInstancer);
            ManuallyRegisterCache(NativePtr, this);
            Rml.RetainInstancer(this);
        }

        public abstract EventListener OnInstanceElement(string value, Element element);

        internal virtual IntPtr OnEventListenerInstancerInternal(string value, IntPtr element)
        {
            var listener = OnInstanceElement(value, new ElementGeneric(element, false));
            _eventListeners.Add(listener);
            return listener.NativePtr;
        }

        public override void Dispose()
        {
            Rml.VerifyOutsideCallback();
            if (IsDisposed)
                return;
            if (Rml.IsRegistered(this))
                throw new InvalidOperationException("Shut down RmlUi before disposing a registered instancer.");
            foreach (var listener in _eventListeners) {
                listener.Dispose();
            }
            _eventListeners.Clear();
            Native.EventListenerInstancer.Free(NativePtr);
            base.Dispose();
        }
    }
}
