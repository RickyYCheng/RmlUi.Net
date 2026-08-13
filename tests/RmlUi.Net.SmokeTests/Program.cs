// RmlUi.Net.SmokeTests
//
// Exercises every public API of RmlUi.Net with at least one call, with particular attention
// to the 51 exports that were missing from the native library published in package 1.0.1.
//
// Usage:
//   RmlUi.Net.SmokeTests [--window]
//
// --window additionally creates a hidden native window on Windows (skipped on other
// platforms). The test suite itself is headless: it uses in-memory stubs for the render,
// system and file interfaces.
//
// Exit code: 0 = all tests passed, 1 = at least one test failed.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RmlUiNet;
using RmlUiNet.Input;
using RmlUiNet.Native;

// Alias the managed types to disambiguate them from the internal static P/Invoke wrapper
// classes of the same name in RmlUiNet.Native.
using Native = RmlUiNet.Native;
using Rml = RmlUiNet.Rml;
using Box = RmlUiNet.Box;
using Context = RmlUiNet.Context;
using DataVariable = RmlUiNet.DataVariable;
using Debugger = RmlUiNet.Debugger;
using Element = RmlUiNet.Element;
using ElementCustom = RmlUiNet.ElementCustom;
using ElementDocument = RmlUiNet.ElementDocument;
using ElementFormControlInput = RmlUiNet.ElementFormControlInput;
using ElementInstancer = RmlUiNet.ElementInstancer;
using Event = RmlUiNet.Event;
using EventInstancer = RmlUiNet.EventInstancer;
using EventListenerInstancer = RmlUiNet.EventListenerInstancer;
using FileInterface = RmlUiNet.FileInterface;
using Plugin = RmlUiNet.Plugin;
using Property = RmlUiNet.Property;
using RenderInterface = RmlUiNet.RenderInterface;
using StyleSheetContainer = RmlUiNet.StyleSheetContainer;
using StyleSheetSpecification = RmlUiNet.StyleSheetSpecification;
using SystemInterface = RmlUiNet.SystemInterface;
using Variant = RmlUiNet.Variant;
using VariableDefinition = RmlUiNet.VariableDefinition;
using XMLAttributes = RmlUiNet.XMLAttributes;

namespace RmlUi.Net.SmokeTests
{
    internal static unsafe class Program
    {
        private static int _passCount;
        private static int _failCount;
        private static readonly List<string> _failures = new();

        private static TestRenderInterface? _renderInterface;
        private static TestSystemInterface? _systemInterface;
        private static TestFileInterface? _fileInterface;
        private static TestPlugin? _plugin;
        private static TestEventInstancer? _eventInstancer;
        private static TestEventListenerInstancer? _listenerInstancer;
        private static TestElementInstancer? _elementInstancer;
        private static Context? _context;
        private static ElementDocument? _document;

        private static int Main(string[] args)
        {
            bool createWindow = false;
            int limit = int.MaxValue;
            foreach (string arg in args)
            {
                if (arg == "--window") createWindow = true;
                if (arg.StartsWith("--limit=") && int.TryParse(arg.Substring(8), out int parsed)) limit = parsed;
            }

            Console.WriteLine("RmlUi.Net smoke tests");
            Console.WriteLine($"Platform: {RuntimeInformation.OSDescription}, RID: {RuntimeInformation.RuntimeIdentifier}");
            Console.WriteLine();

            int executed = 0;
            try
            {
                if (++executed <= limit || limit <= 0) Run("Initialise + interfaces + plugin", TestInitialise);
                if (++executed <= limit || limit <= 0) Run("Logging + custom event type", TestLoggingAndEventType);
                if (++executed <= limit || limit <= 0) Run("Variants", TestVariants);
                if (++executed <= limit || limit <= 0) Run("Dictionary APIs", TestDictionaryApis);
                if (++executed <= limit || limit <= 0) Run("Context creation + input processing", TestContext);
                if (++executed <= limit || limit <= 0) Run("Document load + title/source", TestDocument);
                if (++executed <= limit || limit <= 0) Run("Element basics", TestElementBasics);
                if (++executed <= limit || limit <= 0) Run("Element tree manipulation", TestElementTree);
                if (++executed <= limit || limit <= 0) Run("Attributes + properties (incl. empty strings)", TestAttributesAndProperties);
                if (++executed <= limit || limit <= 0) Run("Form controls", TestFormControls);
                if (++executed <= limit || limit <= 0) Run("Event dispatch + listeners + parameters", TestEvents);
                if (++executed <= limit || limit <= 0) Run("Data model scalars", TestDataModelScalars);
                if (++executed <= limit || limit <= 0) Run("Data model string array + struct", TestDataModelStringArrayAndStruct);
                if (++executed <= limit || limit <= 0) Run("Data model custom variable + events", TestDataModelVariable);
                if (++executed <= limit || limit <= 0) Run("Debugger", TestDebugger);
                if (++executed <= limit || limit <= 0) Run("Style sheet specification + property definitions", TestStyleSheetSpecification);
                if (++executed <= limit || limit <= 0) Run("Event instancers", TestInstancers);
                if (++executed <= limit || limit <= 0) Run("Element instancer + custom elements", TestElementInstancerAndCustom);
                if (++executed <= limit || limit <= 0) Run("Custom document New/Free", TestCustomDocument);
                if (++executed <= limit || limit <= 0) Run("Render pipeline", TestRender);
                if (createWindow && OperatingSystem.IsWindows() && (++executed <= limit || limit <= 0))
                    Run("Hidden window creation", TestWindow);
            }
            finally
            {
                Run("Cleanup (remove context, shutdown)", TestCleanup);
            }

            Console.WriteLine();
            Console.WriteLine($"Passed: {_passCount}, Failed: {_failCount}");
            foreach (string failure in _failures)
            {
                Console.Error.WriteLine($"FAILED: {failure}");
            }

            // Force finalization while the main thread is still alive to surface
            // any teardown issues with a usable stack trace.
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            return _failCount == 0 ? 0 : 1;
        }

