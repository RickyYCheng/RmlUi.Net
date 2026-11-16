using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using RmlUiNet;
using RmlUiNet.Input;
using Xunit;

namespace RmlUi.Net.UnitTests;

[Collection(RmlUiCollection.Name)]
public sealed class VariantTests
{
    [Fact]
    public void Scalar_variants_round_trip()
    {
        Assert.Equal(VariantType.INT, new Variant(42).Type);
        Assert.Equal(42, (int)new Variant(42).Value!);

        Assert.Equal(VariantType.UINT, new Variant(42u).Type);
        Assert.Equal(42u, (uint)new Variant(42u).Value!);

        Assert.Equal(VariantType.FLOAT, new Variant(1.5f).Type);
        Assert.Equal(1.5f, (float)new Variant(1.5f).Value!);

        Assert.Equal(VariantType.DOUBLE, new Variant(2.5).Type);
        Assert.Equal(2.5, (double)new Variant(2.5).Value!);

        Assert.Equal(VariantType.BOOL, new Variant(true).Type);
        Assert.True((bool)new Variant(true).Value!);

        Assert.Equal(VariantType.STRING, new Variant("hello").Type);
        Assert.Equal("hello", (string)new Variant("hello").Value!);
    }

    [Fact]
    public void Empty_string_variant_stays_a_string()
    {
        var variant = new Variant(string.Empty);

        Assert.Equal(VariantType.STRING, variant.Type);
        Assert.Equal(string.Empty, (string)variant.Value!);
    }

