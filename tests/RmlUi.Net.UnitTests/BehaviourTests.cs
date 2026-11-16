using System;
using System.Collections.Generic;
using System.Linq;
using RmlUiNet;
using RmlUiNet.Input;
using Xunit;

namespace RmlUi.Net.UnitTests;

/// <summary>
/// Event dispatch through the managed listener shim. The important cases here are the ones
/// the binding got wrong: custom event types never reached their handler, and the propagation
/// phase was reported with the wrong numeric value.
/// </summary>
[Collection(RmlUiCollection.Name)]
public sealed class EventTests : DocumentTestBase
{
    public EventTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void Builtin_click_reaches_a_listener_with_type_and_phase()
    {
        Element span = Scope.Find("span");
        var seen = new List<(string Type, EventPhase Phase, string? Target, EventId Id)>();

        span.AddEventListener("click", e => seen.Add((e.Type, e.Phase, e.TargetElement?.Id, e.Id)));

        span.Click();

        var only = Assert.Single(seen);
        Assert.Equal("click", only.Type);
        Assert.Equal(EventPhase.Target, only.Phase);
        Assert.Equal("span", only.Target);
        Assert.Equal(EventId.Click, only.Id);
    }

    [Fact]
    public void Phase_values_match_the_native_flags()
    {
        // RmlUi declares EventPhase as a bit-flag enum (Capture = 1, Target = 2, Bubble = 4).
        // The managed enum used to renumber them densely, so a bubbled event reported "4" as
        // Bubble while the binding read it as something else entirely.
        Assert.Equal(1, (int)EventPhase.Capture);
        Assert.Equal(2, (int)EventPhase.Target);
        Assert.Equal(4, (int)EventPhase.Bubble);
        Assert.Equal(0, (int)EventPhase.None);

        Assert.Equal(EventPhase.None, EventPhase.None);
        Assert.True(Enum.IsDefined(typeof(EventPhase), EventPhase.Bubble));
    }

    [Fact]
    public void Bubbling_listener_reports_the_bubble_phase_and_original_target()
    {
        Element root = Root;
        Element span = Scope.Find("span");
        var phases = new List<(string? Current, EventPhase Phase, string? Target)>();

        root.AddEventListener("click", e => phases.Add((e.CurrentElement?.Id, e.Phase, e.TargetElement?.Id)));
        span.Click();

        var bubbled = Assert.Single(phases);
        Assert.Equal("root", bubbled.Current);
        Assert.Equal(EventPhase.Bubble, bubbled.Phase);
        Assert.Equal("span", bubbled.Target);
    }

    [Fact]
    public void Custom_event_type_reaches_its_handler()
    {
        // The handler shim used to look listeners up by ev.Id.ToString(), which is a numeric
        // string for ids allocated by RegisterEventType. Custom events therefore silently never
        // fired. Lookup is by ev.Type now.
        Element span = Scope.Find("span");
        string type = $"unit_custom_{Guid.NewGuid():N}";

        EventId id = Rml.RegisterEventType(type, interruptible: true, bubbles: true, DefaultActionPhase.Target);
        Assert.NotEqual(EventId.Invalid, id);

        int hits = 0;
        span.AddEventListener(type, _ => hits++);

        bool propagating = span.DispatchEvent(type, new Dictionary<string, object>(), interruptible: true, bubbles: true);

        Assert.Equal(1, hits);
        Assert.True(propagating);

        // The id is allocated past the built-in range, so it is not one of the named members.
        Assert.DoesNotContain(id, new[] { EventId.Click, EventId.MouseDown, EventId.TabChange });
    }

