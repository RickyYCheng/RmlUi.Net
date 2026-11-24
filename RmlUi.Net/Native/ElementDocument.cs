using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class ElementDocument
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_Show")]
        public static extern void Show(IntPtr document, ModalFlag modalFlag, FocusFlag focusFlag, ScrollFlag scrollFlag);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_Hide")]
        public static extern void Hide(IntPtr document);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_AddStyleSheetContainer")]
        public static extern void AddStyleSheetContainer(IntPtr document, IntPtr container);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_Close")]
        public static extern void Close(IntPtr document);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_PullToFront")]
        public static extern void PullToFront(IntPtr document);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_New")]
        public static extern IntPtr Create(OnLoadInlineScript? onLoadInlineScript, OnLoadExternalScript? onLoadExternalScript);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_CreateElement")]
        public static extern IntPtr CreateElement(IntPtr document, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_CreateTextNode")]
        public static extern IntPtr CreateTextNode(IntPtr document, [MarshalAs(UnmanagedType.LPUTF8Str)] string text);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_SetTitle")]
        public static extern void SetTitle(IntPtr document, [MarshalAs(UnmanagedType.LPUTF8Str)] string title);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_GetTitle")]
        public static extern IntPtr GetTitle(IntPtr document);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_GetSourceURL")]
        public static extern IntPtr GetSourceURL(IntPtr document);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementDocument_Free")]
        public static extern void Free(IntPtr document);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnLoadInlineScript([MarshalAs(UnmanagedType.LPUTF8Str)] string context, [MarshalAs(UnmanagedType.LPUTF8Str)] string source_path, int source_line);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate void OnLoadExternalScript([MarshalAs(UnmanagedType.LPUTF8Str)] string source_path);
    }
}
