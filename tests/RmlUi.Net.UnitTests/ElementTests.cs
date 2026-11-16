using System;
using System.Collections.Generic;
using RmlUiNet;
using Xunit;

namespace RmlUi.Net.UnitTests;

/// <summary>
/// Base for tests that need an isolated document. xunit creates one instance per test, and a
/// scope gets its own context, so documents and elements never leak between tests.
/// </summary>
public abstract class DocumentTestBase : IDisposable
{
    protected DocumentTestBase(RmlUiFixture fixture)
    {
        Fixture = fixture;
        Scope = fixture.OpenDocument();
    }

    protected RmlUiFixture Fixture { get; }
    protected DocumentScope Scope { get; }
    protected ElementDocument Document => Scope.Document;
    protected Element Root => Scope.Root;

    public void Dispose() => Scope.Dispose();
}

[Collection(RmlUiCollection.Name)]
public sealed class ElementTraversalTests : DocumentTestBase
{
    public ElementTraversalTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void Id_reflects_the_attribute()
    {
        Assert.Equal("root", Root.Id);
        Assert.Equal("para", Scope.Find("para").Id);
        Assert.Equal("root", Scope.Find("para").Closest("#root")!.Id);
    }

    [Fact]
    public void SetId_renames_the_element()
    {
        Element para = Scope.Find("para");

        para.SetId("renamed");
        Assert.Equal("renamed", para.Id);

        // The id index is rebuilt during the next context update.
        Scope.Context.Update();
        Assert.Null(Document.GetElementById("para"));
        Assert.NotNull(Document.GetElementById("renamed"));
    }

    [Fact]
    public void Children_are_reachable_by_index_first_and_last()
    {
        Assert.True(Root.NumChildren >= 3);
        Assert.True(Root.HasChildNodes);

        Element first = Root.GetFirstChild()!;
        Element last = Root.GetLastChild()!;
        Assert.Same(first, Root.GetChild(0));
        Assert.Same(last, Root.GetChild(Root.NumChildren - 1));
    }

    [Fact]
    public void Siblings_chain_in_both_directions()
    {
        Element first = Root.GetFirstChild()!;
        Element second = Root.GetChild(1)!;

        Assert.Same(second, first.GetNextSibling());
        Assert.Same(first, second.GetPreviousSibling());
        Assert.Null(first.GetPreviousSibling());
        Assert.Null(Root.GetLastChild()!.GetNextSibling());
    }

    [Fact]
    public void SetInnerText_replaces_children_with_one_text_node()
    {
        Element span = Scope.Find("span");

        span.SetInnerRml("<b>bold</b>");
        Assert.Single(span.GetElementsByTagName("b"));

        span.SetInnerText("plain");
        Scope.Context.Update();
        Assert.Equal("plain", span.GetInnerRml());
        Assert.Empty(span.GetElementsByTagName("b"));
    }

    [Fact]
    public void InsertBefore_moves_an_element()
    {
        Element para = Scope.Find("para");
        Element span = Scope.Find("span");

        Assert.True(Root.InsertBefore(span, para));
        Assert.Same(span, Root.GetFirstChild());
        Assert.Same(para, span.GetNextSibling());
    }

    [Fact]
    public void ScrollIntoView_does_not_throw()
    {
        Root.ScrollIntoView();
    }

    [Fact]
    public void Parent_of_a_root_child_is_the_body()
    {
        Element? parent = Root.GetParentNode();

        Assert.NotNull(parent);
        Assert.Equal("body", parent!.TagName);
    }

