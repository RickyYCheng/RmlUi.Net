# Changelog

All notable changes to this project are documented in this file.

## 1.1.1 — 2026-08-14

### Backspace in text inputs / native bool marshalling fixes

- The vendored RmlUi (tag 6.2) crashed or read garbage when key events reached a
  focused `<input type="text">` before its lines were formatted (no layout yet,
  or no font face available): `WidgetTextInput::GetRelativeCursorIndices`
  indexed `lines[-1]` on an empty line list. The fix is shipped as a git patch
  (`RmlUi.Native/patches/0001-WidgetTextInput-GetRelativeCursorIndices-empty-lines.patch`)
  applied by CMake at configure time, keeping the submodule pristine at the
  official tag 6.2: the cursor now maps to the start of the field when there
  are no lines. This is what made
  `Context.ProcessKeyDown(Input::KI_BACK, 0)` fail (or crash on 32-bit builds)
  while text input and arrow keys kept working.
- All P/Invoke declarations returning a native C++ `bool` now marshal the
  result with `[return: MarshalAs(UnmanagedType.I1)]`. Previously the 4-byte
  BOOL marshalling read garbage upper bits on 32-bit Release builds, so
  `Element.HasAttribute`/`HasClass`, `Event.IsPropagating`, and every other
  boolean query could return the wrong value.
- New smoke test `Input backspace deletion`: loads `<input type="text">`, types
  `"ab"` with `ProcessTextInput`, presses `KI_BACK`, and checks the value is
  `"a"` (plus a Delete no-op at end of text and a second backspace).
- CI now also runs the full smoke suite against the win-x86 native build under
  a 32-bit .NET host, so 32-bit-only breakage cannot ship again.

## 1.1.0 — 2026-08-13

Published from the maintained fork `PourrezJ/RmlUi.Net` under the NuGet id
**PourrezJ.RmlUi.Net** (the `RmlUi.Net` id on nuget.org belongs to the upstream
project). The managed API is unchanged: this package is a drop-in replacement
with the same `RmlUiNet` namespace and assembly name.

### Restoration of the 51 missing native exports (100 % export/import parity)

Package 1.0.1 shipped an `RmlUiNative.dll` built from a commit that predated the
managed sources: 51 P/Invoke entry points had no native export and threw
`EntryPointNotFoundException` at run time.

- 48 exports already existed in this repository's native sources; they are now
  actually compiled into the published DLL.
- 3 exports were truly absent on the native side and have been implemented:
  - `rml_DataModelConstructor_BindStringArray`
  - `rml_DataModelConstructor_UnbindStringArray`
  - `rml_DataModelConstructor_RegisterStruct`
- New utility export `rml_String_Free`: strings allocated on the native side
  (`strdup`/`malloc`) are now freed with the matching allocator instead of
  `Marshal.FreeHGlobal` (incompatible allocator, potential heap corruption).
- Verified by the `tools/RmlUiExportParity` checker: **174 DllImport entries ↔
  205 native exports**, on win-x64 and win-x86. The tool parses PE, ELF and
  Mach-O export tables and fails when a managed entry point has no native
  export (blocking CI step).

### Empty string handling fix

- `Element.SetAttribute` and `Element.SetProperty` now accept an empty string
  (`""`), which is a legal value; they still throw `ArgumentNullException` for
  `null`.

### Native binding robustness fixes

- `rml_ElementDocument_CreateElement` / `CreateTextNode` no longer return the
  address of a temporary (`ElementPtr`): detached elements are pooled until
  they are attached (`rml_Element_AppendChild`, `rml_Element_ReplaceChild`).
- `rml_Element_RemoveChild` keeps the ownership returned by RmlUi 6.0
  (`ElementPtr`) so a removed child can be re-attached without being destroyed.
- `rml_Element_SetInnerText` is actually implemented (removes existing text
  nodes and appends the new one).
- `rml_ElementDocument_AddStyleSheetContainer` no longer crashes when the
  document has no style sheet container yet.
- `rml_Shutdown` releases remaining detached elements before `Rml::Shutdown`,
  so instancer callbacks run while the managed side is still alive (no more
  crash at process teardown).
- Null pointer guards added to the property/dictionary/element exports: no
  more silent null dereferences.
- `rml_Property_GetString` and `rml_Element_GetPropertyString` return the value
  with its unit (`"50px"` instead of `"50"`) via `Property::ToString()`.
- All native strings use the portable `RMLUI_DUP_STR` allocator helper
  (`_strdup` on MSVC, `strdup` elsewhere): the native code compiles with
  GCC/Clang.
- Native builds use `/W4 /WX` (MSVC) and `-Wall -Wextra -Werror` (GCC/Clang):
  zero warnings.

### Embedded RmlUi version

- The `RmlUi.Native/ext/RmlUi` submodule is pinned to **RmlUi 6.2** (tag `6.2`),
  upgraded from 6.0. The 6.1/6.2 changes are additive for the binding
  (`ProcessTouch*` touch input, `PropertyId` animation overloads,
  `Element::Contains`, stability fixes): no export signature changed.
- Embedded FreeType: 2.12.1 (submodule), built from source on every platform.

### Platforms and packaging

- Native artifacts: `win-x64`, `win-x86`, `linux-x64` (`libRmlUiNative.so`),
  `osx-x64` (universal arm64+x86_64 binary: `RmlUiNative.dylib`).
- The `.nupkg` ships all four RIDs under `runtimes/<rid>/native/`.
- Packaging guard: `dotnet pack` fails if any of the four native artifacts is
  missing (`ValidateRuntimeArtifacts` MSBuild target).
- Explicit version in the `.csproj` (`VersionPrefix`), GitVersion removed:
  versions are set by hand and reproducible locally.

### CI and tests

- GitHub Actions pipeline:
  - native build + blocking export/import parity on every platform
    (`native-build.yml`);
  - smoke tests on Windows/Linux/macOS (`ci.yml`);
  - packaging with all four artifacts verified (`ci.yml`, `release.yml`).
- `tests/RmlUi.Net.SmokeTests` suite: 22 tests exercising every public API,
  including all 51 restored exports; a hidden Win32 window is created with
  `--window` on Windows.

### Managed API compatibility

- No public type, method or enum was removed or renamed.
- Nullability annotations fixed (e.g. `Element.OwnerDocument` returns
  `ElementDocument?`): metadata-only impact, no consumer code changes.
- `InternalsVisibleTo("RmlUi.Net.SmokeTests")` added so tests can cover the
  internal P/Invoke wrappers.

## 1.0.1 — 2025-02-28

- Initial published release (managed and native DLLs out of sync: 51 P/Invoke
  entry points without a native export — fixed in 1.1.0).