        private static void Run(string name, Action test)
        {
            try
            {
                test();
                _passCount++;
                Console.WriteLine($"PASS: {name}");
            }
            catch (Exception ex)
            {
                _failCount++;
                _failures.Add($"{name}: {ex.Message}");
                Console.WriteLine($"FAIL: {name}: {ex.Message}");
            }
        }

        private static void Assert(bool condition, string message)
        {
            if (!condition) throw new Exception(message);
        }

        private static void AssertEqual<T>(T expected, T actual, string message)
        {
            if (!EqualityComparer<T>.Default.Equals(expected, actual))
                throw new Exception($"{message} (expected '{expected}', got '{actual}')");
        }

        // ------------------------------------------------------------------
        // Test stubs
        // ------------------------------------------------------------------

        private class TestRenderInterface : RenderInterface
        {
            public int GeometryCompiled;
            public int GeometryRendered;

            public override IntPtr CompileGeometry(Vertex* vertices, int vertexCount, int* indices, int indexCount)
            {
                GeometryCompiled++;
                return new IntPtr(0x1234);
            }

            public override void RenderGeometry(IntPtr geometry, Vector2f translation, IntPtr texture)
            {
                GeometryRendered++;
            }

            public override void Test()
            {
                Native.RenderInterface.Test(NativePtr);
            }
        }

        private class TestSystemInterface : SystemInterface
        {
            public override double ElapsedTime => 0.016;

            public override void SetClipboardText(string text) { }

            public override string GetClipboardText() => "";

            public override void ActivateKeyboard(float caretX, float caretY, float lineHeight) { }

            public override void DeactivateKeyboard() { }
        }

        private sealed class TestFileInterface : FileInterface
        {
            private readonly Dictionary<string, byte[]> _files = new();
            private readonly Dictionary<long, byte[]> _openFiles = new();
            private readonly Dictionary<long, long> _positions = new();
            private long _nextHandle = 1;

            public void AddFile(string path, string content) =>
                _files[path] = System.Text.Encoding.UTF8.GetBytes(content);

            public override IntPtr Open(string path)
            {
                if (!_files.TryGetValue(path, out byte[]? data)) return IntPtr.Zero;
                long handle = _nextHandle++;
                _openFiles[handle] = data;
                _positions[handle] = 0;
                return new IntPtr(handle);
            }

            public override void Close(IntPtr file)
            {
                _openFiles.Remove(file.ToInt64());
                _positions.Remove(file.ToInt64());
            }

            public override ulong Read(ulong size, IntPtr file, out byte[] bytes)
            {
                long handle = file.ToInt64();
                if (!_openFiles.TryGetValue(handle, out byte[]? data) || !_positions.TryGetValue(handle, out long position))
                {
                    bytes = Array.Empty<byte>();
                    return 0;
                }

                long remaining = data.LongLength - position;
                int count = (int)Math.Min((long)size, remaining);
                bytes = new byte[count];
                Array.Copy(data, position, bytes, 0, count);
                _positions[handle] = position + count;
                return (ulong)count;
            }

