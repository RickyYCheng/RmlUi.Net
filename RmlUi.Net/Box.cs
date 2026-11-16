using System;

namespace RmlUiNet {
    /// <summary>
    /// A snapshot of an element's box (content, padding, border and margin sizes).
    /// </summary>
    /// <remarks>
    /// The wrapper does not own the underlying <c>Rml::Box</c>: it points into the element
    /// and is invalidated by the next layout. Read the values you need right away instead of
    /// holding on to the instance.
    /// </remarks>
    public class Box : RmlBase<Box> {

        internal Box(IntPtr ptr) : base(ptr) {

        }

        /// <summary>The top-left position of the given area, relative to the border box.</summary>
        public Vector2f GetPosition(BoxArea area = BoxArea.Content)
        {
            Native.Box.GetPosition(NativePtr, (int)area, out float x, out float y);
            return new Vector2f(x, y);
        }

        /// <summary>The size of the content area.</summary>
        public Vector2f GetSize()
        {
            Native.Box.GetSize(NativePtr, out float x, out float y);
            return new Vector2f(x, y);
        }

        /// <summary>The size of the given area, including all inner areas.</summary>
        public Vector2f GetSize(BoxArea area)
        {
            Native.Box.GetSize(NativePtr, (int)area, out float x, out float y);
            return new Vector2f(x, y);
        }
    }
}
