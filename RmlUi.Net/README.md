# RmlUi.Net

C# bindings for [RmlUi](https://github.com/mikke89/RmlUi) — a native UI library
based on HTML/CSS. Embedded RmlUi: **6.2**.

## Supported platforms

| RID | Native file |
| --- | --- |
| `win-x64`, `win-x86` | `runtimes/<rid>/native/RmlUiNative.dll` |
| `linux-x64` | `runtimes/linux-x64/native/libRmlUiNative.so` |
| `osx-x64` | `runtimes/osx-x64/native/RmlUiNative.dylib` (universal arm64+x86_64) |

## Installation

This package is an independently maintained fork of the upstream
`Chorizite/RmlUi.Net` project and is published on nuget.org under its own id:

```powershell
dotnet add package RickyYC.RmlUi.Net
```

## Quick start

```csharp
using RmlUiNet;

Rml.Initialise();

var context = Rml.CreateContext("main", new Vector2i(1280, 720));
var document = context.LoadDocumentFromMemory(
    "<rml><head><title>Hello</title></head><body><p>Hello, RmlUi.Net!</p></body></rml>");

while (running)
{
    context.Update();
    context.Render();
}

Rml.Shutdown();
```

## Version 1.1.0

Restores the 51 native exports that were missing from 1.0.1 (no more
`EntryPointNotFoundException`), fixes `Element.SetAttribute`/`SetProperty` for
empty string values, and ships native builds for all four platforms. See the
[CHANGELOG](https://github.com/RickyYCheng/RmlUi.Net/blob/main/CHANGELOG.md)
for details.

## License

MIT. RmlUi (MIT) and FreeType (FTL/GPLv2) are embedded under their respective
licenses.
