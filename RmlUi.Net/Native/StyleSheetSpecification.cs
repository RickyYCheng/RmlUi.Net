using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class StyleSheetSpecification
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_StyleSheetSpecification_RegisterProperty")]
        public static extern IntPtr RegisterProperty([MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string defaultValue, [MarshalAs(UnmanagedType.I1)] bool inherited, [MarshalAs(UnmanagedType.I1)] bool forcesLayout);
    }
}
