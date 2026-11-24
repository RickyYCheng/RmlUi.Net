# RickyYC.RmlUi.Net

Unofficial .NET 8 bindings for [RmlUi 6.3](https://github.com/mikke89/RmlUi).
This package includes native libraries for Windows x64 and x86, Linux x64
with glibc, and macOS on Intel and Apple Silicon.

The source is maintained at [RickyYCheng/RmlUi.Net](https://github.com/RickyYCheng/RmlUi.Net),
a fork of [PourrezJ/RmlUi.Net](https://github.com/PourrezJ/RmlUi.Net), which
continues [Chorizite/RmlUi.Net](https://github.com/Chorizite/RmlUi.Net).
Use only one of these packages in the application. The managed assembly and
native library from different forks or releases are not interchangeable.

## Start a context

```csharp
using RmlUiNet;

using var renderer = new RenderInterface();
Rml.SetRenderInterface(renderer);
try
{
    if (!Rml.Initialise())
        throw new InvalidOperationException("Could not initialise RmlUi.");

    using var context = Rml.CreateContext("main", new Vector2i(1280, 720))
        ?? throw new InvalidOperationException("Could not create a context.");
    var document = context.LoadDocumentFromMemory(
        "<rml><head><title>Hello</title></head><body><div id='main'/></body></rml>")
        ?? throw new InvalidOperationException("Could not load the document.");
    document.Show();
    context.Update();
    context.Render();
}
finally { Rml.Shutdown(); }
```

The base renderer is headless. To draw on screen, implement `RenderInterface`
for your graphics backend, load fonts with `Rml.LoadFontFace` before loading
documents that contain text, and call `Update` and `Render` from your game loop.

## Ownership and callbacks

- Use one thread for interface registration, initialization, UI operations and
  shutdown. Dispose installed adapters and registered plugins or instancers
  after shutdown. Do not tear down the runtime or a context from inside a callback.
- `Context.Dispose` removes the context. `DataModelConstructor.Dispose` removes
  the model from its context. Close a document with `Close`; its native
  destruction is deferred until the next context update. Do not use wrappers
  after their objects are destroyed.
- Removing a child detaches it rather than destroying it. Reattach it or call
  `Rml.DestroyDetachedElement` to release it before shutdown.
- `Event` and data-event argument variants are valid for the duration of the
  callback. Copy any values you need later. Detach a manually managed listener
  before disposing it. `Rml.CallbackError` reports managed exceptions caught
  at the native callback boundary.
- Pointer-based constructors and custom data-binding storage require native
  lifetime management by the caller. For a custom element instancer, return a
  new binding-owned `ElementCustom` or `ElementDocument`; the release callback
  notifies you but does not transfer ownership.

Load markup, styles and fonts from trusted sources. The default file interface
accesses the host filesystem and is not a sandbox.

See [third-party notices](https://github.com/RickyYCheng/RmlUi.Net/blob/main/THIRD-PARTY-NOTICES.md)
for licenses, including the fonts built into the native debugger.