            public override bool Seek(IntPtr file, uint offset, int origin)
            {
                long handle = file.ToInt64();
                if (!_openFiles.TryGetValue(handle, out byte[]? data) || !_positions.TryGetValue(handle, out long position))
                    return false;

                long target = origin switch
                {
                    0 => offset,
                    1 => position + offset,
                    2 => data.LongLength + offset,
                    _ => -1,
                };
                if (target < 0) return false;
                _positions[handle] = target;
                return true;
            }

            public override ulong Tell(IntPtr file)
            {
                long handle = file.ToInt64();
                return _positions.TryGetValue(handle, out long position) ? (ulong)position : 0;
            }

            public override ulong Length(IntPtr file)
            {
                long handle = file.ToInt64();
                return _openFiles.TryGetValue(handle, out byte[]? data) ? (ulong)data.LongLength : 0;
            }

            public override string LoadFile(string path) =>
                _files.TryGetValue(path, out byte[]? data)
                    ? System.Text.Encoding.UTF8.GetString(data)
                    : string.Empty;
        }

        private class TestPlugin : Plugin
        {
            public bool Initialised;
            public bool ShutDown;
            public bool DocumentLoaded;

            public override void OnInitialise() => Initialised = true;
            public override void OnShutdown() => ShutDown = true;
            public override void OnDocumentLoad(ElementDocument context) => DocumentLoaded = true;
        }

        private class TestEventListener : EventListener
        {
            public Event? LastEvent;
            public bool Attached;
            public bool Detached;
            public EventId CapturedId;
            public EventPhase CapturedPhase;
            public bool CapturedInterruptible;
            public bool CapturedPropagating;
            public bool CapturedImmediatePropagating;
            public Dictionary<string, object> CapturedParameters = [];

            public override void ProcessEvent(Event ev)
            {
                // The event object is owned by RmlUi and released as soon as the dispatch
                // completes; everything must be captured here, inside the callback.
                LastEvent = ev;
                CapturedId = ev.Id;
                CapturedPhase = ev.Phase;
                CapturedInterruptible = ev.IsInterruptible;
                CapturedPropagating = ev.IsPropagating;
                CapturedImmediatePropagating = ev.IsImmediatePropagating;
                CapturedParameters = ev.Parameters;
                _ = ev.TargetElement;
                _ = ev.CurrentElement;
                ev.StopPropagation();
                ev.StopImmediatePropagation();
            }

            public override void OnAttach(RmlUiNet.Element element) => Attached = true;
            public override void OnDetach(RmlUiNet.Element element) => Detached = true;
        }

        private class TestEventInstancer : EventInstancer
        {
            public int Instanced;

            public override void OnInstanceEvent(RmlUiNet.Element element, EventId id, string name, Dictionary<string, object> parameters, bool interruptible)
            {
                Instanced++;
            }
        }

        private class TestEventListenerInstancer : EventListenerInstancer
        {
            public override EventListener OnInstanceElement(string value, RmlUiNet.Element element) => new TestEventListener();
        }

        private class TestElementInstancer : ElementInstancer
        {
            public int Instanced;

            public TestElementInstancer(string tag) : base(tag) { }

            public override IntPtr OnInstanceElement(RmlUiNet.Element parent, string tag, XMLAttributes attributes)
            {
                Instanced++;
                return Native.ElementCustom.Create(tag, null);
            }

            public override void OnReleaseElement(RmlUiNet.Element element)
            {
                Native.ElementCustom.Free(element.NativePtr);
            }
        }

        private class TestCustomElement : ElementCustom
        {
            public bool Rendered;

            public TestCustomElement(string tag) : base(tag) { }

            public override void OnRender() => Rendered = true;
        }

        private class TestVariableDefinition : VariableDefinition
        {
            public TestVariableDefinition() : base(DataVariableType.Scalar) { }

            public override DataVariable Child(IntPtr ptr, string name, int index) => new(this, ptr);

            public override int Size(IntPtr ptr) => 1;

            public override bool Set(IntPtr ptr, Variant variant) => true;

            public override bool Get(IntPtr ptr, out Variant variant)
            {
                variant = new Variant(7);
                return true;
            }
        }

        // ------------------------------------------------------------------
        // Tests
        // ------------------------------------------------------------------

