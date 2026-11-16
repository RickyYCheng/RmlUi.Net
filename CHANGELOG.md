# Changelog

All notable changes to this project are documented in this file.

## 1.2.0 — unreleased

### Vendored RmlUi upgraded to 6.3

The submodule now tracks the official `6.3` tag (`ba95ffe8`). The C++ binding sources
compiled unchanged against it; the existing `WidgetTextInput` patch still applies
(RmlUi 6.3 fixed a related out-of-bounds access but not the empty-`lines` case) and is
still required.

New RmlUi 6.3 API exposed on the managed side:

- `Rml.LoadFontFace(fileName, family, style, weight, fallbackFace, faceIndex)` — the
  overload behind the new RCSS `@font-face` rule, which registers a face under a
  caller-chosen family instead of the name stored in the file. `FontStyle` is new.
- `ElementDocument.Show(ModalFlag, FocusFlag, ScrollFlag)` — `ScrollFlag` controls
  whether the focused element is scrolled into view; `ScrollAlignment.Adaptive` is new.
- `DocumentScope`/`StructHandle.RegisterMember` and the struct-member exports
  (`rml_StructHandle_RegisterMember{Float,Int,String}`) are now reachable. The
  previous `DataModelConstructor.RegisterStruct` truncated the returned pointer to
  `int`; `RegisterStructHandle` returns the full handle.

### Native/managed ABI corrections

Thirteen exports returned `void` in C++ while the managed side declared `bool` or
`IntPtr`. The managed value was therefore read from an undefined return register, so
the result was effectively random:

- `rml_LoadFontFace`, `rml_RemoveContext`, `rml_Element_Focus` and
  `rml_Element_SetProperty` now really return `bool` (each also null-checks its
  arguments).
- `rml_Log`, `rml_Element_SetClass`, `rml_Element_SetAttributeString`,
  `rml_Element_ScrollTo`, `rml_ElementDocument_SetTitle`, `rml_ElementCustom_Free`,
  `rml_EventInstancer_Free`, `rml_EventListenerInstancer_Free` and
  `rml_RenderInterface_Test` now match their existing `void` declaration on the
  managed side.

`DataModelHandle.IsVariableDirty` returned `void` and discarded the flag; it now
returns `bool`.

### Element tree and query bindings

28 native exports had no managed wrapper. Most importantly, the element-list queries
(`QuerySelectorAll`, `GetElementsByTagName`, `GetElementsByClassName`) existed only as
commented-out native code that returned a pointer to a stack local. They are now
implemented as heap-allocated opaque handles read through `rml_ElementList_*` and
released with `rml_ElementList_Free`, and backed by `IReadOnlyList<Element>` on the
managed side.

Also newly bound: `GetNumChildren`, `GetChild`, `GetFirstChild`, `GetLastChild`,
`GetNextSibling`, `GetPreviousSibling`, `HasChildNodes`, `GetId`/`SetId`,
`GetClientLeft`/`GetClientTop`, `GetOffsetLeft`/`GetOffsetWidth`/`GetOffsetHeight`/
`GetOffsetParent`, `GetScrollTop`/`GetScrollWidth`/`GetScrollHeight`,
`Closest`, `Matches`, `IsClassSet`, `InsertBefore`, `SetInnerText`,
`ScrollIntoView`, `Event.Type` and `Dictionary.Remove`.

### Element ownership and tree-mutation fixes

- `rml_Element_InsertBefore` built an owning `ElementPtr` straight from a raw pointer.
  That created a second owner for an element that was still attached to its tree, so
  the temporary deleted it on return and left the document with a dangling child; any
  later access crashed. It now detaches the element (from its parent or the
  detached-element pool) the way `AppendChild`/`ReplaceChild` already did.
- `rml_Element_SetInnerText` only removed `#text` children, so calling it on an element
  that contained markup left the markup in place. It now removes every child.
- `Element.GetParentNode()` and `Util.GetElementByTypeName` no longer produce a wrapper
  around a null pointer: a missing parent, child or owner document returns `null`
  instead of an object that crashes on first use.
