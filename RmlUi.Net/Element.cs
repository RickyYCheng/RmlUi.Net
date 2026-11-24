using RmlUiNet.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Xml.Linq;
using static RmlUiNet.Native.VariableDefinition;

namespace RmlUiNet
{
    public interface Element
    {
        public IntPtr NativePtr { get; }

        /// <summary>
        /// Gets the name of the element.
        /// </summary>
        public string TagName { get; }

        /// <summary>
        /// Gets the document this element belongs to.
        /// </summary>
        public ElementDocument? OwnerDocument { get; }

        /// <summary>
        /// Adds an event listener to this element.
        /// </summary>
        /// <param name="name">The name of the event to attach to.</param>
        /// <param name="eventListener">Listener object to be attached.</param>
        /// <param name="inCapturePhase">True if the listener is to be attached to the capture phase, false for the bubble phase.</param>
        public void AddEventListener(string name, EventListener eventListener, bool inCapturePhase = false);

        /// <summary>
        /// Adds an event listener to this element.
        /// </summary>
        /// <param name="name">The name of the event to attach to.</param>
        /// <param name="eventListener">Listener object to be attached.</param>
        /// <param name="inCapturePhase">True if the listener is to be attached to the capture phase, false for the bubble phase.</param>
        public void RemoveEventListener(string name, EventListener eventListener, bool inCapturePhase = false);

        /// <summary>
        /// Get a child element by its ID.
        /// </summary>
        /// <param name="id">ID of the the child element</param>
        /// <returns>The child of this element with the given ID, or null if no such child exists.</returns>
        public Element? GetElementById(string id);

        /// <summary>
        /// Sets the markup and content of the element. All existing children will be replaced.
        /// </summary>
        /// <param name="rml">The new content of the element.</param>
        public void SetInnerRml(string rml);

        /// <summary>
        /// Gives focus to the current element.
        /// </summary>
        /// <returns>True if the change focus request was successful.</returns>
        public bool Focus();

        /// <summary>
        /// Removes focus from from this element.
        /// </summary>
        public void Blur();

        /// <summary>
        /// Fakes a mouse click on this element.
        /// </summary>
        public void Click();

        /// <summary>
        /// The distance from this element�s top border to its offset parent�s top border.
        /// </summary>
        float GetOffsetTop();

        /// <summary>
        /// The distance from the context�s left edge and the element�s left border.
        /// </summary>
        float GetAbsoluteLeft();

        /// <summary>
        /// The distance from the context�s top edge and the element�s top border.
        /// </summary>
        float GetAbsoluteTop();

        /// <summary>
        /// The inner width of an element.
        /// </summary>
        float GetClientWidth();

        /// <summary>
        /// The inner height of an element.
        /// </summary>
        float GetClientHeight();

        /// <summary>
        /// Get a properties value as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetProperty(string name);

        /// <summary>
        /// Get a css/style attributes value as a string.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        string GetAttribute(string name);
        Element AppendChild(Element child, bool addToDom = true);
        Element AppendChildTag(string tagName, bool addToDom = true);
        void ScrollTo(float x, float y, ScrollBehavior behavior);
        void AddClass(string className);
        void RemoveClass(string className);
        void SetAttribute(string attributeName, string value);
        void SetProperty(string propertyName, string value);
        void AddEventListener(string name, Action<Event> action);
        string GetInnerRml();

        /// <summary>
        /// Sends an event to this element.
        /// </summary>
        /// <param name="eventId">The event type name, e.g. <c>"click"</c> or a name registered
        /// with <see cref="Rml.RegisterEventType"/>.</param>
        /// <param name="parameters">The event parameters.</param>
        /// <param name="interruptible">True if <see cref="Event.StopPropagation"/> may stop it.</param>
        /// <param name="bubbles">True to propagate up the ancestor chain.</param>
        /// <returns>
        /// True if the event was <em>not</em> consumed (that is, nothing stopped its propagation),
        /// false if it was stopped. Note that an event with no listeners also reports true, so this
        /// is not a delivery check — observe the listener instead.
        /// </returns>
        bool DispatchEvent(string eventId, Dictionary<string, object> parameters, bool interruptible, bool bubbles);

