using System;
using System.Collections.Generic;
using System.IO;
using RmlUiNet;
using Xunit;
using Rml = RmlUiNet.Rml;
using Context = RmlUiNet.Context;
using ElementDocument = RmlUiNet.ElementDocument;

namespace RmlUi.Net.UnitTests;

/// <summary>
/// Owns the process-wide RmlUi instance for the test collection. RmlUi can only be
/// initialised once per process and holds global interface pointers, so every test class
/// shares this fixture instead of calling <see cref="Rml.Initialise"/> itself.
/// </summary>
/// <remarks>
/// The fixture deliberately creates no context: RmlUi keeps every loaded document alive
/// until its context is destroyed, so a shared context would leak elements between tests.
/// Use <see cref="OpenDocument"/> to get a context that is torn down with the test.
/// </remarks>
public sealed class RmlUiFixture : IDisposable
{
    /// <summary>
    /// The font family the tests use. It is loaded from a TTF that ships with the test
    /// project, so layout results do not depend on what the host machine has installed.
    /// </summary>
    public const string FontFamily = "LatoLatin";

    /// <summary>Regular and bold faces, both bundled in <c>assets/</c>.</summary>
    public static readonly string[] BundledFonts = ["LatoLatin-Regular.ttf", "LatoLatin-Bold.ttf"];

    private int _nextContext;

    public RmlUiFixture()
    {
        RenderInterface = new RecordingRenderInterface();
        SystemInterface = new SilentSystemInterface();
        FileInterface = new InMemoryFileInterface();

        if (!Rml.Initialise())
            throw new InvalidOperationException("Rml.Initialise() failed.");

        Rml.SetRenderInterface(RenderInterface);
        Rml.SetSystemInterface(SystemInterface);
        Rml.SetFileInterface(FileInterface);

        LoadedFont = LoadBundledFonts();
        if (!LoadedFont)
        {
            throw new InvalidOperationException(
                $"Could not load the bundled test fonts from '{Path.Combine(AppContext.BaseDirectory, "assets")}'. " +
                "Layout assertions depend on them; check that the assets/*.ttf items were copied to the output.");
        }
    }

    public RecordingRenderInterface RenderInterface { get; }
    public SilentSystemInterface SystemInterface { get; }
    public InMemoryFileInterface FileInterface { get; }

    /// <summary>True when the bundled font faces were loaded. Always true after construction.</summary>
    public bool LoadedFont { get; }

    /// <summary>
    /// Markup every test can rely on. <c>#block</c> and <c>#inline</c> exist so geometry
    /// tests never need to hard-code a size that depends on the font's metrics.
    /// </summary>
    public const string DefaultMarkup = """
        <rml>
          <head>
            <title>Unit</title>
            <style>
              body { font-family: 'LatoLatin'; width: 100%; height: 100%; margin: 0; padding: 0; }
              #root { display: block; }
              .text { display: block; }
              #fixed { display: block; width: 120px; height: 40px; }
              #block { display: block; width: 200px; height: 80px; }
            </style>
          </head>
          <body id="body">
            <div id="root" class="container">
              <p id="para" class="text">Hello</p>
              <span id="span" class="text muted">World</span>
              <input id="input" type="text" value="initial"/>
              <button id="button">Press</button>
              <div id="fixed">fixed</div>
              <div id="block">block</div>
            </div>
          </body>
        </rml>
        """;

    /// <summary>
    /// Creates a context with its own document. Dispose the scope to destroy the context,
    /// which also releases every element the test created.
    /// </summary>
    public DocumentScope OpenDocument(string? markup = null, Vector2i? dimensions = null)
    {
        string name = $"unit-{_nextContext++}";
        Vector2i size = dimensions ?? new Vector2i(1024, 768);

        Context context = Rml.CreateContext(name, size, RenderInterface)
            ?? throw new InvalidOperationException("Rml.CreateContext returned null.");
        context.Update();

        ElementDocument document = context.LoadDocumentFromMemory(markup ?? DefaultMarkup, $"{name}.rml")
            ?? throw new InvalidOperationException("LoadDocumentFromMemory returned null.");
        document.Show();
        context.Update();

        return new DocumentScope(name, context, document);
    }

    /// <summary>
    /// Loads the font faces that ship with the test project. Returns false when they are
    /// missing from the output directory, which only affects layout-dependent assertions.
    /// </summary>
    private static bool LoadBundledFonts()
    {
        bool allLoaded = true;

        foreach (string face in BundledFonts)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "assets", face);
            if (!File.Exists(path))
            {
                allLoaded = false;
                continue;
            }

            allLoaded &= Rml.LoadFontFace(path);
        }

        return allLoaded;
    }

    public void Dispose()
    {
        try
        {
            Rml.Shutdown();
        }
        catch (Exception)
        {
            // Shutdown must not mask a test failure.
        }

        RenderInterface.Dispose();
    }
}

/// <summary>A context plus its document, destroyed together.</summary>
public sealed class DocumentScope : IDisposable
{
    private bool _disposed;

    internal DocumentScope(string name, Context context, ElementDocument document)
    {
        Name = name;
        Context = context;
        Document = document;
    }

    public string Name { get; }
    public Context Context { get; }
    public ElementDocument Document { get; }

    /// <summary>The document's <c>#root</c> element.</summary>
    public Element Root => Document.GetElementById("root")
        ?? throw new InvalidOperationException("root element missing from the document");

    /// <summary>Finds an element by id within this scope's document.</summary>
    public Element Find(string id) => Document.GetElementById(id)
        ?? throw new InvalidOperationException($"element #{id} missing from the document");