        private static void TestInitialise()
        {
            _renderInterface = new TestRenderInterface();
            _systemInterface = new TestSystemInterface();
            _fileInterface = new TestFileInterface();
            _fileInterface.AddFile("smoke.rcss", "body { width: 100px; background-color: #ff0000; }");

            Assert(Rml.Initialise(), "Rml.Initialise() failed");
            Rml.SetRenderInterface(_renderInterface);
            Rml.SetSystemInterface(_systemInterface);
            Rml.SetFileInterface(_fileInterface);
            Assert(Rml.GetRenderInterface().NativePtr == _renderInterface.NativePtr, "GetRenderInterface mismatch");

            _plugin = new TestPlugin();
            Rml.RegisterPlugin(_plugin);
            Assert(_plugin.Initialised, "Plugin was not initialised");

            // Covers rml_LoadFontFace; the missing file is reported through the system
            // interface and must not crash the binding.
            _ = Rml.LoadFontFace("__missing_font__.ttf");
        }

        private static void TestLoggingAndEventType()
        {
            Rml.Log("smoke log", LogType.Always);
            Rml.Log("smoke error", LogType.Error);

            EventId id = Rml.RegisterEventType("smoke_event", false, true, DefaultActionPhase.Target);
            Assert(id != EventId.Invalid, "RegisterEventType returned Invalid");
        }

        private static void TestVariants()
        {
            AssertEqual(VariantType.INT, new Variant(42).Type, "int variant type");
            AssertEqual(42, (int)new Variant(42).Value!, "int variant value");
            AssertEqual(VariantType.UINT, new Variant(42u).Type, "uint variant type");
            AssertEqual(42u, (uint)new Variant(42u).Value!, "uint variant value");
            AssertEqual(VariantType.FLOAT, new Variant(1.5f).Type, "float variant type");
            AssertEqual(1.5f, (float)new Variant(1.5f).Value!, "float variant value");
            AssertEqual(VariantType.DOUBLE, new Variant(2.5).Type, "double variant type");
            AssertEqual(2.5, (double)new Variant(2.5).Value!, "double variant value");
            AssertEqual(VariantType.BOOL, new Variant(true).Type, "bool variant type");
            AssertEqual(true, (bool)new Variant(true).Value!, "bool variant value");
            AssertEqual(VariantType.STRING, new Variant("hello").Type, "string variant type");
            AssertEqual("hello", (string)new Variant("hello").Value!, "string variant value");

            // Exercise rml_Variant_Free directly (the managed wrapper keeps no ownership).
            IntPtr rawVariant = Native.Variant.CreateInt(9);
            Assert(rawVariant != IntPtr.Zero, "CreateInt returned null");
            Native.Variant.Free(rawVariant);
        }

        private static void TestDictionaryApis()
        {
            IntPtr dict = Native.RmlDictionary.Create();
            Assert(dict != IntPtr.Zero, "Dictionary.Create returned null");
            try
            {
                var intVariant = new Variant(42);
                var stringVariant = new Variant("forty-two");
                try
                {
                    Native.RmlDictionary.Insert(dict, "int_key", intVariant.NativePtr);
                    Native.RmlDictionary.Insert(dict, "str_key", stringVariant.NativePtr);
                    AssertEqual(2, Native.RmlDictionary.Size(dict), "dictionary size");

                    AssertEqual(42, (int)new Variant(Native.RmlDictionary.Get(dict, "int_key")).Value!, "dictionary get int");
                    AssertEqual("forty-two", (string)new Variant(Native.RmlDictionary.Get(dict, "str_key")).Value!, "dictionary get string");

                    string[] keys = Native.RmlDictionary.RetrieveKeys(dict);
                    AssertEqual(2, keys.Length, "RetrieveKeys count");

                    IntPtr strKeyValue = Native.XMLAttributes.GetString(dict, "str_key", "default");
                    AssertEqual("forty-two", Marshal.PtrToStringAnsi(strKeyValue) ?? "", "dictionary GetString");
                    Native.Rml.FreeStringBuffer(strKeyValue);

                    IntPtr missingValue = Native.XMLAttributes.GetString(dict, "missing", "default");
                    AssertEqual("default", Marshal.PtrToStringAnsi(missingValue) ?? "", "dictionary GetString default");
                    Native.Rml.FreeStringBuffer(missingValue);
                }
                finally
                {
                    Native.Variant.Free(intVariant.NativePtr);
                    Native.Variant.Free(stringVariant.NativePtr);
                }
            }
            finally
            {
                Native.RmlDictionary.Free(dict);
            }
        }