        Element? GetParentNode();
        void ReplaceChild(Element elementToinsert, Element elementToReplace);
        Element? QuerySelector(string selector);
        bool HasClass(string className);
        void RemoveChild(Element child);
        void RemoveEventListener(string name, Action<Event> action);
        void RemoveAttribute(string prop);
        bool HasAttribute(string attributeName);

        /// <summary>The element's <c>id</c> attribute, or an empty string when unset.</summary>
        string Id { get; }

        /// <summary>Sets the element's <c>id</c> attribute.</summary>
        void SetId(string id);

        /// <summary>Number of child elements.</summary>
        int NumChildren { get; }

        /// <summary>True when the element has at least one child node.</summary>
        bool HasChildNodes { get; }

        /// <summary>Child element at the given index.</summary>
        Element? GetChild(int index);

        /// <summary>The first child element, or null.</summary>
        Element? GetFirstChild();

        /// <summary>The last child element, or null.</summary>
        Element? GetLastChild();

        /// <summary>The next sibling element, or null.</summary>
        Element? GetNextSibling();

        /// <summary>The previous sibling element, or null.</summary>
        Element? GetPreviousSibling();

        /// <summary>Replaces this element's children with a single text node.</summary>
        void SetInnerText(string text);

        /// <summary>Moves <paramref name="elementToAdd"/> before <paramref name="adjacentElement"/>.</summary>
        bool InsertBefore(Element elementToAdd, Element adjacentElement);

        /// <summary>The nearest ancestor matching the selector, or null.</summary>
        Element? Closest(string selectors);

        /// <summary>True when the element matches the selector.</summary>
        bool Matches(string selectors);

        /// <summary>True when the class is set on the element. Alias of <see cref="HasClass"/>.</summary>
        bool IsClassSet(string className);

        /// <summary>All descendants matching the selector.</summary>
        IReadOnlyList<Element> QuerySelectorAll(string selector);

        /// <summary>All descendant elements with the given tag name.</summary>
        IReadOnlyList<Element> GetElementsByTagName(string tagName);

        /// <summary>All descendant elements carrying the given class.</summary>
        IReadOnlyList<Element> GetElementsByClassName(string className);

        /// <summary>Scrolls the element into view using the adaptive alignment.</summary>
        void ScrollIntoView();

        /// <inheritdoc cref="Element.GetOffsetLeft"/>
        float GetOffsetLeft();

        /// <inheritdoc cref="Element.GetOffsetWidth"/>
        float GetOffsetWidth();

        /// <inheritdoc cref="Element.GetOffsetHeight"/>
        float GetOffsetHeight();

        /// <summary>The offset parent element, or null for the root.</summary>
        Element? GetOffsetParent();

        /// <inheritdoc cref="Element.GetClientLeft"/>
        float GetClientLeft();

        /// <inheritdoc cref="Element.GetClientTop"/>
        float GetClientTop();

        /// <inheritdoc cref="Element.GetScrollTop"/>
        float GetScrollTop();

        /// <inheritdoc cref="Element.GetScrollWidth"/>
        float GetScrollWidth();

        /// <inheritdoc cref="Element.GetScrollHeight"/>
        float GetScrollHeight();
    }

    internal interface IRmlElementIdentity
    {
        NativeLifetime? Lifetime { get; }
    }

