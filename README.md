# RmlUi.Net

C# bindings for [RmlUi](https://github.com/mikke89/RmlUi), a native UI library
based on HTML/CSS. Embedded RmlUi: **6.2**.

## Repository layout

- `RmlUi.Native/` — native `RmlUiNative` library (C++17, CMake), RmlUi and
  FreeType sources as git submodules.
- `RmlUi.Net/` — managed library (`net8.0`) and per-RID native artifacts
  (`runtimes/<rid>/native/`).
- `tools/RmlUiExportParity/` — checks native exports vs. managed P/Invoke
  imports (built-in PE/ELF/Mach-O parsers, zero dependencies).
- `tests/RmlUi.Net.SmokeTests/` — smoke suite covering every public API.

## Prerequisites

- .NET 8 SDK
- CMake ≥ 3.21 + Ninja
- Windows: Visual Studio 2022 with the C++ workload; Linux: GCC/G++; macOS: Xcode CLT

## Building the native library

```powershell
# Windows x64 (x64 developer prompt)
cmake --preset x64-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/x64-release --parallel

# Windows x86 (x86 developer prompt, vcvars32)
cmake -S RmlUi.Native -B RmlUi.Native/out/build/x86-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build RmlUi.Native/out/build/x86-release --parallel

# Linux
cmake --preset linux-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/linux-release --parallel

# macOS (universal binary: arm64 + x86_64)
cmake --preset macos-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/macos-release --parallel
```

Outputs are written to `out/build/<preset>/bin/` (`RmlUiNative.dll`,
`libRmlUiNative.so` or `RmlUiNative.dylib`).

## Building the managed library and testing

```powershell
dotnet build RmlUi.Net/RmlUi.Net.csproj -c Release -warnaserror

# Export/import parity check (fails if any DllImport has no native export)
dotnet build tools/RmlUiExportParity/RmlUiExportParity.csproj -c Release
dotnet tools/RmlUiExportParity/bin/Release/net8.0/RmlUiExportParity.dll `
  --assembly RmlUi.Net/bin/Release/net8.0/RmlUi.Net.dll `
  --native RmlUi.Native/out/build/x64-release/bin/RmlUiNative.dll

# Smoke tests (stage the current platform's native library in
# RmlUi.Net/runtimes/<rid>/native/ first, see below)
dotnet build tests/RmlUi.Net.SmokeTests/RmlUi.Net.SmokeTests.csproj -c Release -warnaserror
dotnet tests/RmlUi.Net.SmokeTests/bin/Release/net8.0/RmlUi.Net.SmokeTests.dll   # add --window on Windows
```

The smoke tests copy the current platform's native library from
`RmlUi.Net/runtimes/<rid>/native/` into their output directory.

## Packaging

Stage the four native artifacts in `RmlUi.Net/runtimes/`:

```
runtimes/win-x64/native/RmlUiNative.dll
runtimes/win-x86/native/RmlUiNative.dll
runtimes/linux-x64/native/libRmlUiNative.so
runtimes/osx-x64/native/RmlUiNative.dylib
```

```powershell
dotnet pack RmlUi.Net/RmlUi.Net.csproj -c Release -o artifacts
```

`dotnet pack` fails if any of the four artifacts is missing
(`ValidateRuntimeArtifacts` target). The version is set via `VersionPrefix` in
the `.csproj` (see `CHANGELOG.md`).

## NuGet

This repository is an independently maintained fork of the upstream
`Chorizite/RmlUi.Net` project. It is published on nuget.org under its own
package id:

```powershell
dotnet add package PourrezJ.RmlUi.Net
```

The `RmlUi.Net` id on nuget.org belongs to the upstream project and is not
published from this repository.

## CI

- `.github/workflows/ci.yml`: push/PR — native build + parity + smoke tests on
  every platform, package assembly.
- `.github/workflows/release.yml`: manual publishing — native rebuild, repack,
  GitHub Packages push, GitHub release, and nuget.org push when the
  `NUGET_API_KEY` secret is configured.
