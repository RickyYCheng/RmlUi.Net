using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet.Native
{

    internal static class EventListenerInstancer
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_EventListenerInstancer_New")]
        public static extern IntPtr Create(OnEventListenerInstancer eventListenerInstancer);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_EventListenerInstancer_Free")]
        public static extern void Free(IntPtr eventListenerInstancer);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr OnEventListenerInstancer([MarshalAs(UnmanagedType.LPUTF8Str)] string value, IntPtr element);
    }
}