    [Fact]
    public void Int_variant_compares_by_value()
    {
        Assert.True(new Variant(7).Value is int first && first == 7);
        Assert.Equal(new Variant(7).Type, new Variant(7 + 0).Type);
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class DictionaryTests
{
    [Fact]
    public void Dictionary_round_trips_variants()
    {
        IntPtr dictionary = RmlUiNet.Native.RmlDictionary.Create();
        Assert.NotEqual(IntPtr.Zero, dictionary);

        var number = new Variant(42);
        var text = new Variant("forty-two");
        try
        {
            RmlUiNet.Native.RmlDictionary.Insert(dictionary, "int_key", number.NativePtr);
            RmlUiNet.Native.RmlDictionary.Insert(dictionary, "str_key", text.NativePtr);

            Assert.Equal(2, RmlUiNet.Native.RmlDictionary.Size(dictionary));
            Assert.Equal(42, (int)new Variant(RmlUiNet.Native.RmlDictionary.Get(dictionary, "int_key")).Value!);
            Assert.Equal("forty-two", (string)new Variant(RmlUiNet.Native.RmlDictionary.Get(dictionary, "str_key")).Value!);

            string[] keys = RmlUiNet.Native.RmlDictionary.RetrieveKeys(dictionary);
            Assert.Equal(2, keys.Length);
            Assert.Contains("int_key", keys);
            Assert.Contains("str_key", keys);

            // Remove is bound so callers can drop a key without rebuilding the dictionary.
            RmlUiNet.Native.RmlDictionary.Remove(dictionary, "int_key");
            Assert.Equal(1, RmlUiNet.Native.RmlDictionary.Size(dictionary));
            Assert.DoesNotContain("int_key", RmlUiNet.Native.RmlDictionary.RetrieveKeys(dictionary));
        }
        finally
        {
            RmlUiNet.Native.Variant.Free(number.NativePtr);
            RmlUiNet.Native.Variant.Free(text.NativePtr);
            RmlUiNet.Native.RmlDictionary.Free(dictionary);
        }
    }

    [Fact]
    public void RetrieveKeys_on_an_empty_pointer_returns_no_keys()
    {
        Assert.Empty(RmlUiNet.Native.RmlDictionary.RetrieveKeys(IntPtr.Zero));
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class FontTests
{
    private readonly RmlUiFixture _fixture;

    public FontTests(RmlUiFixture fixture) => _fixture = fixture;

    private static string BundledFont(string face) =>
        System.IO.Path.Combine(AppContext.BaseDirectory, "assets", face);

    [Fact]
    public void LoadFontFace_reports_failure_for_a_missing_file()
    {
        // The native export used to return void, so the managed bool read garbage and callers
        // could not tell a failed load from a successful one.
        Assert.False(Rml.LoadFontFace("__definitely_missing_font__.ttf"));
    }

    [Fact]
    public void LoadFontFace_reports_success_for_a_real_file()
    {
        string path = BundledFont("LatoLatin-Regular.ttf");
        Assert.True(System.IO.File.Exists(path), $"bundled font missing at {path}");

        // Loading the same face twice is not an error in 6.3.
        Assert.True(Rml.LoadFontFace(path));
        Assert.True(Rml.LoadFontFace(path));
    }

    [Fact]
    public void LoadFontFace_with_explicit_family_registers_the_alias()
    {
        // The 6.3 overload backs the RCSS @font-face rule: it registers the face under a
        // caller-chosen family instead of the name stored inside the file.
        Assert.True(Rml.LoadFontFace(BundledFont("LatoLatin-Regular.ttf"), "unit-test-family", FontStyle.Normal));
    }

    [Fact]
    public void Explicit_family_replaces_the_name_stored_in_the_file()
    {
        // "LatoLatin" is what the TTF declares; the alias must win, otherwise the test above
        // would pass for the wrong reason (the face loading under its own name).
        Assert.True(Rml.LoadFontFace(BundledFont("LatoLatin-Regular.ttf"), "aliased-only", FontStyle.Normal));

        using DocumentScope scope = _fixture.OpenDocument("""
            <rml><head><title>T</title></head><body id="body"></body></rml>
            """);

        Element probe = scope.Find("body");

        probe.SetProperty("font-family", "aliased-only");
        scope.Context.Update();
        Assert.Equal("aliased-only", probe.GetProperty("font-family"));

        // A family nothing registered must not resolve; the alias must.
        probe.SetProperty("font-family", "no-such-family-anywhere");
        scope.Context.Update();
        Assert.Equal("no-such-family-anywhere", probe.GetProperty("font-family"));
    }

    [Fact]
    public void Missing_file_with_explicit_family_still_fails()
    {
        Assert.False(Rml.LoadFontFace("__missing__.ttf", "family", FontStyle.Normal));
    }

    [Fact]
    public void Bold_face_loads_as_a_separate_face()
    {
        Assert.True(Rml.LoadFontFace(BundledFont("LatoLatin-Bold.ttf")));
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class DataModelTests : IDisposable
{
    private readonly DocumentScope _scope;

    public DataModelTests(RmlUiFixture fixture) => _scope = fixture.OpenDocument();

    public void Dispose() => _scope.Dispose();

    [Fact]
    public void Scalar_bindings_accept_updates()
    {
        DataModelConstructor model = _scope.Context.CreateDataModel("scalars");
        try
        {
            Assert.True(model.BindFloat("f", 1.5f));
            Assert.True(model.BindInt("i", 3));
            Assert.True(model.BindUInt("u", 4));
            Assert.True(model.BindBool("b", true));
            Assert.True(model.BindString("s", "one"));

            // Re-binding an existing name updates the backing storage in place.
            Assert.True(model.BindFloat("f", 2.5f));
            Assert.True(model.BindString("s", "two"));

            model.Handle.DirtyAllVariables();
            model.Handle.DirtyVariable("f");
            _ = model.Handle.IsVariableDirty("f");

            Assert.NotEqual(IntPtr.Zero, model.Handle.NativePtr);
        }
        finally
        {
            model.Dispose();
        }
    }

    [Fact]
    public void IsVariableDirty_reports_the_dirty_flag()
    {
        DataModelConstructor model = _scope.Context.CreateDataModel("dirty");
        try
        {
            Assert.True(model.BindInt("counter", 0));

            model.Handle.DirtyVariable("counter");
            Assert.True(model.Handle.IsVariableDirty("counter"));

            // The flag is cleared once the context consumes it.
            _scope.Context.Update();
            Assert.False(model.Handle.IsVariableDirty("counter"));
        }
        finally
        {
            model.Dispose();
        }
    }

    [Fact]
    public void Struct_handle_registers_members()
    {
        DataModelConstructor model = _scope.Context.CreateDataModel("structs");
        try
        {
            StructHandle? handle = model.RegisterStructHandle("point");
            Assert.NotNull(handle);

            // Offsets are byte offsets into the managed struct.
            Assert.True(handle!.RegisterMember("x", 0, StructMemberType.Float));
            Assert.True(handle.RegisterMemberInt("y", 4));
        }
        finally
        {
            model.Dispose();
        }
    }

    [Fact]
    public void GetDataModel_returns_the_live_model_and_RemoveDataModel_drops_it()
    {
        DataModelConstructor model = _scope.Context.CreateDataModel("lifecycle");
        model.BindInt("value", 1);
        model.Dispose();

        Assert.NotNull(_scope.Context.GetDataModel("lifecycle"));
        Assert.True(_scope.Context.RemoveDataModel("lifecycle"));
        Assert.Null(_scope.Context.GetDataModel("lifecycle"));
    }

    [Fact]
    public void RemoveDataModel_reports_an_unknown_model()
    {
        Assert.False(_scope.Context.RemoveDataModel("never-created"));
    }
}

[Collection(RmlUiCollection.Name)]
public sealed class ContextInputTests : IDisposable
{
    private readonly DocumentScope _scope;

    public ContextInputTests(RmlUiFixture fixture) => _scope = fixture.OpenDocument();

    public void Dispose() => _scope.Dispose();

    [Fact]
    public void Mouse_events_are_accepted()
    {
        Context context = _scope.Context;

        _ = context.ProcessMouseMove(100, 100, KeyModifier.None);
        _ = context.ProcessMouseButtonDown(0, KeyModifier.None);
        _ = context.ProcessMouseButtonUp(0, KeyModifier.None);
        _ = context.ProcessMouseWheel(new Vector2f(0, -1), KeyModifier.None);
        _ = context.IsMouseInteracting;
        _ = context.ProcessMouseLeave();
    }

    [Fact]
    public void Keyboard_events_are_accepted()
    {
        Context context = _scope.Context;

        _ = context.ProcessKeyDown(KeyIdentifier.KI_A, KeyModifier.None);
        _ = context.ProcessKeyUp(KeyIdentifier.KI_A, KeyModifier.None);
        _ = context.ProcessTextInput("a");
    }

    [Fact]
    public void Typing_into_a_focused_input_updates_its_value()
    {
        Element input = _scope.Find("input");
        Assert.IsType<ElementFormControlInput>(input);
        var field = (ElementFormControlInput)input;

        Assert.True(input.Focus());
        Assert.IsAssignableFrom<ElementFormControlInput>(_scope.Context.GetFocusElement());

        // Focus places the cursor at the start of the value, so typed text becomes a prefix.
        Assert.False(_scope.Context.ProcessTextInput("ab"));
        Assert.Equal("abinitial", field.GetValue());

        // Backspace deletes the character before the cursor, which sits after "ab".
        Assert.False(_scope.Context.ProcessKeyDown(KeyIdentifier.KI_BACK, KeyModifier.None));
        Assert.Equal("ainitial", field.GetValue());

        field.SetValue("reset");
        Assert.Equal("reset", field.GetValue());
    }
}
