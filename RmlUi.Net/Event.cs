using RmlUiNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet {

    public class Event : RmlBase<Event> {
        private Dictionary<string, object>? _parameters = null;
        /// <summary>
        /// The event id.
        /// </summary>
        public EventId Id => (EventId)Native.Event.GetId(NativePtr);

        /// <summary>
        /// The event's type name, e.g. <c>click</c> or a custom name registered through
        /// <see cref="Rml.RegisterEventType"/>.
        /// </summary>
        public string Type
        {
            get
            {
                var ptr = Native.Event.GetEventType(NativePtr);
                return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
            }
        }

        /// <summary>
        /// The current propagation phase.
        /// </summary>
        public EventPhase Phase => (EventPhase)Native.Event.GetPhase(NativePtr);

        /// <summary>
        /// The current element in the propagation.
        /// </summary>
        public Element? CurrentElement {
            get {
                var elementType = Marshal.PtrToStringAnsi(
                    Native.Event.GetCurrentElement(NativePtr, out var elementPtr)
                );
                if (elementType is null) return null;
                return Util.GetElementByTypeName(elementPtr, elementType);
            }
        }

        /// <summary>
        /// The target element of this event.
        /// </summary>
        public Element? TargetElement {
            get {
                var elementType = Marshal.PtrToStringAnsi(
                    Native.Event.GetTargetElement(NativePtr, out var elementPtr)
                );
                if (elementType is null) return null;
                return Util.GetElementByTypeName(elementPtr, elementType);
            }
        }

        /// <summary>
        /// True if the event can be interrupted, that is, stopped from propagating.
        /// </summary>
        public bool IsInterruptible => Native.Event.IsInterruptible(NativePtr);

        /// <summary>
        /// True if the event is still propagating.
        /// </summary>
        public bool IsPropagating => Native.Event.IsPropagating(NativePtr);

        /// <summary>
        /// True if the event is still immediate propagating.
        /// </summary>
        public bool IsImmediatePropagating => Native.Event.IsImmediatePropagating(NativePtr);

        public Event(IntPtr ptr, bool automaticallyRegisterInCache = false) : base(ptr, automaticallyRegisterInCache) {
        }

        public Dictionary<string, object> Parameters {
            get
            {
                if (_parameters is not null) return _parameters;
                var snapshot = new Dictionary<string, object>(StringComparer.Ordinal);
                var parameters = Native.Event.GetParameters(NativePtr);
                var keys = Native.RmlDictionary.RetrieveKeys(parameters);
                foreach (var key in keys)
                {
                    var pointer = Native.RmlDictionary.Get(parameters, key);
                    if (pointer == IntPtr.Zero)
                        continue;
                    using var variant = new Variant(pointer);
                    if (variant.Value is { } value)
                        snapshot.TryAdd(key, value);
                }
                return _parameters = snapshot;
            }
        }

        /// <summary>
        /// Stops propagation of the event if it is interruptible, but finish all listeners on the current element.
        /// </summary>
        public void StopPropagation() {
            Native.Event.StopPropagation(NativePtr);
        }

        /// <summary>
        /// Stops propagation of the event if it is interruptible, including to any other listeners on the current element.
        /// </summary>
        public void StopImmediatePropagation() {
            Native.Event.StopImmediatePropagation(NativePtr);
        }
    }
}