- `Box.GetPosition`/`GetSize` used to allocate a `V2F` on the native heap per call and
  never free it. They now write into caller-provided floats.

### Event dispatch fixes

- `EventPhase` was renumbered densely (`None, Capture, Target, Bubble` = 0..3) while
  RmlUi defines it as bit flags (`Capture = 1, Target = 2, Bubble = 4`). Every
  `Event.Phase` value read from native was therefore reinterpreted: a bubbling event
  reported `4`, which the managed enum called `(EventPhase)4` — not a member at all.
  The managed enum now uses the native values.
- `Element.AddEventListener(string, Action<Event>)` resolved handlers with
  `ev.Id.ToString()`. Ids allocated by `Rml.RegisterEventType` are numbers, so a
  **custom event never reached its handler**. Lookup is now by `ev.Type`, which is
  correct for built-in and custom types alike.
- `DefaultActionPhase` had a `Capture` member that RmlUi's enum does not define (RmlUi
  asserts capture is never part of a default-action phase). It is replaced by
  `TargetAndBubble`, matching `Rml::DefaultActionPhase`.
- `EventId` no longer lists `ColumnAdd`/`RowAdd`/`RowChange`/`RowRemove`/`RowUpdate`:
  6.3's `Rml::EventId` ends at `TabChange`, so those members could never be produced.
- `Element.DispatchEvent` was only declared on `Element<T>`, so it was unreachable
  through the `Element` interface returned by every query method. It is on the
  interface now.
- `Element.Equals`/`GetHashCode` compared `GetInnerRml()`, making distinct empty
  elements equal and changing an element's hash whenever its content changed. They
  compare `NativePtr` now.

### Wrapper cache correctness

- `RmlInstanceCache.GetOrCreate` only rebuilt a cached wrapper when the stale entry
  happened to be an `ElementGeneric`. RmlUi recycles element addresses, so when a
  released `<select>` address was later used by a `<textarea>` (or any other concrete
  type) the lookup threw `InvalidCastException`. Any type mismatch now rebuilds the
  wrapper.
- `Rml.RemoveContext` clears the wrapper cache: destroying a context destroys every
  element it owned, and the cached wrappers for those addresses were dangling.

### Documentation

- New `KNOWN_ISSUES.md` documenting the vendored RmlUi behaviours that surprise
  callers, with file/line references into the submodule: zero-sized layout without a
  resolvable font, non-DOM children (and `<select>` options being unreachable),
  `HasClass` vs the `class` attribute, style changes needing a `Context.Update()`,
  document-wide `GetElementById`, deferred `Close()`, `DispatchEvent`'s return value,
  case-sensitive selectors and the missing `*` tag query, `<rml>`/`<body>` not being
  elements, and more.

### Tests

- New `tests/RmlUi.Net.UnitTests` xunit project covering lifecycle, element
  traversal, selector queries, geometry, variants, dictionary, fonts, data models,
  input, event dispatch, document structure and the wrapper cache (91 tests). Each
  test gets its own context so documents cannot leak between tests.
- Root-caused a flaky test to the wrapper cache bug above rather than papering over
  it; the suite passes repeatedly (verified over consecutive runs).
- Layout tests load two Lato faces that ship with the test project (`assets/`,
  SIL OFL, taken from RmlUi's samples) instead of probing for a system font, so
  results no longer depend on the host machine. Font tests do the same.
- Geometry assertions derive their expected values from the parsed style properties
  rather than hard-coding sizes.
- The smoke suite gained `Element traversal + queries`, `Font faces` and
  `RmlUi 6.3 additions` cases (26 total).
- Both test projects copy the current platform's native library next to the test
  assembly and can switch between the x64 and x86 builds via `RmlNativeArch`.

### Repository

- The default branch is now `main`; `master` was deleted and the CI workflows updated.
- Test and tool projects are part of `RmlUi.Net.sln`.

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
