using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace RmlUiNet
{

    /// <summary>
    /// The propagation phases in which an event type may run its default action.
    /// </summary>
    /// <remarks>
    /// Mirrors <c>Rml::DefaultActionPhase</c> (RmlUi's <c>Include/RmlUi/Core/Event.h</c>), which only
    /// defines <c>None</c>, <c>Target</c> and <c>TargetAndBubble</c>. RmlUi asserts that the capture
    /// phase is never part of a default-action phase, so there is deliberately no <c>Capture</c>
    /// member here either.
    /// </remarks>
    [Flags]
    public enum DefaultActionPhase : int {
        None = 0,
        Target = 2,
        TargetAndBubble = Target | 4,
    }

    public enum ModalFlag
    {
        /// <summary>Remove modal state.</summary>
        None,

        /// <summary>Set modal state, other documents cannot receive focus.</summary>
        Modal,

        /// <summary>Modal state unchanged.</summary>
        Keep
    }

    public enum FocusFlag
    {
        /// <summary>No focus.</summary>
        None,

        /// <summary>Focus the document.</summary>
        Document,

        /// <summary>Focus the element in the document which last had focus.</summary>
        Keep,

        /// <summary>Focus the first tab element with the 'autofocus' attribute or else the document.</summary>
        Auto
    }

    public enum ScrollBehavior {
        Auto,
        Smooth,
        Instant
    }

    /// <summary>Controls whether showing a document also scrolls the focused element into view.</summary>
    public enum ScrollFlag
    {
        /// <summary>Never scroll.</summary>
        None,

        /// <summary>Scroll the focused element into view, if applicable.</summary>
        Auto
    }

    /// <summary>Alignment applied by <c>ScrollIntoView</c>, matching <c>Rml::ScrollAlignment</c>.</summary>
    public enum ScrollAlignment
    {
        /// <summary>Align to the start of the scroll container.</summary>
        Start,

        /// <summary>Align to the center of the scroll container.</summary>
        Center,

        /// <summary>Align to the end of the scroll container.</summary>
        End,

        /// <summary>Align with the minimal scroll change.</summary>
        Nearest,

        /// <summary>Do not scroll if already in view, otherwise align to the center.</summary>
        Adaptive
    }

    public enum VariantType {
        NONE = '-',
        BOOL = 'B',
        BYTE = 'b',
        CHAR = 'c',
        FLOAT = 'f',
        DOUBLE = 'd',
        INT = 'i',
        INT64 = 'I',
        UINT = 'u',
        UINT64 = 'U',
        STRING = 's',
        VECTOR2 = '2',
        VECTOR3 = '3',
        VECTOR4 = '4',
        COLOURF = 'g',
        COLOURB = 'h',
        SCRIPTINTERFACE = 'p',
        TRANSFORMPTR = 't',
        TRANSITIONLIST = 'T',
        ANIMATIONLIST = 'A',
        DECORATORSPTR = 'D',
        FILTERSPTR = 'F',
        FONTEFFECTSPTR = 'E',
        COLORSTOPLIST = 'C',
        BOXSHADOWLIST = 'S',
        VOIDPTR = '*',
    };

    public enum FontWeight
    {
        Auto,
        Normal = 400,
        Bold = 700
    }

    /// <summary>Font slant registered with a font face, matching <c>Rml::Style::FontStyle</c>.</summary>
    public enum FontStyle
    {
        Normal,
        Italic
    }

    public enum BoxArea { Margin, Border, Padding, Content, Auto };

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vector2i
    {
        public readonly int X;
        public readonly int Y;

        public Vector2i(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vector2f
    {
        public readonly float X;
        public readonly float Y;

        public Vector2f(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct Vertex
    {
        /// <summary>Two-dimensional position of the vertex (usually in pixels).</summary>
        public readonly Vector2f Position;

        /// <summary>RGBA-ordered 8-bit / channel colour.</summary>
        public readonly ColorB Colour;

        /// <summary>Texture coordinate for any associated texture.</summary>
        public readonly Vector2f TextureCoordinates;
    }

    [StructLayout(LayoutKind.Sequential)]
    public readonly struct ColorB
    {
        public readonly byte Red;
        public readonly byte Green;
        public readonly byte Blue;
        public readonly byte Alpha;

        public override string ToString()
        {
            return $"({Red}, {Green}, {Blue}, {Alpha})";
        }
    }
}
