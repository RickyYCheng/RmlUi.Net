using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native {
    internal unsafe static class DataModelConstructor {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindFloat")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindFloat(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindUInt")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindUInt(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindInt")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindInt(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindBool")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindBool(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindString")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindString(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindStringArray")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindStringArray(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr data, int count);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_GetModelHandle")]
        public static extern IntPtr GetModelHandle(IntPtr instance);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_RegisterStruct")]
        public static extern IntPtr RegisterStruct(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        // Struct member registration. The offsets are byte offsets into the managed struct,
        // converted to member pointers on the native side.
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_StructHandle_RegisterMemberFloat")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RegisterStructMemberFloat(IntPtr structHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int offset);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_StructHandle_RegisterMemberInt")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RegisterStructMemberInt(IntPtr structHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int offset);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_StructHandle_RegisterMemberString")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RegisterStructMemberString(IntPtr structHandle, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int offset);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindVariable")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindVariable(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, IntPtr variable);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_Free")]
        public static extern void Free(IntPtr instance);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_DataModelConstructor_BindEventCallback")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool BindEventCallback(IntPtr instance, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, DataEventFunc func);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void DataEventFunc(IntPtr dataModel, IntPtr evt, nint* variants, int numVariants);
    }
}