    public abstract class Element<T> : RmlBase<T>, Element, IRmlElementIdentity
        where T : class
    {
        private ElementEventListener? _listener;
        NativeLifetime? IRmlElementIdentity.Lifetime => Lifetime;
        #region Properties

        public string TagName => Marshal.PtrToStringUTF8(Native.Element.GetTagName(NativePtr)) ?? string.Empty;

        /// <summary>
        /// Gets the document this element belongs to.
        /// </summary>
        public ElementDocument? OwnerDocument
        {
            get
            {
                var el = Native.Element.GetOwnerDocument(NativePtr, out var elementPtr);
                var type = Marshal.PtrToStringUTF8(el);
                return type is null ? null : Util.GetElementByTypeName(elementPtr, type) as ElementDocument;
            }
        }

        #endregion

        #region Methods

        protected Element(IntPtr ptr, bool automaticallyRegisterInCache)
            : this(ptr, automaticallyRegisterInCache, true) { }

        internal Element(IntPtr ptr, bool automaticallyRegisterInCache, bool track)
            : base(ptr, automaticallyRegisterInCache)
        {
            if (track && ptr != IntPtr.Zero)
                Lifetime = RmlInstanceCache.Instance.TrackElement(ptr);
        }

        /// <summary>
        /// Two wrappers are equal when they refer to the same native element.
        /// </summary>
        /// <remarks>
        /// This used to compare <see cref="GetInnerRml"/>, which made distinct empty elements
        /// equal and changed an element's hash whenever its content changed — corrupting any
        /// <see cref="HashSet{T}"/> or dictionary keyed on elements.
        /// </remarks>
        public override bool Equals(object? obj)
        {
            return ReferenceEquals(this, obj) || (Lifetime is not null && obj is IRmlElementIdentity element &&
                ReferenceEquals(Lifetime, element.Lifetime));
        }

        public override int GetHashCode()
        {
            return Identity.GetHashCode();
        }

        /// <summary>
        /// Adds an event listener to this element.
        /// </summary>
        /// <param name="name">The name of the event to attach to.</param>
        /// <param name="eventListener">Listener object to be attached.</param>
        /// <param name="inCapturePhase">True if the listener is to be attached to the capture phase, false for the bubble phase.</param>
        public void AddEventListener(string name, EventListener eventListener, bool inCapturePhase = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            Native.Element.AddEventListener(NativePtr, name, eventListener.NativePtr, inCapturePhase);
        }

        internal class ElementEventListener : EventListener
        {
            internal Dictionary<string, List<Action<Event>>> _handlers = [];
            private Element _owningElement;

            public ElementEventListener(Element owningElement) : base()
            {
                _owningElement = owningElement;
            }

            public void AddHandler(string eventName, Action<Event> handler)
            {
                if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException("eventName");
                if (!_handlers.TryGetValue(eventName, out var handlers))
                {
                    handlers = new();
                    _handlers[eventName] = handlers;
                    _owningElement.AddEventListener(eventName, this);
                }
                handlers.Add(handler);
            }

            public void RemoveHandler(string eventName, Action<Event> handler)
            {
                if (string.IsNullOrEmpty(eventName)) throw new ArgumentNullException("eventName");
                if (_handlers.TryGetValue(eventName, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }

            public override void ProcessEvent(Event ev)
            {
                // Dispatch by event type, not by EventId: a type registered through
                // Rml.RegisterEventType gets an id outside the EventId enum, so ev.Id.ToString()
                // is a number and would never match a handler registered by name.
                if (!string.IsNullOrEmpty(ev.Type) &&
                    _handlers.TryGetValue(ev.Type.ToLowerInvariant(), out var handlers))
                {
                    foreach (var handler in handlers.ToArray())
                    {
                        Rml.InvokeCallback(() => handler(ev));
                        if (!ev.IsImmediatePropagating)
                            break;
                    }
                }
            }

            public override void OnAttach(Element element)
            {
                base.OnAttach(element);
            }

            public override void OnDetach(Element element)
            {
                if (AttachmentCount == 0)
                    Rml.Retire(() =>
                    {
                        if (AttachmentCount == 0)
                            Dispose();
                    });
            }
        }

        public void AddEventListener(string name, Action<Event> action)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            _ = NativePtr;
            ArgumentNullException.ThrowIfNull(action);
            if (_listener is null || _listener.IsDisposed)
                _listener = new ElementEventListener(this);
            _listener.AddHandler(name.ToLowerInvariant(), action);
        }

        public void RemoveEventListener(string name, Action<Event> action)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            _ = NativePtr;
            _listener?.RemoveHandler(name.ToLowerInvariant(), action);
        }

        /// <summary>
        /// Adds an event listener to this element.
        /// </summary>
        /// <param name="name">The name of the event to attach to.</param>
        /// <param name="eventListener">Listener object to be attached.</param>
        /// <param name="inCapturePhase">True if the listener is to be attached to the capture phase, false for the bubble phase.</param>
        public void RemoveEventListener(string name, EventListener eventListener, bool inCapturePhase = false)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            Native.Element.RemoveEventListener(NativePtr, name, eventListener.NativePtr, inCapturePhase);
        }