    [Fact]
    public void DispatchEvent_parameters_are_readable_in_the_handler()
    {
        Element span = Scope.Find("span");
        string type = $"unit_params_{Guid.NewGuid():N}";
        Rml.RegisterEventType(type, interruptible: true, bubbles: false, DefaultActionPhase.None);

        Dictionary<string, object>? received = null;
        span.AddEventListener(type, e => received = e.Parameters);

        span.DispatchEvent(type, new Dictionary<string, object>
        {
            ["text"] = "hello",
            ["count"] = 7,
            ["ratio"] = 0.5f,
        }, interruptible: true, bubbles: false);

        Assert.NotNull(received);
        Assert.Equal("hello", received!["text"]);
        Assert.Equal(7, received["count"]);
        Assert.Equal(0.5f, received["ratio"]);
    }

    [Fact]
    public void StopPropagation_prevents_ancestors_from_seeing_the_event()
    {
        Element root = Root;
        Element span = Scope.Find("span");
        string type = $"unit_stop_{Guid.NewGuid():N}";
        Rml.RegisterEventType(type, interruptible: true, bubbles: true, DefaultActionPhase.None);

        int childHits = 0;
        int parentHits = 0;
        span.AddEventListener(type, e =>
        {
            childHits++;
            e.StopPropagation();
        });
        root.AddEventListener(type, _ => parentHits++);

        bool propagating = span.DispatchEvent(type, new Dictionary<string, object>(), interruptible: true, bubbles: true);

        Assert.Equal(1, childHits);
        Assert.Equal(0, parentHits);
        Assert.False(propagating);
    }

    [Fact]
    public void DispatchEvent_without_listeners_reports_propagating()
    {
        // RmlUi returns "still propagating" (true) when there is nobody to consume the event,
        // so the return value cannot be treated as "was delivered".
        Element span = Scope.Find("span");
        string type = $"unit_nobody_{Guid.NewGuid():N}";
        Rml.RegisterEventType(type, interruptible: true, bubbles: true, DefaultActionPhase.None);

        Assert.True(span.DispatchEvent(type, new Dictionary<string, object>(), interruptible: true, bubbles: true));
    }

    [Fact]
    public void Removing_a_handler_stops_delivery()
    {
        Element span = Scope.Find("span");
        string type = $"unit_remove_{Guid.NewGuid():N}";
        Rml.RegisterEventType(type, interruptible: true, bubbles: false, DefaultActionPhase.None);

        int hits = 0;
        Action<Event> handler = _ => hits++;

        span.AddEventListener(type, handler);
        span.DispatchEvent(type, new Dictionary<string, object>(), interruptible: true, bubbles: false);
        Assert.Equal(1, hits);

        span.RemoveEventListener(type, handler);
        span.DispatchEvent(type, new Dictionary<string, object>(), interruptible: true, bubbles: false);
        Assert.Equal(1, hits);
    }

    [Fact]
    public void Listener_registration_is_case_insensitive()
    {
        Element span = Scope.Find("span");
        int hits = 0;

        span.AddEventListener("CLICK", _ => hits++);
        span.Click();

        Assert.Equal(1, hits);
    }

    [Fact]
    public void Event_arguments_are_validated()
    {
        Element span = Scope.Find("span");

        Assert.Throws<ArgumentNullException>(() => span.AddEventListener("", _ => { }));
        Assert.Throws<ArgumentNullException>(() => span.DispatchEvent("", new Dictionary<string, object>(), true, true));
    }
}

/// <summary>Selectors, and the places where RmlUi's parser deviates from CSS.</summary>
[Collection(RmlUiCollection.Name)]
public sealed class SelectorTests : DocumentTestBase
{
    public SelectorTests(RmlUiFixture fixture) : base(fixture) { }

    public const string Markup = """
        <rml>
          <head>
            <title>Selectors</title>
            <style>
              body { font-family: 'LatoLatin'; }
              #list { display: block; }
              .item { display: block; }
            </style>
          </head>
          <body id="body">
            <div id="list">
              <span id="s1" class="item" data-kind="alpha">1</span>
              <span id="s2" class="item" data-kind="beta">2</span>
              <span id="s3" class="item">3</span>
              <DIV id="upper">4</DIV>
            </div>
            <div id="empty"></div>
          </body>
        </rml>
        """;

