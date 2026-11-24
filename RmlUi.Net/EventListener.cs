using System;
using System.Runtime.InteropServices;

namespace RmlUiNet
{

    public abstract class EventListener : RmlBase<EventListener>
    {

        private Native.Rml.OnProcessEvent _onProcessEvent;
        private Native.Rml.OnAttachEvent _onAttachEvent;
        private Native.Rml.OnDetachEvent _onDetachEvent;
        protected int AttachmentCount { get; private set; }

        public EventListener() : base(IntPtr.Zero)
        {
            _onProcessEvent = ProcessEventInternal;
            _onAttachEvent = AttachEventInternal;
            _onDetachEvent = DetachEventInternal;

            NativePtr = Native.Rml.CreateEventListener(
                _onProcessEvent,
                _onAttachEvent,
                _onDetachEvent
            );

            ManuallyRegisterCache(NativePtr, this);
        }

        protected override void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;
            Rml.VerifyOutsideCallback();
            if (AttachmentCount != 0)
                throw new InvalidOperationException("Detach the event listener before disposing it.");
            Native.Rml.ReleaseEventListener(NativePtr);
            base.Dispose(disposing);
        }

        internal void ProcessEventInternal(IntPtr eventPtr) {
            var lifetime = new NativeLifetime();
            try { Rml.InvokeCallback(() => ProcessEvent(new Event(eventPtr) { Lifetime = lifetime })); }
            finally { lifetime.IsAlive = false; }
        }

        internal void AttachEventInternal(IntPtr elementPtr, string elementType) {
            AttachmentCount++;
            Rml.InvokeCallback(() => OnAttach(Util.GetOrThrowElementByTypeName(elementPtr, elementType)));
        }

        internal void DetachEventInternal(IntPtr elementPtr, string elementType) {
            AttachmentCount--;
            var lifetime = new NativeLifetime();
            try { Rml.InvokeCallback(() => OnDetach(new ElementGeneric(elementPtr, false, track: false) { Lifetime = lifetime })); }
            finally { lifetime.IsAlive = false; }
        }

        public abstract void ProcessEvent(Event ev);

        public virtual void OnAttach(Element element)
        {
        }

        public virtual void OnDetach(Element element)
        {
        }
    }

    public enum EventId : ushort
    {
        Invalid,

        // Core events
        MouseDown,
        MouseScroll,
        MouseOver,
        MouseOut,
        Focus,
        Blur,
        KeyDown,
        KeyUp,
        TextInput,
        MouseUp,
        Click,
        DoubleClick,
        Load,
        Unload,
        Show,
        Hide,
        MouseMove,
        DragMove,
        Drag,
        DragStart,
        DragOver,
        DragDrop,
        DragOut,
        DragEnd,
        HandleDrag,
        Resize,
        Scroll,
        AnimationEnd,
        TransitionEnd,

        // Form control events
        Change,
        Submit,
        TabChange,
    }

    /// <summary>The phase an event is currently being dispatched in.</summary>
    /// <remarks>
    /// The values are bit flags and mirror <c>Rml::EventPhase</c> (see RmlUi's
    /// <c>Include/RmlUi/Core/Event.h</c>). They must not be renumbered: <see cref="Event.Phase"/> casts
    /// the native value straight to this enum.
    /// </remarks>
    [System.Flags]
    public enum EventPhase
    {
        None = 0,
        Capture = 1,
        Target = 2,
        Bubble = 4,
    }
}