        /// <inheritdoc cref="Element.DispatchEvent(string, Dictionary{string, object}, bool, bool)"/>
        public bool DispatchEvent(string event_id, Dictionary<string, object> parameters, bool interruptible, bool bubbles)
        {
            if (string.IsNullOrEmpty(event_id)) throw new ArgumentNullException("event_id");
            var dict = Native.RmlDictionary.Create();
            try
            {
                foreach (var kv in parameters)
                {
                    using var variant = Util.ToVariant(kv.Value);

                    if (variant is not null)
                    {
                        Native.RmlDictionary.Insert(dict, kv.Key, variant.NativePtr);
                    }
                }
                return Native.Element.DispatchEvent(NativePtr, event_id, dict, interruptible, bubbles);
            }
            finally
            {
                Native.RmlDictionary.Free(dict);
            }
        }

        /// <summary>
        /// Finds an element by its ID.
        /// </summary>
        /// <param name="id">The ID to look for. The special names <c>#self</c>, <c>#parent</c> and
        /// <c>#document</c> resolve to this element, its parent and its owner document.</param>
        /// <returns>The matching element, or null if there is none.</returns>
        /// <remarks>
        /// Despite being an instance method, this searches the <em>whole owner document</em>, not
        /// just this element's subtree (RmlUi's <c>Element::GetElementById</c> walks from
        /// <c>GetOwnerDocument()</c>), and the search is breadth-first, so the shallowest match
        /// wins when IDs are duplicated. Use <see cref="QuerySelectorAll"/> for subtree-scoped
        /// lookups.
        /// </remarks>
        public Element? GetElementById(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentNullException("id");
            var nativeElementType = Native.Element.GetElementById(NativePtr, id, out var elementPtr);
            var elementType = Marshal.PtrToStringAnsi(nativeElementType);

            if (null == elementType)
            {
                return null;
            }

            return Util.GetOrThrowElementByTypeName(elementPtr, elementType);
        }

        public bool HasClass(string className)
        {
            if (string.IsNullOrEmpty(className)) throw new ArgumentNullException("className");
            return Native.Element.HasClass(NativePtr, className);
        }

        public bool HasAttribute(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentNullException("attributeName");
            return Native.Element.HasAttribute(NativePtr, attributeName);
        }

        internal string GetElementTypeName()
        {
            var typePtr = Native.Element.GetElementTypeName(NativePtr);
            return Marshal.PtrToStringAnsi(typePtr) ?? "Unknown";
        }

        /// <summary>
        /// Append child element
        /// </summary>
        /// <param name="child"></param>
        /// <param name="addToDom"></param>
        /// <returns></returns>
        public Element AppendChild(Element child, bool addToDom = true)
        {
            var nativeElement = Native.Element.AppendChild(NativePtr, child.NativePtr, addToDom);
            var typePtr = Native.Element.GetElementTypeName(nativeElement);
            var elementType = Marshal.PtrToStringAnsi(typePtr);
            if (elementType is null) throw new Exception("Failed to get element type name for appended child");
            return Util.GetOrThrowElementByTypeName(nativeElement, elementType);
        }

