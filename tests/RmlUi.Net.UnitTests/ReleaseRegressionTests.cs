using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using RmlUiNet;
using Xunit;

namespace RmlUi.Net.UnitTests;

[Collection(RmlUiCollection.Name)]
public sealed class ReleaseRegressionTests(RmlUiFixture fixture)
{
    [Fact]
    public void Destroying_one_context_invalidates_its_wrappers_but_keeps_the_other_context()
    {
        using var left = fixture.OpenDocument();
        using var right = fixture.OpenDocument();
        var stale = left.Find("button");
        var live = right.Find("button");
        int hash = stale.GetHashCode();
        left.Dispose();

        Assert.Throws<ObjectDisposedException>(() => left.Context.Update());
        Assert.Throws<ObjectDisposedException>(() => left.Document.GetTitle());
        Assert.Throws<ObjectDisposedException>(() => stale.Click());
        Assert.Throws<ObjectDisposedException>(() => stale.OwnerDocument);
        Assert.Equal(hash, stale.GetHashCode());
        Assert.Same(live, right.Find("button"));
        Assert.Equal("button", live.TagName);
        Assert.Same(fixture.RenderInterface, Rml.GetRenderInterface());
    }

    [Fact]
    public void Removing_a_subtree_never_reuses_a_stale_wrapper_for_a_new_element()
    {
        using var scope = fixture.OpenDocument();
        Element root = scope.Root;
        var old = scope.Find("para");
        root.SetInnerRml("<p id='para'>replacement</p>");

        Assert.Throws<ObjectDisposedException>(() => old.GetInnerRml());
        Assert.NotEqual(old, scope.Find("para"));
        Assert.Equal("replacement", scope.Find("para").GetInnerRml());
    }

    [Fact]
    public void Rejected_reparenting_keeps_the_tree_intact_and_detached_nodes_can_be_destroyed()
    {
        using var scope = fixture.OpenDocument();
        Element child = scope.Find("para");
        Element root = scope.Root;
        Assert.False(child.InsertBefore(root, child));
        Assert.False(root.InsertBefore(child, scope.Document.AppendChildTag("aside")));
        Assert.Same(root, child.GetParentNode());
        Assert.False(Rml.DestroyDetachedElement(child));

        root.RemoveChild(child);
        Assert.Null(child.GetParentNode());
        Assert.True(Rml.DestroyDetachedElement(child));
        Assert.Throws<ObjectDisposedException>(() => child.TagName);
    }

    [Fact]
    public void Models_and_handles_are_isolated_by_context_and_removal_invalidates_all_handles()
    {
        using var left = fixture.OpenDocument();
        using var right = fixture.OpenDocument();
        using var first = left.Context.CreateDataModel("shared");
        using var second = right.Context.CreateDataModel("shared");
        Assert.True(first.BindInt("score", 10));
        Assert.True(second.BindInt("score", 20));
        Assert.False(first.BindString("score", "wrong type"));
        Assert.Throws<InvalidOperationException>(() => left.Context.CreateDataModel("shared"));
        Assert.Same(first, left.Context.GetDataModel("shared"));
        Assert.Same(second, right.Context.GetDataModel("shared"));

        first.Handle.DirtyVariable("score");
        Assert.True(first.Handle.IsVariableDirty("score"));
        Assert.False(second.Handle.IsVariableDirty("score"));

        using var structure = Assert.IsType<StructHandle>(first.RegisterStructHandle("layout"));
        first.Dispose();
        Assert.Throws<ObjectDisposedException>(() => first.BindInt("score", 99));
        Assert.Throws<ObjectDisposedException>(() => first.Handle.DirtyVariable("score"));
        Assert.Throws<ObjectDisposedException>(() => structure.RegisterMember("value", 0));
        Assert.Null(left.Context.GetDataModel("shared"));
        using var replacement = left.Context.CreateDataModel("shared");
        Assert.True(replacement.BindString("score", "new model"));

        right.Dispose();
        Assert.True(second.IsDisposed);
        Assert.True(second.Handle.IsDisposed);
    }