    private DocumentScope OpenList() => Fixture.OpenDocument(Markup);

    [Fact]
    public void Attribute_selectors_work_with_and_without_quotes()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Single(list.QuerySelectorAll("[data-kind=alpha]"));
        Assert.Single(list.QuerySelectorAll("[data-kind=\"alpha\"]"));
        Assert.Single(list.QuerySelectorAll("span[data-kind=alpha]"));
        Assert.Equal(2, list.QuerySelectorAll("[data-kind]").Count);
    }

    [Fact]
    public void Sibling_and_child_combinators_are_supported()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Equal(3, list.QuerySelectorAll("span").Count);
        Assert.Equal(3, list.QuerySelectorAll("div > span").Count);
        Assert.Equal(2, list.QuerySelectorAll("span + span").Count);
        Assert.Equal(2, list.QuerySelectorAll("span ~ span").Count);
        Assert.Equal(2, list.QuerySelectorAll("#s1 ~ span").Count);
    }

    [Fact]
    public void Structural_pseudo_classes_are_supported()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Single(list.QuerySelectorAll("span:first-of-type"));
        Assert.Single(list.QuerySelectorAll("span:last-of-type"));
        Assert.Single(scope.Context.GetRootElement()!.QuerySelectorAll("div:empty"));
    }

    [Fact]
    public void Selector_lists_and_wildcards_are_supported()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Equal(2, list.QuerySelectorAll("#s1, #s2").Count);
        Assert.Equal(4, list.QuerySelectorAll("*").Count);
    }

    [Fact]
    public void Tag_matching_is_case_sensitive_even_though_markup_is_lowercased()
    {
        using DocumentScope scope = OpenList();

        // The parser lowercases <DIV> to "div", but the selector comparison does not fold case.
        Assert.Equal("div", scope.Find("upper").TagName);
        Assert.Empty(scope.Document.QuerySelectorAll("DIV"));
        Assert.Single(scope.Document.QuerySelectorAll("div#upper"));
    }

    [Fact]
    public void Tag_name_queries_have_no_wildcard()
    {
        using DocumentScope scope = OpenList();

        // GetElementsByTagName compares the tag literally, so "*" matches nothing — unlike
        // QuerySelectorAll("*"), which the selector grammar handles.
        Assert.Empty(scope.Document.GetElementsByTagName("*"));
        Assert.Equal(3, scope.Document.GetElementsByTagName("span").Count);
    }

    [Fact]
    public void Text_nodes_are_children_but_not_selector_matches()
    {
        using DocumentScope scope = OpenList();
        Element s1 = scope.Find("s1");

        Assert.Equal(1, s1.NumChildren);
        Assert.Equal("#text", s1.GetChild(0)!.TagName);
        Assert.Single(s1.GetElementsByTagName("#text"));
        Assert.Empty(s1.QuerySelectorAll("*"));
    }

    [Fact]
    public void QuerySelector_returns_the_first_match_and_skips_text_nodes()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Equal("s1", list.QuerySelector("span")!.Id);
        Assert.Null(list.QuerySelector("article"));
    }

    [Fact]
    public void Selector_syntax_errors_yield_no_matches_instead_of_throwing()
    {
        using DocumentScope scope = OpenList();
        Element list = scope.Find("list");

        Assert.Empty(list.QuerySelectorAll("<"));
        Assert.Empty(list.QuerySelectorAll("::not-a-selector::"));
    }
}

/// <summary>
/// The shape of the parsed document: where the <c>rml</c>/<c>body</c> tags end up, and what
/// <c>Context.GetRootElement</c> vs <c>ElementDocument</c> mean.
/// </summary>
[Collection(RmlUiCollection.Name)]
public sealed class DocumentStructureTests : DocumentTestBase
{
    public DocumentStructureTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void The_rml_and_head_tags_do_not_become_elements()
    {
        Assert.Empty(Document.GetElementsByTagName("rml"));
        Assert.Empty(Document.GetElementsByTagName("head"));

        // The body attributes were moved onto the document itself.
        Assert.Empty(Document.GetElementsByTagName("body"));
        Assert.Equal("body", Document.TagName);
    }