        private static void TestContext()
        {
            Context context = Rml.CreateContext("smoke", new Vector2i(1280, 720), _renderInterface) ?? throw new Exception("CreateContext returned null");
            _context = context;

            context.Update();
            context.SetDimensions(800, 600);
            AssertEqual("smoke", context.Name, "context name");
            _ = context.IsMouseInteracting;

            _ = context.ProcessMouseMove(100, 100, KeyModifier.None);
            _ = context.ProcessMouseButtonDown(0, KeyModifier.None);
            _ = context.ProcessMouseButtonUp(0, KeyModifier.None);
            _ = context.ProcessMouseWheel(new Vector2f(0, -1), KeyModifier.None);
            _ = context.ProcessKeyDown(KeyIdentifier.KI_A, KeyModifier.None);
            _ = context.ProcessKeyUp(KeyIdentifier.KI_A, KeyModifier.None);
            _ = context.ProcessTextInput("a");
            _ = context.ProcessMouseLeave();
        }

        private static void TestDocument()
        {
            const string rml =
                "<rml><head><title>Smoke</title></head><body id=\"body\">" +
                "<div id=\"div1\" class=\"box\"><p id=\"p1\">Hello</p></div>" +
                "<form><input id=\"inp\" type=\"text\" value=\"hello\"/></form>" +
                "<button id=\"btn\">Click me</button>" +
                "</body></rml>";

            _document = _context!.LoadDocumentFromMemory(rml, "smoke.rml");
            ElementDocument doc = _document ?? throw new Exception("LoadDocumentFromMemory returned null");

            AssertEqual("Smoke", doc.GetTitle(), "document title");
            doc.SetTitle("Smoke Two");
            AssertEqual("Smoke Two", doc.GetTitle(), "document title after SetTitle");
            AssertEqual("smoke.rml", doc.GetSourceURL(), "document source URL");

            doc.PullToFront();
            doc.Show();
            doc.Show(ModalFlag.Modal);
            doc.Hide();

            // Style sheet container load + merge into the document.
            var styleSheet = new StyleSheetContainer("smoke.rcss");
            try
            {
                doc.AddStyleSheetContainer(styleSheet);
            }
            finally
            {
                styleSheet.Dispose();
            }

            // GetRootElement / GetFocusElement / GetHoverElement / UnfocusDocument (all missing in 1.0.1).
            Element? root = _context.GetRootElement();
            Assert(root is not null, "GetRootElement returned null");
            _ = _context.GetFocusElement();
            _ = _context.GetHoverElement();
            _context.UnfocusDocument(doc);
        }

        private static void TestElementBasics()
        {
            ElementDocument document = _document!;
            ElementGeneric div = document.GetElementById("div1") as ElementGeneric ?? throw new Exception("GetElementById(div1) returned null");

            AssertEqual("div", div.TagName, "tag name");

            Assert(!string.IsNullOrEmpty(div.GetInnerRml()), "GetInnerRml empty");
            div.SetInnerRml("<p id=\"inner-p\">Inner</p>");
            Assert(div.GetElementById("inner-p") is not null, "SetInnerRml child missing");

            // rml_Element_GetElementTypeName (missing in 1.0.1).
            string typeName = div.GetElementTypeName();
            Assert(typeName.Contains("Element"), $"unexpected type name '{typeName}'");

            div.Focus();
            div.Blur();
            div.Click();

            _ = div.GetOffsetTop();
            _ = div.GetAbsoluteLeft();
            _ = div.GetAbsoluteTop();
            _ = div.GetClientWidth();
            _ = div.GetClientHeight();

            // RmlUi merges the body element's attributes onto the document root element,
            // so the parent of any top-level element is the ElementDocument itself.
            RmlUiNet.Element? parent = div.GetParentNode();
            Assert(parent is ElementDocument, "GetParentNode should return the document root");

            ElementGeneric? found = div.QuerySelector("#inner-p") as ElementGeneric;
            Assert(found is not null, "QuerySelector returned null");
        }

        private static void TestElementTree()
        {
            // The document root is the parent of all top-level elements (see TestElementBasics).
            ElementDocument root = _document!;

            // AppendChildTag + RemoveChild + AppendChild + ReplaceChild (all missing in 1.0.1).
            ElementGeneric span = (ElementGeneric?)root.AppendChildTag("span") ?? throw new Exception("AppendChildTag failed");
            AssertEqual("span", span.TagName, "appended child tag");

            root.RemoveChild(span);
            ElementGeneric reappended = (ElementGeneric?)root.AppendChild(span) ?? throw new Exception("AppendChild failed");

            ElementGeneric em = (ElementGeneric?)root.AppendChildTag("em") ?? throw new Exception("AppendChildTag(em) failed");
            root.ReplaceChild(em, span);

            // Document-level CreateElement + CreateTextNode (missing in 1.0.1).
            IntPtr created = Native.ElementDocument.CreateElement(root.NativePtr, "strong");
            Assert(created != IntPtr.Zero, "CreateElement returned null");
            ElementGeneric strong = ElementGeneric.Create(created) ?? throw new Exception("Failed to wrap created element");
            root.AppendChild(strong);

            IntPtr textNode = Native.ElementDocument.CreateTextNode(root.NativePtr, "some text");
            Assert(textNode != IntPtr.Zero, "CreateTextNode returned null");
            root.AppendChild(ElementGeneric.Create(textNode) ?? throw new Exception("Failed to wrap text node"));

            strong.ScrollTo(0, 0, ScrollBehavior.Instant);
        }