    /// <summary>Loads another document into this scope's context.</summary>
    public ElementDocument Load(string markup, string path)
    {
        var document = Context.LoadDocumentFromMemory(markup, path)
            ?? throw new InvalidOperationException($"Could not load '{path}'.");
        document.Show();
        Context.Update();
        return document;
    }

    /// <summary>Marks the scope as already torn down so <see cref="Dispose"/> becomes a no-op.</summary>
    public void Detach() => _disposed = true;

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;

        // Removing the context destroys its documents and every element they own. The
        // wrapper objects must not be touched afterwards.
        Rml.RemoveContext(Name);
    }
}

/// <summary>Render interface that only records the calls RmlUi makes.</summary>
public sealed unsafe class RecordingRenderInterface : RenderInterface
{
    public int GeometryCompiled { get; private set; }
    public int GeometryRendered { get; private set; }
    public int GeometriesReleased { get; private set; }
    public int TexturesGenerated { get; private set; }
    public int TexturesReleased { get; private set; }
    public int ScissorRegionsSet { get; private set; }
    public bool ScissorEnabled { get; private set; }

    private long _nextHandle = 1;

    public override IntPtr CompileGeometry(Vertex* vertices, int vertexCount, int* indices, int indexCount)
    {
        GeometryCompiled++;
        return new IntPtr(_nextHandle++);
    }

    public override void RenderGeometry(IntPtr geometry, Vector2f translation, IntPtr texture) =>
        GeometryRendered++;

    public override void ReleaseGeometry(IntPtr geometry) => GeometriesReleased++;

    public override IntPtr GenerateTexture(byte* source, int numBytes, Vector2i dimensions)
    {
        TexturesGenerated++;
        return new IntPtr(_nextHandle++);
    }

    public override void ReleaseTexture(IntPtr texture) => TexturesReleased++;

    public override void EnableScissorRegion(bool enable) => ScissorEnabled = enable;

    public override void SetScissorRegion(int x, int y, int width, int height) => ScissorRegionsSet++;
}

/// <summary>System interface with deterministic time and no clipboard or IME.</summary>
public sealed class SilentSystemInterface : SystemInterface
{
    public override double ElapsedTime => 0.016;

    public override void SetClipboardText(string text) { }

    public override string GetClipboardText() => string.Empty;

    public override void ActivateKeyboard(float caretX, float caretY, float lineHeight) { }

    public override void DeactivateKeyboard() { }
}

/// <summary>
/// File interface that serves files from memory, falling back to the real file system.
/// </summary>
/// <remarks>
/// RmlUi loads font faces through the file interface, so an in-memory-only implementation
/// would make every system font unloadable. The fallback keeps test fixtures free of
/// temp files while still allowing real fonts to be loaded.
/// </remarks>
public sealed class InMemoryFileInterface : FileInterface
{
    private readonly Dictionary<string, byte[]> _files = new();
    private readonly Dictionary<long, (byte[] Data, long Position)> _open = new();
    private long _nextHandle = 1;

    public void AddFile(string path, string content) =>
        _files[path] = System.Text.Encoding.UTF8.GetBytes(content);

    public void AddFile(string path, byte[] content) => _files[path] = content;

    private bool TryGetContent(string path, out byte[] data)
    {
        if (_files.TryGetValue(path, out byte[]? inMemory))
        {
            data = inMemory;
            return true;
        }

        // Fall back to the real file system so system fonts and assets can be opened.
        try
        {
            if (File.Exists(path))
            {
                data = File.ReadAllBytes(path);
                return true;
            }
        }
        catch (Exception)
        {
            // Unreadable path: treated as missing.
        }

        data = Array.Empty<byte>();
        return false;
    }

    public override IntPtr Open(string path)
    {
        if (!TryGetContent(path, out byte[] data))
            return IntPtr.Zero;

        long handle = _nextHandle++;
        _open[handle] = (data, 0);
        return new IntPtr(handle);
    }

    public override void Close(IntPtr file) => _open.Remove(file.ToInt64());

    public override ulong Read(ulong size, IntPtr file, out byte[] bytes)
    {
        if (!_open.TryGetValue(file.ToInt64(), out var entry))
        {
            bytes = Array.Empty<byte>();
            return 0;
        }

        int count = (int)Math.Min((long)size, entry.Data.LongLength - entry.Position);
        if (count <= 0)
        {
            bytes = Array.Empty<byte>();
            return 0;
        }

        bytes = new byte[count];
        Array.Copy(entry.Data, entry.Position, bytes, 0, count);
        _open[file.ToInt64()] = (entry.Data, entry.Position + count);
        return (ulong)count;
    }

    public override bool Seek(IntPtr file, uint offset, int origin)
    {
        if (!_open.TryGetValue(file.ToInt64(), out var entry))
            return false;

        long target = origin switch
        {
            0 => offset,
            1 => entry.Position + offset,
            2 => entry.Data.LongLength + offset,
            _ => -1,
        };

        if (target < 0)
            return false;

        _open[file.ToInt64()] = (entry.Data, target);
        return true;
    }

    public override ulong Tell(IntPtr file) =>
        _open.TryGetValue(file.ToInt64(), out var entry) ? (ulong)entry.Position : 0;

    public override ulong Length(IntPtr file) =>
        _open.TryGetValue(file.ToInt64(), out var entry) ? (ulong)entry.Data.LongLength : 0;

    public override string LoadFile(string path) =>
        TryGetContent(path, out byte[] data) ? System.Text.Encoding.UTF8.GetString(data) : string.Empty;
}
