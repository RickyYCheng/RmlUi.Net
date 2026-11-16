using System;
using System.Collections.Generic;
using RmlUiNet;
using Xunit;

namespace RmlUi.Net.UnitTests;

/// <summary>
/// Groups every test class that touches the native library into one xunit collection: RmlUi
/// is a process-wide singleton and cannot be initialised twice, so these tests must not run
/// in parallel with each other.
/// </summary>
[CollectionDefinition(Name)]
public sealed class RmlUiCollection : ICollectionFixture<RmlUiFixture>
{
    public const string Name = "rmlui";
}

[Collection(RmlUiCollection.Name)]
public sealed class LifecycleTests
{
    private readonly RmlUiFixture _fixture;

    public LifecycleTests(RmlUiFixture fixture) => _fixture = fixture;

    [Fact]
    public void Context_has_the_name_it_was_created_with()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Assert.StartsWith("unit-", scope.Name);
        Assert.Equal(scope.Name, scope.Context.Name);
    }

    [Fact]
    public void SetDimensions_updates_the_context_without_throwing()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        scope.Context.SetDimensions(320, 240);
        scope.Context.Update();
        scope.Context.SetDimensions(1024, 768);
        scope.Context.Update();
    }

    [Fact]
    public void Document_reports_title_and_source()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Assert.Equal("Unit", scope.Document.GetTitle());
        Assert.EndsWith(".rml", scope.Document.GetSourceURL());
    }

    [Fact]
    public void Document_title_can_be_replaced()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        scope.Document.SetTitle("Renamed");

        Assert.Equal("Renamed", scope.Document.GetTitle());
    }

    [Fact]
    public void Root_element_is_available()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        Assert.NotNull(scope.Context.GetRootElement());
    }

    [Fact]
    public void Document_show_and_hide_are_repeatable()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        scope.Document.Show();
        scope.Document.Show(ModalFlag.Keep);
        scope.Document.PullToFront();
        scope.Document.Hide();
        scope.Document.Show();
    }

    [Fact]
    public void RemoveContext_reports_a_missing_context()
    {
        // Exercises the bool return the native export used to discard: the managed side read
        // whatever happened to be in the return register.
        Assert.False(Rml.RemoveContext("no-such-context"));
    }

    [Fact]
    public void RemoveContext_returns_true_for_a_live_context()
    {
        using DocumentScope scope = _fixture.OpenDocument();

        // The scope must not try to remove it again on dispose, so it is detached first.
        Assert.True(Rml.RemoveContext(scope.Name));
        scope.Detach();
    }
}