        /// <summary>
        /// Create and append a child element
        /// </summary>
        /// <param name="tagName"></param>
        /// <param name="addToDom"></param>
        /// <returns></returns>
        public Element AppendChildTag(string tagName, bool addToDom = true)
        {
            if (string.IsNullOrEmpty(tagName)) throw new ArgumentNullException("tagName");
            var nativeElement = Native.Element.AppendChildTag(NativePtr, tagName, addToDom);
            var typePtr = Native.Element.GetElementTypeName(nativeElement);
            var elementType = Marshal.PtrToStringAnsi(typePtr);
            if (elementType is null) throw new Exception("Failed to get element type name for appended child");
            return Util.GetOrThrowElementByTypeName(nativeElement, elementType);
        }

        public void RemoveChild(Element child)
        {
            Native.Element.RemoveChild(NativePtr, child.NativePtr);
        }

        /// <summary>
        /// Scroll to the specified offset with the specified behavior
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="behavior"></param>
        public void ScrollTo(float x, float y, ScrollBehavior behavior = ScrollBehavior.Auto)
        {
            Native.Element.ScrollTo(NativePtr, x, y, (int)behavior);
        }

        /// <summary>
        /// Sets the markup and content of the element. All existing children will be replaced.
        /// </summary>
        /// <param name="rml">The new content of the element.</param>
        public void SetInnerRml(string rml)
        {
            Native.Element.SetInnerRml(NativePtr, rml ?? "");
        }

        /// <summary>
        /// Get the markup content of this element
        /// </summary>
        /// <returns></returns>
        public string GetInnerRml()
        {
            return Util.ReadOwnedUtf8(Native.Element.GetInnerRml(NativePtr));
        }

        public void AddClass(string className)
        {
            if (string.IsNullOrEmpty(className)) throw new ArgumentNullException("className");
            Native.Element.SetClass(NativePtr, className, true);
        }

        public void RemoveClass(string className)
        {
            if (string.IsNullOrEmpty(className)) throw new ArgumentNullException("className");
            Native.Element.SetClass(NativePtr, className, false);
        }

        /// <summary>
        /// Gives focus to the current element.
        /// </summary>
        /// <returns>True if the change focus request was successful.</returns>
        public bool Focus()
        {
            return Native.Element.Focus(NativePtr);
        }

        /// <summary>
        /// Removes focus from from this element.
        /// </summary>
        public void Blur()
        {
            Native.Element.Blur(NativePtr);
        }

        /// <summary>
        /// Fakes a mouse click on this element.
        /// </summary>
        public void Click()
        {
            Native.Element.Click(NativePtr);
        }

        /// <inheritdoc cref="Element.GetOffsetTop"/>
        public float GetOffsetTop()
        {
            return Native.Element.GetOffsetTop(NativePtr);
        }

        /// <inheritdoc cref="Element.GetAbsoluteLeft"/>
        public float GetAbsoluteLeft()
        {
            return Native.Element.GetAbsoluteLeft(NativePtr);
        }

        /// <inheritdoc cref="Element.GetAbsoluteTop"/>
        public float GetAbsoluteTop()
        {
            return Native.Element.GetAbsoluteTop(NativePtr);
        }

        /// <inheritdoc cref="Element.GetClientWidth"/>
        public float GetClientWidth()
        {
            return Native.Element.GetClientWidth(NativePtr);
        }

        /// <inheritdoc cref="Element.GetClientHeight"/>
        public float GetClientHeight()
        {
            return Native.Element.GetClientHeight(NativePtr);
        }

        public string GetProperty(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            return Util.ReadOwnedUtf8(Native.Element.GetPropertyString(NativePtr, name));
        }

        public void SetProperty(string propertyName, string value)
        {
            if (string.IsNullOrEmpty(propertyName)) throw new ArgumentNullException("propertyName");
            if (value == null) throw new ArgumentNullException("value");
            Native.Element.SetProperty(NativePtr, propertyName, value);
        }

        /// <inheritdoc cref="Element.GetAttribute(string)"/>
        public string GetAttribute(string attributeName)
        {
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentNullException("attributeName");
            return Util.ReadOwnedUtf8(Native.Element.GetAttributeString(NativePtr, attributeName, ""));
        }

