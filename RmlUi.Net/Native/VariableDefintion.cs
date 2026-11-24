using System;
using System.Runtime.InteropServices;
using System.Text;

namespace RmlUiNet.Native {
    public static class VariableDefinition {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_VariableDefinition_New")]
        public static extern IntPtr Create(DataVariableType type, OnGet onGet, OnSet onSet, OnSize onSize, OnChild onChild);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_VariableDefinition_Free")]
        public static extern void Free(IntPtr instance);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool OnGet(IntPtr ptr, IntPtr variant);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool OnSet(IntPtr ptr, IntPtr variant);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int OnSize(IntPtr ptr);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr OnChild(IntPtr ptr, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int index);
    }
}