    [Fact]
    public void Event_views_expire_and_callback_exceptions_cannot_escape_native_dispatch()
    {
        using var scope = fixture.OpenDocument();
        var errors = new List<Exception>();
        Event? retained = null;
        int laterHandlers = 0;
        void Report(Exception error) => errors.Add(error);
        Rml.CallbackError += Report;
        try
        {
            var target = scope.Find("button");
            target.AddEventListener("click", ev =>
            {
                retained = ev;
                Rml.RemoveContext(scope.Name);
            });
            target.AddEventListener("click", _ => laterHandlers++);
            target.Click();
        }
        finally { Rml.CallbackError -= Report; }

        Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        Assert.Equal(1, laterHandlers);
        Assert.Throws<ObjectDisposedException>(() => retained!.StopPropagation());
        Assert.False(scope.Context.IsDisposed);
    }

    [Fact]
    public void Listener_and_renderer_cannot_be_freed_while_native_code_owns_references()
    {
        using var scope = fixture.OpenDocument();
        using var listener = new CountingListener();
        var button = scope.Find("button");
        button.AddEventListener("click", listener);
        Assert.Throws<InvalidOperationException>(listener.Dispose);
        Assert.Throws<InvalidOperationException>(fixture.RenderInterface.Dispose);
        button.Click();
        Assert.Equal(1, listener.Count);
        button.RemoveEventListener("click", listener);
        listener.Dispose();
        button.Click();
        Assert.Equal(1, listener.Count);
    }

    [Fact]
    public void Access_from_another_thread_is_rejected_before_native_calls()
    {
        using var scope = fixture.OpenDocument();
        var errors = new List<Exception?>();
        var thread = new Thread(() =>
        {
            errors.Add(Record.Exception(scope.Context.Update));
            errors.Add(Record.Exception(Rml.Shutdown));
        }) { IsBackground = true };
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(5)));
        Assert.All(errors, error => Assert.IsType<InvalidOperationException>(error));
        scope.Context.Update();
    }

    [Fact]
    public void Markup_input_and_event_payloads_round_trip_utf8()
    {
        const string text = "界面 – 日本語 🧭";
        using var scope = fixture.OpenDocument();
        scope.Document.SetTitle(text);
        var para = scope.Find("para");
        para.SetId("标题");
        para.SetAttribute("title", text);
        para.SetInnerText(text);
        Assert.Equal(text, scope.Document.GetTitle());
        Assert.Same(para, scope.Document.GetElementById("标题"));
        Assert.Equal("标题", para.Id);
        Assert.Equal(text, para.GetAttribute("title"));
        Assert.Equal(text, para.GetInnerRml());

        var input = Assert.IsType<ElementFormControlInput>(scope.Find("input"));
        input.SetValue(string.Empty);
        Assert.True(input.Focus());
        scope.Context.ProcessTextInput(text);
        Assert.Equal(text, input.GetValue());

        object? actual = null;
        para.AddEventListener("click", ev => actual = ev.Parameters["值"]);
        para.DispatchEvent("click", new() { ["值"] = text }, true, true);
        Assert.Equal(text, actual);
    }

    [Fact]
    public unsafe void Malformed_read_count_is_reported_without_writing_past_the_buffer()
    {
        using var files = new InvalidReader();
        Span<byte> bytes = stackalloc byte[] { 3, 5 };
        var errors = new List<Exception>();
        void Report(Exception error) => errors.Add(error);
        Rml.CallbackError += Report;
        try
        {
            fixed (byte* p = bytes)
                Assert.Equal((nuint)0, files._Read(p, 1, new IntPtr(1)));
            Assert.True(bytes.SequenceEqual(new byte[] { 3, 5 }));
            Assert.IsType<InvalidOperationException>(Assert.Single(errors));
        }
        finally { Rml.CallbackError -= Report; }
    }

    private sealed class CountingListener : EventListener
    {
        public int Count { get; private set; }
        public override void ProcessEvent(Event ev) => Count++;
    }

    private sealed class InvalidReader : RmlUiNet.FileInterface
    {
        public override IntPtr Open(string path) => new(1);
        public override void Close(IntPtr file) { }
        public override ulong Read(ulong size, IntPtr file, out byte[] bytes) { bytes = [99, 99]; return 2; }
        public override bool Seek(IntPtr file, uint offset, int origin) => false;
        public override ulong Tell(IntPtr file) => 0;
        public override ulong Length(IntPtr file) => 0;
        public override string LoadFile(string path) => string.Empty;
    }
}