        public void SetAttribute(string attributeName, string value)
        {
            if (string.IsNullOrEmpty(attributeName)) throw new ArgumentNullException("attributeName");
            if (value == null) throw new ArgumentNullException("value");
            Native.Element.SetAttributeString(NativePtr, attributeName, value);
        }

        /// <summary>The parent element, or null for the root.</summary>
        public Element? GetParentNode()
        {
            var elementPtr = Native.Element.GetParentNode(NativePtr);
            if (elementPtr == IntPtr.Zero)
                return null;

            var elementType = Marshal.PtrToStringAnsi(Native.Element.GetElementTypeName(elementPtr));
            return elementType is null ? null : Util.GetOrThrowElementByTypeName(elementPtr, elementType);
        }

        public void ReplaceChild(Element elementToinsert, Element elementToReplace)
        {
            Native.Element.ReplaceChild(NativePtr, elementToinsert.NativePtr, elementToReplace.NativePtr);
        }

        public Element? QuerySelector(string selector)
        {
            if (string.IsNullOrEmpty(selector)) throw new ArgumentNullException("selector");
            var elementPtr = Native.Element.QuerySelector(NativePtr, selector);
            if (elementPtr == IntPtr.Zero) return null;
            var nativeElementType = Native.Element.GetElementTypeName(elementPtr);
            var elementType = Marshal.PtrToStringAnsi(nativeElementType);

            if (null == elementType)
            {
                return null;
            }

            return Util.GetOrThrowElementByTypeName(elementPtr, elementType);
        }

        public void RemoveAttribute(string prop)
        {
            if (string.IsNullOrEmpty(prop)) throw new ArgumentNullException("prop");
            Native.Element.RemoveAttribute(NativePtr, prop);
        }

        #region Traversal

        /// <inheritdoc />
        public string Id
        {
            get
            {
                var ptr = Native.Element.GetId(NativePtr);
                return Marshal.PtrToStringUTF8(ptr) ?? string.Empty;
            }
        }

        /// <inheritdoc />
        public int NumChildren => Native.Element.GetNumChildren(NativePtr, false);

        /// <inheritdoc />
        public bool HasChildNodes => Native.Element.HasChildNodes(NativePtr);

        /// <inheritdoc />
        public Element? GetChild(int index)
        {
            if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
            return WrapOrNull(Native.Element.GetChild(NativePtr, index));
        }

        /// <inheritdoc />
        public Element? GetFirstChild() => WrapOrNull(Native.Element.GetFirstChild(NativePtr));

        /// <inheritdoc />
        public Element? GetLastChild() => WrapOrNull(Native.Element.GetLastChild(NativePtr));

        /// <inheritdoc />
        public Element? GetNextSibling() => WrapOrNull(Native.Element.GetNextSibling(NativePtr));

        /// <inheritdoc />
        public Element? GetPreviousSibling() => WrapOrNull(Native.Element.GetPreviousSibling(NativePtr));