    [Fact]
    public void The_body_id_identifies_the_document()
    {
        Element? found = Document.GetElementById("body");

        Assert.NotNull(found);
        Assert.IsType<ElementDocument>(found);
        Assert.Equal(Document.NativePtr, found!.NativePtr);
    }

    [Fact]
    public void Context_root_is_the_anonymous_root_not_the_document()
    {
        Element root = Scope.Context.GetRootElement()!;

        Assert.Equal("#root", root.TagName);
        Assert.IsType<ElementDocument>(root.GetFirstChild());

        // The document is the only child of the context root.
        Assert.Equal(1, root.NumChildren);
    }

    [Fact]
    public void Top_level_elements_have_the_document_as_parent()
    {
        // The fixture nests everything under #root, so walk up from #para to the outermost
        // element and check that its parent is the document itself.
        Element current = Scope.Find("para");
        while (current.GetParentNode() is { } parent && parent.TagName != "body")
        {
            current = parent;
        }

        Assert.Equal("root", current.Id);
        Assert.IsType<ElementDocument>(current.GetParentNode());
    }

    [Fact]
    public void An_element_appended_to_the_document_is_a_direct_child()
    {
        using DocumentScope scope = Fixture.OpenDocument("""
            <rml><head><title>T</title></head><body id="body"><div id="only">x</div></body></rml>
            """);

        Element only = scope.Find("only");

        Assert.Equal(1, scope.Document.NumChildren);
        Assert.IsType<ElementDocument>(only.GetParentNode());
    }

    [Fact]
    public void GetElementById_searches_the_whole_document_not_the_subtree()
    {
        // RmlUi's Element::GetElementById starts at the owner document, so calling it on a
        // nested element still finds siblings elsewhere in the tree.
        Element root = Root;

        Assert.NotNull(root.GetElementById("para"));
        Assert.Equal("para", root.GetElementById("para")!.Id);

        // The lookup is not scoped to this subtree: searching from #para finds #fixed, a
        // sibling of #para, because the search starts at the owner document.
        Element para = Scope.Find("para");
        Assert.NotNull(para.GetElementById("fixed"));

        // Subtree-scoped lookups need the selector API.
        Assert.Empty(para.QuerySelectorAll("#fixed"));
    }

    [Fact]
    public void GetElementById_understands_the_special_names()
    {
        Element para = Scope.Find("para");

        Assert.Equal(para.NativePtr, para.GetElementById("#self")!.NativePtr);
        Assert.Equal("root", para.GetElementById("#parent")!.Id);
        Assert.IsType<ElementDocument>(para.GetElementById("#document"));
    }

    [Fact]
    public void A_missing_id_returns_null()
    {
        Assert.Null(Document.GetElementById("no-such-element"));
    }
}

/// <summary>The two forms of child storage, and how the accessors disagree about them.</summary>
[Collection(RmlUiCollection.Name)]
public sealed class NonDomChildTests : DocumentTestBase
{
    public NonDomChildTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void AppendChild_without_addToDom_hides_the_child_from_dom_accessors()
    {
        Element span = Scope.Find("span");
        int domChildrenBefore = span.NumChildren;
        Element hidden = span.AppendChildTag("b", addToDom: false);

        // NumChildren counts DOM children only, so it does not move.
        Assert.Equal(domChildrenBefore, span.NumChildren);

        // The child is still in the native child vector, past the DOM range, where GetChild
        // can still reach it by raw index.
        Assert.Equal(hidden.NativePtr, span.GetChild(domChildrenBefore)!.NativePtr);

        // ...but not through the DOM-facing views. GetInnerRml serialises only the DOM range,
        // so the element's own text survives but the hidden <b> does not appear.
        Assert.Empty(span.GetElementsByTagName("b"));
        Assert.Empty(span.QuerySelectorAll("b"));
        Assert.Equal("World", span.GetInnerRml());
        Assert.DoesNotContain("<b", span.GetInnerRml());

        // It is still parented, just outside the DOM range.
        Assert.Equal(span.NativePtr, hidden.GetParentNode()!.NativePtr);
    }

