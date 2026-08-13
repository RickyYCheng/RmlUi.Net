# RmlUi.Net

Bindings C# pour [RmlUi](https://github.com/mikke89/RmlUi), bibliothèque d'interface
utilisateur native basée sur HTML/CSS. RmlUi embarqué : **6.2**.

## Structure

- `RmlUi.Native/` — DLL natif `RmlUiNative` (C++17, CMake), sources RmlUi + FreeType en
  sous-modules.
- `RmlUi.Net/` — bibliothèque managée (`net8.0`) et artefacts natifs par RID
  (`runtimes/<rid>/native/`).
- `tools/RmlUiExportParity/` — outil de vérification de la parité exports natifs ↔
  imports P/Invoke (parseurs PE/ELF/Mach-O intégrés, aucune dépendance).
- `tests/RmlUi.Net.SmokeTests/` — suite de fumée couvrant toutes les API publiques.

## Prérequis

- SDK .NET 8
- CMake ≥ 3.21 + Ninja
- Windows : Visual Studio 2022 (charge de travail C++) ; Linux : GCC/G++ ; macOS : Xcode CLT

## Construire le natif

```powershell
# Windows x64 (invite développeur x64)
cmake --preset x64-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/x64-release --parallel

# Windows x86 (invite développeur x86, vcvars32)
cmake -S RmlUi.Native -B RmlUi.Native/out/build/x86-release -G Ninja -DCMAKE_BUILD_TYPE=Release
cmake --build RmlUi.Native/out/build/x86-release --parallel

# Linux
cmake --preset linux-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/linux-release --parallel

# macOS (binaire universel arm64+x86_64)
cmake --preset macos-release -S RmlUi.Native
cmake --build RmlUi.Native/out/build/macos-release --parallel
```

Les sorties sont dans `out/build/<preset>/bin/` (`RmlUiNative.dll`,
`libRmlUiNative.so` ou `RmlUiNative.dylib`).

## Construire le managé et tester

```powershell
dotnet build RmlUi.Net/RmlUi.Net.csproj -c Release -warnaserror

# Parité exports/imports (échoue si un DllImport n'a pas d'export natif)
dotnet build tools/RmlUiExportParity/RmlUiExportParity.csproj -c Release
dotnet tools/RmlUiExportParity/bin/Release/net8.0/RmlUiExportParity.dll `
  --assembly RmlUi.Net/bin/Release/net8.0/RmlUi.Net.dll `
  --native RmlUi.Native/out/build/x64-release/bin/RmlUiNative.dll

# Tests de fumée (copier d'abord le natif de la plateforme courante dans
# RmlUi.Net/runtimes/<rid>/native/, voir ci-dessous)
dotnet build tests/RmlUi.Net.SmokeTests/RmlUi.Net.SmokeTests.csproj -c Release -warnaserror
dotnet tests/RmlUi.Net.SmokeTests/bin/Release/net8.0/RmlUi.Net.SmokeTests.dll   # + --window sur Windows
```

Les tests de fumée copient la bibliothèque native de la plateforme courante depuis
`RmlUi.Net/runtimes/<rid>/native/` vers le répertoire de sortie.

## Empaqueter

Stager d'abord les quatre artefacts natifs dans `RmlUi.Net/runtimes/` :

```
runtimes/win-x64/native/RmlUiNative.dll
runtimes/win-x86/native/RmlUiNative.dll
runtimes/linux-x64/native/libRmlUiNative.so
runtimes/osx-x64/native/RmlUiNative.dylib
```

```powershell
dotnet pack RmlUi.Net/RmlUi.Net.csproj -c Release -o artifacts
```

`dotnet pack` échoue si l'un des quatre artefacts est manquant (cible
`ValidateRuntimeArtifacts`). La version est fixée par `VersionPrefix` dans le
`.csproj` (voir `CHANGELOG.md`).

## CI

- `.github/workflows/ci.yml` : push/PR — build natif + parité + tests de fumée sur
  chaque plateforme, packaging.
- `.github/workflows/release.yml` : publication manuelle — repack + push NuGet
  (nuget.org et GitHub Packages) + release GitHub.