        /// <inheritdoc />
        public void SetInnerText(string text)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            Native.Element.SetInnerText(NativePtr, text);
        }

        /// <inheritdoc />
        public bool InsertBefore(Element elementToAdd, Element adjacentElement)
        {
            if (elementToAdd == null) throw new ArgumentNullException(nameof(elementToAdd));
            if (adjacentElement == null) throw new ArgumentNullException(nameof(adjacentElement));
            return Native.Element.InsertBefore(NativePtr, elementToAdd.NativePtr, adjacentElement.NativePtr);
        }

        /// <summary>Sets the element's <c>id</c> attribute.</summary>
        public void SetId(string id)
        {
            if (id == null) throw new ArgumentNullException(nameof(id));
            Native.Element.SetId(NativePtr, id);
        }

        #endregion

        #region Queries

        /// <inheritdoc />
        public Element? Closest(string selectors)
        {
            if (string.IsNullOrEmpty(selectors)) throw new ArgumentNullException(nameof(selectors));
            return WrapOrNull(Native.Element.Closest(NativePtr, selectors));
        }

        /// <inheritdoc />
        public bool Matches(string selectors)
        {
            if (string.IsNullOrEmpty(selectors)) throw new ArgumentNullException(nameof(selectors));
            return Native.Element.Matches(NativePtr, selectors);
        }

        /// <inheritdoc />
        public bool IsClassSet(string className)
        {
            if (string.IsNullOrEmpty(className)) throw new ArgumentNullException(nameof(className));
            return Native.Element.IsClassSet(NativePtr, className);
        }

        /// <inheritdoc />
        public IReadOnlyList<Element> QuerySelectorAll(string selector)
        {
            if (string.IsNullOrEmpty(selector)) throw new ArgumentNullException(nameof(selector));
            return WrapList(Native.Element.QuerySelectorAll(NativePtr, selector));
        }

        /// <inheritdoc />
        public IReadOnlyList<Element> GetElementsByTagName(string tagName)
        {
            if (string.IsNullOrEmpty(tagName)) throw new ArgumentNullException(nameof(tagName));
            return WrapList(Native.Element.GetElementsByTagName(NativePtr, tagName));
        }

        /// <inheritdoc />
        public IReadOnlyList<Element> GetElementsByClassName(string className)
        {
            if (string.IsNullOrEmpty(className)) throw new ArgumentNullException(nameof(className));
            return WrapList(Native.Element.GetElementsByClassName(NativePtr, className));
        }

        /// <summary>
        /// Copies an opaque native <c>ElementList</c> into managed wrappers and releases it.
        /// The list holds non-owning element pointers that RmlUi invalidates on the next tree
        /// change, so they are wrapped without taking ownership.
        /// </summary>
        private static IReadOnlyList<Element> WrapList(IntPtr list)
        {
            if (list == IntPtr.Zero)
                return Array.Empty<Element>();

            try
            {
                int count = Native.Element.ElementListSize(list);
                if (count <= 0)
                    return Array.Empty<Element>();

                var result = new List<Element>(count);
                for (int i = 0; i < count; i++)
                {
                    Element? element = WrapOrNull(Native.Element.ElementListGet(list, i));
                    if (element is not null)
                        result.Add(element);
                }
                return result;
            }
            finally
            {
                Native.Element.ElementListFree(list);
            }
        }

        /// <summary>Wraps a raw element pointer using its concrete managed type, or null.</summary>
        private static Element? WrapOrNull(IntPtr elementPtr)
        {
            if (elementPtr == IntPtr.Zero)
                return null;

            var typePtr = Native.Element.GetElementTypeName(elementPtr);
            var elementType = typePtr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(typePtr);
            return elementType is null ? null : Util.GetOrThrowElementByTypeName(elementPtr, elementType);
        }

        #endregion

        #region Geometry

        /// <inheritdoc />
        public void ScrollIntoView() => Native.Element.ScrollIntoView(NativePtr);

        /// <inheritdoc />
        public float GetOffsetLeft() => Native.Element.GetOffsetLeft(NativePtr);

        /// <inheritdoc />
        public float GetOffsetWidth() => Native.Element.GetOffsetWidth(NativePtr);

        /// <inheritdoc />
        public float GetOffsetHeight() => Native.Element.GetOffsetHeight(NativePtr);

        /// <inheritdoc />
        public Element? GetOffsetParent() => WrapOrNull(Native.Element.GetOffsetParent(NativePtr));

        /// <inheritdoc />
        public float GetClientLeft() => Native.Element.GetClientLeft(NativePtr);

        /// <inheritdoc />
        public float GetClientTop() => Native.Element.GetClientTop(NativePtr);

        /// <inheritdoc />
        public float GetScrollTop() => Native.Element.GetScrollTop(NativePtr);

        /// <inheritdoc />
        public float GetScrollWidth() => Native.Element.GetScrollWidth(NativePtr);

        /// <inheritdoc />
        public float GetScrollHeight() => Native.Element.GetScrollHeight(NativePtr);

        #endregion

        public override void Dispose()
        {
            base.Dispose();
        }

        #endregion
    }
}