    [Fact]
    public void GetChild_returns_null_past_the_native_child_count()
    {
        Element span = Scope.Find("span");

        // <span>World</span> holds a single #text child.
        Assert.NotNull(span.GetChild(0));
        Assert.Null(span.GetChild(1));
        Assert.Null(span.GetChild(99));
    }

    [Fact]
    public void GetChild_rejects_a_negative_index()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Root.GetChild(-1));
    }

    [Fact]
    public void Select_options_are_not_reachable_through_the_element_tree()
    {
        using DocumentScope scope = Fixture.OpenDocument("""
            <rml>
              <head><title>Select</title><style>body { font-family: 'LatoLatin'; }</style></head>
              <body id="body">
                <select id="select">
                  <option id="first" value="1">One</option>
                  <option id="second" value="2">Two</option>
                </select>
              </body>
            </rml>
            """);

        Element select = scope.Find("select");

        // Options live under a non-DOM container, so none of the tree queries see them.
        Assert.Equal(0, select.NumChildren);
        Assert.Null(scope.Document.GetElementById("first"));
        Assert.Empty(scope.Document.GetElementsByTagName("option"));
        Assert.Equal(string.Empty, select.GetInnerRml());
    }

    [Fact]
    public void Text_area_content_is_stored_as_the_value_attribute()
    {
        using DocumentScope scope = Fixture.OpenDocument("""
            <rml>
              <head><title>TextArea</title><style>body { font-family: 'LatoLatin'; }</style></head>
              <body id="body"><textarea id="area">hello</textarea></body>
            </rml>
            """);

        Element area = scope.Find("area");

        // XMLNodeHandlerTextArea pipes the element data into SetValue instead of creating
        // children, so the markup text ends up as the value attribute.
        Assert.IsAssignableFrom<ElementFormControlTextArea>(area);
        Assert.Equal("hello", area.GetAttribute("value"));
        Assert.Equal(0, area.NumChildren);

        // A textarea has a custom node handler, so SetInnerRml half-works: it appends a DOM
        // child, but the value the widget actually edits is untouched.
        area.SetInnerRml("replaced");
        Assert.Equal("hello", area.GetAttribute("value"));
        Assert.True(area.NumChildren > 0);
    }
}

/// <summary>Behaviour the binding has to handle because RmlUi's own API is stateful.</summary>
[Collection(RmlUiCollection.Name)]
public sealed class LifecycleEdgeCaseTests
{
    private readonly RmlUiFixture _fixture;

    public LifecycleEdgeCaseTests(RmlUiFixture fixture) => _fixture = fixture;

