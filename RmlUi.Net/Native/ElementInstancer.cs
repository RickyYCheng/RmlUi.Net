using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RmlUiNet.Native {

    internal static class ElementInstancer {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementInstancer_Free")]
        public static extern void Free(IntPtr instance);
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementInstancer_New")]
        public static extern IntPtr Create([MarshalAs(UnmanagedType.LPUTF8Str)] string tag, OnInstanceElement onInstanceElement, OnReleaseElement onReleaseElement);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate IntPtr OnInstanceElement(IntPtr parentElement, [MarshalAs(UnmanagedType.LPUTF8Str)] string tag, IntPtr xmlAttributes);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnReleaseElement(IntPtr element);
    }
}