        private static void TestAttributesAndProperties()
        {
            ElementDocument document = _document!;
            ElementGeneric div = document.GetElementById("div1") as ElementGeneric ?? throw new Exception("div1 not found");

            // The empty-string fix: SetAttribute/SetProperty must accept "".
            div.SetAttribute("data-empty", "");
            Assert(div.HasAttribute("data-empty"), "HasAttribute(data-empty) after empty set");
            AssertEqual("", div.GetAttribute("data-empty"), "empty attribute value");

            div.SetAttribute("data-value", "42");
            AssertEqual("42", div.GetAttribute("data-value"), "attribute value");
            div.RemoveAttribute("data-value");
            Assert(!div.HasAttribute("data-value"), "attribute still present after remove");

            div.AddClass("extra");
            Assert(div.HasClass("extra"), "HasClass after AddClass");
            div.RemoveClass("extra");
            Assert(!div.HasClass("extra"), "HasClass after RemoveClass");

            div.SetProperty("width", "50px");
            AssertEqual("50px", div.GetProperty("width"), "property value");
            div.SetProperty("width", ""); // empty value is legal

            // rml_Element_GetProperty + rml_Property_Get / rml_Property_GetString (missing in 1.0.1).
            IntPtr propertyPtr = Native.Element.GetProperty(div.NativePtr, "width");
            Assert(propertyPtr != IntPtr.Zero, "GetProperty returned null");
            var property = new Property(propertyPtr, false);
            AssertEqual("50px", property.GetString("none"), "Property.GetString");
            Assert(property.Get() is not null, "Property.Get returned null");
        }

        private static void TestFormControls()
        {
            ElementDocument document = _document!;
            Element? inputElement = document.GetElementById("inp");
            Assert(inputElement is ElementFormControlInput, "input is not ElementFormControlInput");

            var input = (ElementFormControlInput)inputElement!;
            AssertEqual("hello", input.GetValue(), "input initial value");
            input.SetValue("world");
            AssertEqual("world", input.GetValue(), "input value after SetValue");
        }

        private static void TestEvents()
        {
            ElementDocument document = _document!;
            ElementGeneric button = document.GetElementById("btn") as ElementGeneric ?? throw new Exception("button not found");

            var listener = new TestEventListener();
            try
            {
                button.AddEventListener("click", listener);
                Assert(listener.Attached, "listener was not attached");

                button.Click();
                Assert(listener.LastEvent is not null, "click listener did not receive the event");
                AssertEqual(EventId.Click, listener.CapturedId, "event id");
                AssertEqual(EventPhase.Target, listener.CapturedPhase, "event phase");
                _ = listener.CapturedInterruptible;
                _ = listener.CapturedPropagating;
                _ = listener.CapturedImmediatePropagating;

                button.RemoveEventListener("click", listener);
            }
            finally
            {
                listener.Dispose();
            }

            // DispatchEvent with parameters: exercises rml_Element_DispatchEvent,
            // rml_Event_GetParameters and the full dictionary round-trip.
            var capture = new TestEventListener();
            try
            {
                button.AddEventListener("smoke_event", capture);
                bool dispatched = button.DispatchEvent("smoke_event", new Dictionary<string, object> { ["value"] = 42 }, false, true);
                Assert(dispatched, "DispatchEvent returned false");
                Assert(capture.LastEvent is not null, "event listener did not receive dispatched event");

                // The parameters were captured inside the callback (the event is freed after dispatch).
                Dictionary<string, object> parameters = capture.CapturedParameters;
                Assert(parameters.ContainsKey("value"), "event parameters missing 'value'");
                AssertEqual(42, parameters["value"], "event parameter value");
            }
            finally
            {
                // Remove the listener before disposal: the dispatcher holds a raw pointer
                // to the native listener until it is removed.
                button.RemoveEventListener("smoke_event", capture);
                capture.Dispose();
            }
        }