    [Fact]
    public void CreateContext_rejects_a_duplicate_name_by_returning_null()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        // RmlUi refuses to create a second context with a name already in use.
        Assert.Null(Rml.CreateContext(scope.Name, new Vector2i(100, 100), _fixture.RenderInterface));
    }

    [Fact]
    public void Removing_a_context_twice_reports_false_the_second_time()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Assert.True(Rml.RemoveContext(scope.Name));
        Assert.False(Rml.RemoveContext(scope.Name));

        // The scope must not try to remove it again.
        scope.Detach();
    }

    [Fact]
    public void Closing_a_document_keeps_it_alive_until_the_next_update()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        ElementDocument extra = scope.Load("""
            <rml><head><title>Extra</title></head><body id="extra"><div id="d">x</div></body></rml>
            """, "extra.rml");

        string title = extra.GetTitle();
        extra.Close();

        // Destruction is deferred to Context::Update; nothing may touch the document afterwards.
        Assert.Equal("Extra", title);
        scope.Context.Update();
    }

    [Fact]
    public void Document_title_and_source_round_trip()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Assert.Equal("Unit", scope.Document.GetTitle());

        scope.Document.SetTitle("Replaced");
        Assert.Equal("Replaced", scope.Document.GetTitle());
        Assert.EndsWith(".rml", scope.Document.GetSourceURL());
    }

    [Fact]
    public void Show_hide_and_PullToFront_are_repeatable()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        scope.Document.Show();
        scope.Document.Show(ModalFlag.Keep, FocusFlag.Keep, ScrollFlag.None);
        scope.Document.PullToFront();
        scope.Document.Hide();
        scope.Document.Show(ModalFlag.None, FocusFlag.Auto, ScrollFlag.Auto);
    }

    [Fact]
    public void Multiple_documents_can_live_in_one_context()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        ElementDocument second = scope.Load("""
            <rml><head><title>Second</title></head><body id="second"><p id="p2">two</p></body></rml>
            """, "second.rml");

        Assert.Equal("Second", second.GetTitle());
        Assert.NotNull(second.GetElementById("p2"));

        // The first document is untouched.
        Assert.NotNull(scope.Document.GetElementById("para"));
    }
}

/// <summary>
/// The wrapper cache is keyed by native address, which RmlUi recycles. These tests cover the
/// two ways that used to leak a wrong wrapper into a later lookup.
/// </summary>
[Collection(RmlUiCollection.Name)]
public sealed class WrapperCacheTests
{
    private readonly RmlUiFixture _fixture;

    public WrapperCacheTests(RmlUiFixture fixture) => _fixture = fixture;

    [Fact]
    public void Repeated_lookups_return_the_same_wrapper()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Element first = scope.Find("para");
        Element second = scope.Document.GetElementById("para")!;

        Assert.Same(first, second);
    }

    [Fact]
    public void Elements_of_different_types_can_reuse_an_address()
    {
        // Create and destroy contexts until the allocator hands the same address to a
        // different element type. Before the fix the cache kept serving the old wrapper and
        // threw InvalidCastException as soon as the type did not match.
        for (int i = 0; i < 12; i++)
        {
            using (DocumentScope scope = _fixture.OpenDocument("""
                <rml>
                  <head><title>Reuse</title><style>body { font-family: 'LatoLatin'; }</style></head>
                  <body id="body"><select id="probe"><option value="1">one</option></select></body>
                </rml>
                """))
            {
                Assert.IsAssignableFrom<ElementFormControlSelect>(scope.Find("probe"));
            }

            using (DocumentScope scope = _fixture.OpenDocument("""
                <rml>
                  <head><title>Reuse</title><style>body { font-family: 'LatoLatin'; }</style></head>
                  <body id="body"><textarea id="probe">text</textarea></body>
                </rml>
                """))
            {
                Assert.IsAssignableFrom<ElementFormControlTextArea>(scope.Find("probe"));
            }
        }
    }

    [Fact]
    public void Removing_a_context_drops_the_wrappers_it_owned()
    {
        DocumentScope scope = _fixture.OpenDocument();
        string name = scope.Name;

        // Remember what the old document looked like, then destroy it.
        string oldTitle = scope.Document.GetTitle();
        scope.Detach();
        Assert.True(Rml.RemoveContext(name));

        // The destroyed document's address may be handed to the next context's elements. If the
        // cache still held the old wrapper, this lookup would either throw InvalidCastException
        // or silently answer with the dead document.
        using DocumentScope next = _fixture.OpenDocument();

        Assert.Equal("Unit", next.Document.GetTitle());
        Assert.Equal(oldTitle, next.Document.GetTitle());
        Assert.Equal("para", next.Find("para").Id);

        // The fresh document must be backed by a live wrapper, not the disposed one.
        Assert.False(next.Document.IsDisposed);
        Assert.True(next.Document.NumChildren > 0);
    }
}