    [Fact]
    public void OffsetParent_of_a_root_child_is_the_body()
    {
        Assert.Equal("body", Root.GetOffsetParent()!.TagName);
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class ElementQueryTests : DocumentTestBase
{
    public ElementQueryTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void QuerySelectorAll_returns_matching_descendants()
    {
        IReadOnlyList<Element> texts = Root.QuerySelectorAll("p.text");

        Assert.Single(texts);
        Assert.Equal("para", texts[0].Id);
    }

    [Fact]
    public void Class_queries_also_match_input_type_classes()
    {
        // RmlUi mirrors an input's type into a CSS class, so <input type="text"> answers
        // IsClassSet("text") == true even without an explicit class attribute.
        IReadOnlyList<Element> textClass = Root.QuerySelectorAll(".text");

        Assert.Contains(textClass, element => element.Id == "para");
        Assert.Contains(textClass, element => element.Id == "span");
        Assert.Contains(textClass, element => element.Id == "input");
    }

    [Fact]
    public void QuerySelectorAll_returns_empty_for_no_match()
    {
        Assert.Empty(Root.QuerySelectorAll(".does-not-exist"));
    }

    [Fact]
    public void GetElementsByTagName_finds_elements()
    {
        IReadOnlyList<Element> paragraphs = Root.GetElementsByTagName("p");

        Assert.Single(paragraphs);
        Assert.Equal("para", paragraphs[0].Id);
    }

    [Fact]
    public void GetElementsByClassName_finds_elements()
    {
        IReadOnlyList<Element> muted = Root.GetElementsByClassName("muted");

        Assert.Single(muted);
        Assert.Equal("span", muted[0].Id);
    }

    [Fact]
    public void Matches_and_Closest_use_selectors()
    {
        Element para = Scope.Find("para");

        Assert.True(para.Matches(".text"));
        Assert.False(para.Matches(".muted"));
        Assert.Equal("root", para.Closest(".container")!.Id);
        Assert.Null(para.Closest(".unrelated"));
    }

    [Fact]
    public void IsClassSet_matches_HasClass()
    {
        Element para = Scope.Find("para");

        Assert.True(para.IsClassSet("text"));
        Assert.Equal(para.HasClass("text"), para.IsClassSet("text"));
        Assert.False(para.IsClassSet("muted"));
    }

    [Fact]
    public void Query_arguments_are_validated()
    {
        Assert.Throws<ArgumentNullException>(() => Root.QuerySelectorAll(""));
        Assert.Throws<ArgumentNullException>(() => Root.GetElementsByTagName(""));
        Assert.Throws<ArgumentNullException>(() => Root.GetElementsByClassName(""));
        Assert.Throws<ArgumentNullException>(() => Root.Matches(""));
        Assert.Throws<ArgumentNullException>(() => Root.Closest(""));
        Assert.Throws<ArgumentNullException>(() => Root.IsClassSet(""));
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class ElementGeometryTests : DocumentTestBase
{
    public ElementGeometryTests(RmlUiFixture fixture) : base(fixture) { }

    [Fact]
    public void Geometry_accessors_return_finite_values()
    {
        foreach (float value in new[]
        {
            Root.GetOffsetLeft(),
            Root.GetOffsetTop(),
            Root.GetOffsetWidth(),
            Root.GetOffsetHeight(),
            Root.GetClientLeft(),
            Root.GetClientTop(),
            Root.GetClientWidth(),
            Root.GetClientHeight(),
            Root.GetScrollTop(),
            Root.GetScrollWidth(),
            Root.GetScrollHeight(),
            Root.GetAbsoluteLeft(),
            Root.GetAbsoluteTop(),
        })
        {
            Assert.False(float.IsNaN(value), "geometry value is NaN");
            Assert.False(float.IsInfinity(value), "geometry value is infinite");
        }
    }

    [Fact]
    public void Explicit_width_and_height_are_reported_verbatim()
    {
        // The expected value is read back from the parsed style rather than hard-coded, so the
        // test still holds if the fixture markup changes.
        Element block = Scope.Find("block");

        AssertPx(ParsePx(block.GetProperty("width")), block.GetOffsetWidth());
        AssertPx(ParsePx(block.GetProperty("height")), block.GetOffsetHeight());
    }

    [Fact]
    public void Padding_and_border_widen_the_offset_box_but_not_the_client_box()
    {
        Element block = Scope.Find("block");
        float contentWidth = block.GetClientWidth();
        float contentHeight = block.GetClientHeight();

        block.SetProperty("padding", "10px");
        block.SetProperty("border", "3px");
        Scope.Context.Update();

        // offset* covers content + padding + border; client* covers content + padding.
        Assert.Equal(contentWidth + 26f, block.GetOffsetWidth(), 1);
        Assert.Equal(contentHeight + 26f, block.GetOffsetHeight(), 1);
        Assert.Equal(contentWidth + 20f, block.GetClientWidth(), 1);
        Assert.Equal(contentHeight + 20f, block.GetClientHeight(), 1);
    }

    [Fact]
    public void Style_changes_need_an_update_before_layout_reflects_them()
    {
        Element block = Scope.Find("block");
        float before = block.GetOffsetWidth();

        block.SetProperty("width", $"{before + 40f:0}px");

        // The computed value updates immediately; the cached box does not.
        Assert.Equal($"{before + 40f:0}px", block.GetProperty("width"));
        Assert.Equal(before, block.GetOffsetWidth(), 1);

        Scope.Context.Update();
        Assert.Equal(before + 40f, block.GetOffsetWidth(), 1);
    }

    [Fact]
    public void Scroll_extent_is_non_negative()
    {
        Assert.True(Root.GetScrollHeight() >= 0f);
        Assert.True(Root.GetScrollWidth() >= 0f);
    }

    [Fact]
    public void Absolute_position_is_relative_to_the_context_origin()
    {
        // The fixture's body and #root have no margin or padding, so the block's absolute
        // position equals its offset from the context origin.
        Element block = Scope.Find("block");
        Assert.Equal(block.GetOffsetLeft(), block.GetAbsoluteLeft(), 1);
    }

    /// <summary>Parses an RCSS pixel value such as <c>"120px"</c>.</summary>
    private static float ParsePx(string property)
    {
        Assert.EndsWith("px", property);
        return float.Parse(property[..^2], System.Globalization.CultureInfo.InvariantCulture);
    }

    private static void AssertPx(float expected, float actual) =>
        Assert.Equal(expected, actual, 1);
}