        private static void TestDataModelScalars()
        {
            var dataModel = _context!.CreateDataModel("smoke_model");
            try
            {
                Assert(dataModel.BindFloat("f", 1.5f), "BindFloat");
                Assert(dataModel.BindInt("i", 3), "BindInt");
                Assert(dataModel.BindUInt("u", 4), "BindUInt");
                Assert(dataModel.BindBool("b", true), "BindBool");
                Assert(dataModel.BindString("s", "one"), "BindString");
                Assert(dataModel.BindString("s", "two"), "BindString update");

                dataModel.Handle.DirtyAllVariables();
                dataModel.Handle.DirtyVariable("f");
                dataModel.Handle.IsVariableDirty("f");

                Assert(dataModel.Handle.NativePtr != IntPtr.Zero, "model handle is null");
            }
            finally
            {
                dataModel.Dispose();
            }
        }

        private static void TestDataModelStringArrayAndStruct()
        {
            var dataModel = _context!.CreateDataModel("smoke_model_2");
            try
            {
                // rml_DataModelConstructor_BindStringArray / UnbindStringArray (missing in 1.0.1).
                string[] strings = { "alpha", "beta" };
                IntPtr[] stringPointers = new IntPtr[strings.Length];
                IntPtr arrayPtr = Marshal.AllocHGlobal(IntPtr.Size * strings.Length);
                try
                {
                    for (int i = 0; i < strings.Length; i++)
                    {
                        stringPointers[i] = Marshal.StringToHGlobalAnsi(strings[i]);
                        Marshal.WriteIntPtr(arrayPtr, i * IntPtr.Size, stringPointers[i]);
                    }

                    Assert(Native.DataModelConstructor.BindStringArray(dataModel.NativePtr, "arr", arrayPtr, strings.Length), "BindStringArray");
                    Assert(Native.DataModelConstructor.UnbindStringArray(dataModel.NativePtr, "arr"), "UnbindStringArray");
                }
                finally
                {
                    for (int i = 0; i < stringPointers.Length; i++)
                    {
                        Marshal.FreeHGlobal(stringPointers[i]);
                    }
                    Marshal.FreeHGlobal(arrayPtr);
                }

                // rml_DataModelConstructor_RegisterStruct (missing in 1.0.1).
                int structId = dataModel.RegisterStruct("smoke_struct");
                Assert(structId != 0, "RegisterStruct returned zero");
                IntPtr structHandle = Native.DataModelConstructor.RegisterStruct(dataModel.NativePtr, "smoke_struct");
                Assert(structHandle != IntPtr.Zero, "RegisterStruct returned null handle");
            }
            finally
            {
                dataModel.Dispose();
            }
        }

        private static void TestDataModelVariable()
        {
            var dataModel = _context!.CreateDataModel("smoke_model_3");
            try
            {
                var definition = new TestVariableDefinition();
                var dataVariable = new DataVariable(definition, IntPtr.Zero);
                Assert(dataModel.BindVariable("custom", dataVariable), "BindVariable");

                bool callbackFired = false;
                Assert(dataModel.BindEventCallback("on_change", (ev, variants) => callbackFired = true), "BindEventCallback");
                Assert(!callbackFired, "event callback fired unexpectedly");

                dataModel.Handle.DirtyVariable("custom");
                dataVariable.Dispose();
            }
            finally
            {
                dataModel.Dispose();
            }

            Assert(_context.GetDataModel("smoke_model_3") is not null, "GetDataModel should return the model still owned by the context");
            Assert(_context.RemoveDataModel("smoke_model_3"), "RemoveDataModel should return true");
        }

        private static void TestDebugger()
        {
            Debugger.Initialise(_context!);
            Debugger.SetVisible(true);
            Assert(Debugger.IsVisible(), "debugger should be visible after SetVisible(true)");
            Debugger.SetVisible(false);
            Debugger.SetContext(_context!);
            Debugger.Shutdown();
        }

        private static void TestStyleSheetSpecification()
        {
            var definition = StyleSheetSpecification.RegisterProperty("smoke-prop", "auto", false, false);
            if (definition is null) throw new Exception("RegisterProperty returned null");
            definition.AddParser("keyword", "");
            Assert(definition.GetId() != 0, "PropertyDefinition.GetId returned 0");

            // rml_PropertyDefinition_GetValueString + rml_Element_GetPropertyById (missing in 1.0.1).
            ElementGeneric div = _document!.GetElementById("div1") as ElementGeneric ?? throw new Exception("div1 not found");

            // The element has no local 'smoke-prop' property, so the lookup must return null
            // and GetValueString must fall back to the provided default.
            IntPtr propertyPtr = Native.Element.GetPropertyById(div.NativePtr, definition.GetId());
            IntPtr valuePtr = Native.PropertyDefinition.GetValueString(definition.NativePtr, propertyPtr, "fallback");
            try
            {
                string value = Marshal.PtrToStringAnsi(valuePtr) ?? "";
                AssertEqual("fallback", value, "PropertyDefinition.GetValueString fallback");
            }
            finally
            {
                Native.Rml.FreeStringBuffer(valuePtr);
            }
        }

