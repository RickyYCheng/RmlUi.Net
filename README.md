# RmlUi.Net

C# bindings for [RmlUi](https://github.com/mikke89/RmlUi), an HTML/CSS-style UI
library for applications with their own renderer. The bindings target .NET 8
and include RmlUi 6.3 and FreeType 2.14.3.

The project descends from [Chorizite/RmlUi.Net](https://github.com/Chorizite/RmlUi.Net)
through [PourrezJ/RmlUi.Net](https://github.com/PourrezJ/RmlUi.Net). The package
from this repository is `RickyYC.RmlUi.Net`, not `RmlUi.Net` or `PourrezJ.RmlUi.Net`.
See [the package README](RmlUi.Net/README.md) for usage and
[third-party notices](THIRD-PARTY-NOTICES.md) for redistribution information.

## Supported runtime targets

| Host | .NET runtime identifiers |
| --- | --- |
| Windows | `win-x64`, `win-x86` |
| Linux with glibc | `linux-x64` |
| macOS | `osx-x64`, `osx-arm64` |

Linux CI builds on Ubuntu 22.04. The macOS builds target macOS 12 or later.
Windows Arm64, Linux Arm64, musl and mobile targets are not in the CI matrix.

## Build and test

Use .NET 10 SDK to build and .NET 8 runtime to test. Native builds need Git,
CMake 3.21 or later, Ninja and a C++ compiler. On Windows, use the Visual Studio
developer prompt matching the target architecture. On macOS, use Xcode command
line tools and build on the architecture you want to test.

```sh
git submodule update --init
cmake -S RmlUi.Native -B RmlUi.Native/out/local -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build RmlUi.Native/out/local --parallel
```

Copy the library from `RmlUi.Native/out/local/bin/` into the directory for the
current runtime under `RmlUi.Net/runtimes/<rid>/native/`. The file names are
`RmlUiNative.dll`, `libRmlUiNative.so` and `RmlUiNative.dylib` for Windows, Linux
and macOS respectively. Use separate build and staging directories for different
architectures. The source tree does not ship prebuilt native binaries.

```sh
dotnet build RmlUi.Net.sln -c Release -warnaserror
dotnet test RmlUi.Net.sln -c Release --no-build
dotnet run --project tests/RmlUi.Net.SmokeTests -c Release
```

Pass `-p:RmlNativeRid=win-x86` when building the Windows 32-bit smoke tests, then
run the generated DLL with a 32-bit .NET 8 host. Add `--window` on Windows to
exercise window creation. The [export checker](tools/RmlUiExportParity) compares
managed imports with exports in PE, ELF and Mach-O binaries:

```sh
dotnet run --project tools/RmlUiExportParity -c Release -- \
  --assembly RmlUi.Net/bin/Release/net8.0/RmlUi.Net.dll \
  --native RmlUi.Native/out/local/bin/libRmlUiNative.so
```

## Package and release

`dotnet pack` requires all five runtime libraries and the initialized submodules.
For a local test feed only, use a distinct version:

```sh
dotnet pack RmlUi.Net/RmlUi.Net.csproj -c Release -o artifacts \
  -p:LocalPack=true -p:PackageVersion=1.3.0-local.1
```

For publication, use the [release checklist](RELEASING.md). Do not publish a
local or partial package. Use [KNOWN_ISSUES.md](KNOWN_ISSUES.md) for limitations
of the embedded layout engine.
