using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class PropertyDefinition
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_PropertyDefinition_AddParser")]
        public static extern IntPtr AddParser(IntPtr propertyDefinition, [MarshalAs(UnmanagedType.LPUTF8Str)] string parserName, [MarshalAs(UnmanagedType.LPUTF8Str)] string parserParams);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_PropertyDefinition_GetId")]
        public static extern int GetId(IntPtr propertyDefinition);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_PropertyDefinition_GetValueString")]
        public static extern IntPtr GetValueString(IntPtr propertyDefinition, IntPtr property, [MarshalAs(UnmanagedType.LPUTF8Str)] string defaultValue);
    }
}
