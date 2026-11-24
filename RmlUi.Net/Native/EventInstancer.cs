using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet.Native
{

    internal static class EventInstancer
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_EventInstancer_New")]
        public static extern IntPtr Create(OnInstanceEvent onInstanceEvent);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_EventInstancer_Free")]
        public static extern void Free(IntPtr eventListenerInstancer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnInstanceEvent(IntPtr element, [MarshalAs(UnmanagedType.LPUTF8Str)] string elementType, EventId eventId, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr parameters, [MarshalAs(UnmanagedType.I1)] bool interruptible);
    }
}
