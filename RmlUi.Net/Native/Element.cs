using System;
using System.Runtime.InteropServices;

namespace RmlUiNet.Native
{
    internal static class Element
    {
        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetTagName")]
        public static extern IntPtr GetTagName(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_AddEventListener")]
        public static extern void AddEventListener(IntPtr element, string name, IntPtr eventListener, bool inCapturePhase);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_RemoveEventListener")]
        public static extern void RemoveEventListener(IntPtr element, string name, IntPtr eventListener, bool inCapturePhase);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetElementByID")]
        public static extern IntPtr GetElementById(IntPtr element, string id, out IntPtr foundElement);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOwnerDocument")]
        public static extern IntPtr GetOwnerDocument(IntPtr element, out IntPtr foundElement);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetInnerRml")]
        public static extern void SetInnerRml(IntPtr element, string rml);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetInnerRml")]
        public static extern IntPtr GetInnerRml(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_Focus")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Focus(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_Blur")]
        public static extern void Blur(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_Click")]
        public static extern void Click(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetBox")]
        public static extern IntPtr GetBox(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOffsetTop")]
        public static extern float GetOffsetTop(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetAbsoluteLeft")]
        public static extern float GetAbsoluteLeft(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetAbsoluteTop")]
        public static extern float GetAbsoluteTop(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetClientHeight")]
        public static extern float GetClientHeight(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetClientWidth")]
        public static extern float GetClientWidth(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetAttributeString")]
        public static extern IntPtr GetAttributeString(IntPtr element, string attributeName, string defaultValue);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetAttributeString")]
        public static extern void SetAttributeString(IntPtr element, string attributeName, string value);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetPropertyString")]
        public static extern IntPtr GetPropertyString(IntPtr element, string propertyName);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetProperty")]
        public static extern IntPtr GetProperty(IntPtr element, string propertyName);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetPropertyById")]
        public static extern IntPtr GetPropertyById(IntPtr element, int propertyId);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetProperty")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetProperty(IntPtr element, string propertyName, string value);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetElementTypeName")]
        public static extern IntPtr GetElementTypeName(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_AppendChild")]
        public static extern IntPtr AppendChild(IntPtr element, IntPtr elementToRemove, bool addToDom);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_AppendChildTag")]
        public static extern IntPtr AppendChildTag(IntPtr element, string tagName, bool addToDom);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_RemoveChild")]
        public static extern void RemoveChild(IntPtr element, IntPtr elementToAppend);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_ScrollTo")]
        public static extern void ScrollTo(IntPtr element, float x, float y, int behavior);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetClass")]
        public static extern void SetClass(IntPtr element, string className, bool enable);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetParentNode")]
        public static extern IntPtr GetParentNode(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_ReplaceChild")]
        public static extern void ReplaceChild(IntPtr element, IntPtr elementToInsert, IntPtr elementToReplace);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_QuerySelector")]
        public static extern IntPtr QuerySelector(IntPtr element, string selector);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_HasClass")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HasClass(IntPtr element, string className);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_HasAttribute")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HasAttribute(IntPtr element, string attributeName);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_RemoveAttribute")]
        public static extern void RemoveAttribute(IntPtr element, string attributeName);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_DispatchEvent")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool DispatchEvent(IntPtr element, string event_id, IntPtr parameters, bool interruptible, bool bubbles);

        // --- Traversal -------------------------------------------------------------

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetNumChildren")]
        public static extern int GetNumChildren(IntPtr element, bool includeNonDomElements);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetChild")]
        public static extern IntPtr GetChild(IntPtr element, int index);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetFirstChild")]
        public static extern IntPtr GetFirstChild(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetLastChild")]
        public static extern IntPtr GetLastChild(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetNextSibling")]
        public static extern IntPtr GetNextSibling(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetPreviousSibling")]
        public static extern IntPtr GetPreviousSibling(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_HasChildNodes")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool HasChildNodes(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetId")]
        public static extern IntPtr GetId(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetId")]
        public static extern void SetId(IntPtr element, string id);

        // --- Geometry --------------------------------------------------------------

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetClientLeft")]
        public static extern float GetClientLeft(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetClientTop")]
        public static extern float GetClientTop(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOffsetLeft")]
        public static extern float GetOffsetLeft(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOffsetWidth")]
        public static extern float GetOffsetWidth(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOffsetHeight")]
        public static extern float GetOffsetHeight(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetOffsetParent")]
        public static extern IntPtr GetOffsetParent(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetScrollTop")]
        public static extern float GetScrollTop(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetScrollWidth")]
        public static extern float GetScrollWidth(IntPtr element);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetScrollHeight")]
        public static extern float GetScrollHeight(IntPtr element);

        // --- Selector queries ------------------------------------------------------

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_Closest")]
        public static extern IntPtr Closest(IntPtr element, string selectors);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_Matches")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool Matches(IntPtr element, string selectors);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_IsClassSet")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsClassSet(IntPtr element, string className);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_InsertBefore")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool InsertBefore(IntPtr element, IntPtr elementToAdd, IntPtr adjacentElement);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_SetInnerText")]
        public static extern void SetInnerText(IntPtr element, string text);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_ScrollIntoView")]
        public static extern void ScrollIntoView(IntPtr element);

        // --- Element lists (opaque handle, released with ElementListFree) ----------

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_QuerySelectorAll")]
        public static extern IntPtr QuerySelectorAll(IntPtr element, string selector);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetElementsByTagName")]
        public static extern IntPtr GetElementsByTagName(IntPtr element, string tagName);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_Element_GetElementsByClassName")]
        public static extern IntPtr GetElementsByClassName(IntPtr element, string className);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementList_Size")]
        public static extern int ElementListSize(IntPtr list);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementList_Get")]
        public static extern IntPtr ElementListGet(IntPtr list, int index);

        [DllImport("RmlUiNative", CallingConvention = CallingConvention.Cdecl, EntryPoint = "rml_ElementList_Free")]
        public static extern void ElementListFree(IntPtr list);
    }
}