        private static void TestInstancers()
        {
            // rml_EventInstancer_New (missing in 1.0.1). The instances stay alive until
            // TestCleanup: Rml::Factory keeps referencing them, so they cannot be freed
            // before Rml.Shutdown().
            _eventInstancer = new TestEventInstancer();

            // rml_EventListenerInstancer_New (missing in 1.0.1).
            _listenerInstancer = new TestEventListenerInstancer();

            // rml_ElementInstancer_New: create an element through the custom tag.
            // The element stays in the detached-element pool until shutdown, which then
            // releases it through the instancer's OnReleaseElement callback.
            _elementInstancer = new TestElementInstancer("smoke-tag");
            IntPtr elementPtr = Native.ElementDocument.CreateElement(_document!.NativePtr, "smoke-tag");
            Assert(elementPtr != IntPtr.Zero, "instanced element is null");
            Assert(_elementInstancer.Instanced == 1, "instancer was not called");
        }

        private static void TestElementInstancerAndCustom()
        {
            // ElementInstancer coverage lives in TestInstancers (the instance must stay
            // alive until shutdown because Rml::Factory keeps a raw reference to it).

            // ElementCustom New/Free via the managed wrapper.
            var custom = new TestCustomElement("smoke-custom");
            try
            {
                AssertEqual("smoke-custom", custom.TagName, "custom element tag");
            }
            finally
            {
                custom.Dispose();
            }
        }

        private static void TestCustomDocument()
        {
            // rml_ElementDocument_New / Free (missing in 1.0.1).
            IntPtr customDocument = Native.ElementDocument.Create(null, null);
            Assert(customDocument != IntPtr.Zero, "ElementDocument_New returned null");
            Native.ElementDocument.Free(customDocument);
        }

        private static void TestRender()
        {
            _document!.Show();
            _context!.Update();
            _context.Render();
            Assert(_renderInterface!.GeometryRendered > 0, "no geometry was rendered");
        }

        private static void TestWindow()
        {
            IntPtr hwnd = Win32.CreateMessageWindow();
            Assert(hwnd != IntPtr.Zero, "CreateWindowEx failed");
            Assert(Win32.DestroyWindow(hwnd), "DestroyWindow failed");
        }

        private static void TestCleanup()
        {
            if (_document is not null)
            {
                _document.Close();
                _document.Dispose();
                _document = null;
            }

            if (_context is not null)
            {
                _context.Update();
                _context.Dispose();
                _context = null;
            }

            Rml.RemoveContext("smoke");

            if (_plugin is not null)
            {
                _plugin.Dispose();
                _plugin = null;
            }

            _renderInterface?.Dispose();
            _systemInterface?.Dispose();
            _fileInterface?.Dispose();

            Rml.Shutdown();

            // Free the instancers only after shutdown: Rml::Factory holds raw pointers to
            // them for the whole lifetime of the library.
            _eventInstancer?.Dispose();
            _listenerInstancer?.Dispose();
            _elementInstancer?.Dispose();
            _eventInstancer = null;
            _listenerInstancer = null;
            _elementInstancer = null;
        }

        private static class Win32
        {
            private const string User32 = "user32.dll";

            [DllImport(User32, CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr CreateWindowExW(
                int dwExStyle,
                string lpClassName,
                string lpWindowName,
                int dwStyle,
                int x,
                int y,
                int nWidth,
                int nHeight,
                IntPtr hWndParent,
                IntPtr hMenu,
                IntPtr hInstance,
                IntPtr lpParam);

            [DllImport(User32, EntryPoint = "DestroyWindow")]
            private static extern bool DestroyWindowInternal(IntPtr hWnd);

            private const int HWND_MESSAGE = -3;

            public static IntPtr CreateMessageWindow()
            {
                // Message-only window: no desktop session or visibility required.
                return CreateWindowExW(0, "STATIC", "RmlUi.Net smoke test", 0, 0, 0, 0, 0,
                    new IntPtr(HWND_MESSAGE), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
            }

            public static bool DestroyWindow(IntPtr hWnd) => DestroyWindowInternal(hWnd);
        }
    }
}
