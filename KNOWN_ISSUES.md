# Known issues

Behaviour of the **vendored RmlUi C++ library** (tag `6.3`, `RmlUi.Native/ext/RmlUi`)
that has bitten this binding or is likely to surprise callers. These are upstream
characteristics, not binding bugs: the binding layer tries to paper over them where it
can, and the listed workarounds are what the tests in `tests/RmlUi.Net.UnitTests` do.

All file/line references point into the pristine submodule at tag `6.3`
(`ba95ffe8`). Paths are relative to `RmlUi.Native/ext/RmlUi/`.

Anything that is a defect *of this fork* (the C#/C++ binding) is tracked in
`CHANGELOG.md` instead.

---

## 1. A missing font silently produces a zero-sized layout

RmlUi performs text layout with the font engine, and the default value of
`font-family` is the empty string:

```cpp
// Source/Core/StyleSheetSpecification.cpp:353
RegisterProperty(PropertyId::FontFamily, "font-family", "", true, true).AddParser("string");
```

With no resolvable `font-family`, `InlineLevelBox::GetFontMetrics`
(`Source/Core/Layout/InlineLevelBox.cpp:28`) returns zeroed-out metrics instead of
failing, and `ElementText::GenerateLine` bails out
(`Source/Core/ElementText.cpp:220`). The result is a document that parses, contains
all the elements, and lays out every content-sized box at 0×0 — with the only signal
being a `LT_WARNING` log line emitted from `LogMissingFontFace`
(`Source/Core/ElementText.cpp:81`).

Note the box, not the font, is what matters: a box with *explicit* `width`/`height`
still gets those dimensions without a font, because layout does not need glyph
metrics for them. Only intrinsic/content sizing collapses.

Measured with the same markup, only `body { font-family: 'LatoLatin'; }` differing
(the intrinsic width is a font metric, so the exact number depends on the face):

| Element | no `font-family` | `font-family` loaded |
| --- | --- | --- |
| `display:block; width:100px; height:50px` | 100×50 | 100×50 |
| `display:block` (auto size) around `"some text"` | **0×0** | non-zero, wider than 0 |

**Workaround:** load a font face before laying out, and always set `font-family`
(usually on `body`) in your style sheet. `RmlUiFixture` in the unit tests loads two
font faces that ship with the test project (`tests/RmlUi.Net.UnitTests/assets/`,
SIL OFL) so layout assertions do not depend on the host machine, and the fixture
fails loudly if they are missing rather than silently weakening the assertions.

---

## 2. `Element::GetNumChildren` excludes non-DOM children, `GetChild` does not

Elements can hold two kinds of children: DOM children and *non-DOM* children,
appended with `AppendChild(element, dom_element = false)`. Only DOM children count
towards `GetNumChildren()`:

```cpp
// Source/Core/Element.cpp:1149
int Element::GetNumChildren(bool include_non_dom_elements) const
{
	return (int)children.size() - (include_non_dom_elements ? 0 : num_non_dom_children);
}
```

but `GetChild(int)` indexes the *full* vector
(`Source/Core/Element.cpp:1141`), and `InsertBefore` can promote an element into
the non-DOM range (`Source/Core/Element.cpp:1400`). So the two accessors disagree
whenever non-DOM children exist:

```
host.NumChildren           = 1        // only the #text child
host.GetChild(0)           = #text
host.GetChild(1)           = span     // non-DOM child, still reachable
host.GetLastChild()        = #text    // last *DOM* child
host.GetInnerRml()         = "x"      // non-DOM children are not serialised
```

`GetInnerRml`, `GetElementsByTagName`, `GetElementsByClassName` and
`QuerySelectorAll` all operate on DOM children only
(`Source/Core/Element.cpp:1154`, `Source/Core/ElementUtilities.cpp:50`).

RmlUi itself creates non-DOM children: `WidgetDropDown` attaches the option
container with `AppendChild(..., false)`
(`Source/Core/Elements/WidgetDropDown.cpp:24`), and `WidgetTextInput` does the same
for its caret/selection elements.

**Workaround:** treat `NumChildren` as a DOM-only count and never derive an
iteration bound for `GetChild` from it. Prefer `GetFirstChild`/`GetNextSibling`
(following only the DOM range) or the `GetElementsBy*` queries. In this binding
`GetChild` returns `null` past the end rather than throwing, so a defensive loop
terminates.

**Consequence for `<select>`:** the `<option>` elements of a `<select>` are *not*
reachable through the element tree. `XMLNodeHandlerSelect` hands each option to
`ElementFormControlSelect::Add`
(`Source/Core/Elements/XMLNodeHandlerSelect.cpp:36`), which attaches it under the
non-DOM option container. Therefore, with `<select id="sel"><option id="o1">…`:

```
doc.GetElementById("o1")            -> null
doc.GetElementsByTagName("option")  -> 0 elements
sel.GetInnerRml()                   -> ""       // options are not DOM children
sel.NumChildren                     -> 0
```

RmlUi's own API for this is `ElementFormControlSelect::GetOption`/`GetValue`/
`SetValue`. The managed `ElementFormControlSelect` wrapper is currently an empty
shell, so a `<select>` cannot be inspected from .NET at all.

---

## 3. `HasClass` and the `class` *attribute* are separate stores

`Element::SetClass` and friends mutate the element's style-side class set, and the
`class` attribute is not updated:

```cpp
// Source/Core/Element.cpp:260
void Element::SetClass(const String& class_name, bool activate)
{
	if (meta->style.SetClass(class_name, activate))
		DirtyDefinition(DirtyNodes::SelfAndSiblings);
}
```

Observed:

```
host.GetAttribute("class")   = "b"     // unchanged by the calls below
host.AddClass("c")           -> HasClass("c") == true,  attribute still "b"
host.RemoveClass("b")        -> HasClass("b") == false, attribute still "b"
host.SetAttribute("class", "b") -> resets the class set
```

The two are synchronised only when the `class` attribute is assigned (that is what
`SetClassNames`/`SetAttribute("class", …)` do).

**Workaround:** pick one API and stay in it. Use `AddClass`/`RemoveClass`/`HasClass`
(or `IsClassSet`) for programmatic changes, and read the `class` attribute only when
you need the original markup. Do not expect `GetAttribute("class")` to reflect
classes you added.

**Related:** `<input>` mirrors its `type` attribute into a CSS class. Changing
`type` to `"text"` calls `SetClass("text", true)`
(`Source/Core/Elements/ElementFormControlInput.cpp:126`), so an
`<input type="text">` matches the selector `.text` and answers
`HasClass("text") == true` without any `class` attribute — and so does every
`<p class="text">` in the document. `type="password"` sets the class `password`
instead, and an unrecognised type falls back to `text`
(`Source/Core/Elements/ElementFormControlInput.cpp:118`).

---

## 4. Style changes need a `Context::Update()` before layout reflects them

`SetProperty`, `AddClass`, `SetInnerRml` and friends mark the element dirty; the box
is recomputed during the next `Context::Update()`.

```
SetProperty("width", …)
  GetProperty("width")  -> the new value    // immediately
  GetOffsetWidth()      -> the old width    // still the previous layout
Context.Update()
  GetOffsetWidth()      -> the new width    // now updated
```

The same lag applies to class changes that alter layout: removing a class that
supplies `display: block` leaves the element's measured box at its block size until
the next `Update()`, then it collapses to the inline size.

**Workaround:** call `Context.Update()` after mutating styles and before reading
geometry. `DocumentScope.Load` in the unit tests does this once for the caller.

---

## 5. `GetElementById` ignores the subtree it is called on, and has magic names

`Element::GetElementById` searches from the **owner document**, not from the
receiver, and recognises three special tokens:

```cpp
// Source/Core/Element.cpp:1514
Element* Element::GetElementById(const String& id)
{
	if (id == "#self")     return this;
	else if (id == "#document") return GetOwnerDocument();
	else if (id == "#parent")   return this->parent;
	else {
		Element* search_root = GetOwnerDocument();   // <- whole document
		...
	}
}
```

```cpp
// Source/Core/Element.cpp:1525
Element* search_root = GetOwnerDocument();
if (search_root == nullptr)
	search_root = this;
return ElementUtilities::GetElementById(search_root, id);
```

Measured: calling `GetElementById` on a nested element finds an element in a *sibling*
subtree of that element, because the search restarts at the document. By contrast
`GetElementsByTagName`/`GetElementsByClassName`/`QuerySelectorAll` are subtree-scoped.

The search is breadth-first (`Source/Core/ElementUtilities.cpp:25`), so with
duplicate ids the shallowest element wins; ids are not required to be unique.

**Workaround:** use `QuerySelector`/`QuerySelectorAll` when you need subtree scope.
Avoid the `#`-prefixed names as real ids.

---

## 6. `Close()` does not destroy the document; it is deferred to `Update()`

```cpp
// Source/Core/ElementDocument.cpp:423
void ElementDocument::Close()
{
	if (context != nullptr)
		context->UnloadDocument(this);
}
```

`UnloadDocument` only queues the document
(`Source/Core/Context.cpp:326`); the memory is released in
`ReleaseUnloadedDocuments`, called from `Context::Update`
(`Source/Core/Context.cpp:217`) and from the destructor.

**Workaround:** after `Close()`, treat every wrapper obtained from that document
(including the `ElementDocument` itself) as dangling. Do not touch them after the
next `Update()`. Call `Close()` at most once — a second call is a no-op only
because `UnloadDocument` de-duplicates, and calling it after the context has already
released the document is a use-after-free.

---

## 7. `DispatchEvent` returns "still propagating", which is not success

```cpp
/// @return True if the event was not consumed (ie, was prevented from propagating by an element), false if it was.
bool DispatchEvent(const String& type, const Dictionary& parameters, bool interruptible, bool bubbles = true);
```

Two consequences that are easy to misread:

- When **nobody is listening**, `EventDispatcher::DispatchEvent` returns early with
  `true` — an empty event reports exactly the same value as a fully handled one:
  ```cpp
  // Source/Core/EventDispatcher.cpp:134
  if (listeners.empty() && default_action_elements.empty())
      return true;
  ```
- `false` means "a listener called `StopPropagation`" (or the event was not
  interruptible and a default action consumed it), not "the event failed".

Events registered through `Rml.RegisterEventType` are **not** in RmlUi's built-in
specification table, so no default action exists for them; only listeners make them
observable.

**Workaround:** do not use the return value as a delivery check. Observe the
listener callback instead.

---

## 8. Custom event ids are not `EventId` values

`Rml::EventId` is a `uint16_t` enum whose named values stop at `Tabchange`
(`Include/RmlUi/Core/ID.h:204`, `NumDefinedIds`), and custom types get ids from
`FirstCustomId` upwards. `Rml::RegisterEventType` returns such an id — a bare number
past the end of the named range.

Casting that to the managed `EventId` therefore produces an arbitrary value that
coincidentally may equal a named member. Any code keyed off `Event.Id` must not
assume the value is meaningful for custom types.

**Workaround:** key off `Event.Type` (the string), which is stable for both built-in
and custom events. That is what this binding's
`Element.AddEventListener(string, Action<Event>)` overload does internally.

---

## 9. Selector support is a subset of CSS, and tag matching is case-sensitive

`QuerySelector`/`QuerySelectorAll`/`Matches`/`Closest` reuse RmlUi's RCSS selector
parser (`Source/Core/StyleSheetParser.cpp`). Verified against 6.3 as supported:

- tag, `#id`, `.class`, `*`;
- attribute selectors with `=` `~=` `|=` `^=` `$=` `*=`, quoted or bare
  (`[data-v=1]` and `[data-v="1"]` both work);
- descendant (`div span`), child (`div > span`) **and** sibling combinators
  (`span + span`, `span ~ span`);
- selector lists (`#s1, #s2`);
- these structural pseudo-classes (`Source/Core/StyleSheetFactory.cpp:14`):
  `nth-child`, `nth-last-child`, `nth-of-type`, `nth-last-of-type`, `first-child`,
  `last-child`, `first-of-type`, `last-of-type`, `only-child`, `only-of-type`,
  `empty`, `not`, `scope`.

The important deviation from CSS is **case sensitivity**. The XML parser lowercases
tags (`<DIV>` becomes `div`), but selector matching compares the raw string
(`Source/Core/StyleSheetNode.cpp:343`), so `QuerySelectorAll("DIV")` returns nothing
while `"div"` matches. The same applies to
`GetElementsByTagName`/`GetElementsByClassName` (`Source/Core/ElementUtilities.cpp:63`).

`GetElementsByTagName` also has **no wildcard**: `GetElementsByTagName("*")` returns
zero elements, whereas `QuerySelectorAll("*")` works. Use the selector API when you
need "all descendants".

State pseudo-classes (`:hover`, `:focus`, `:active`) are only meaningful inside a
style sheet; as *query* selectors they simply match nothing.

The parser is not error-reporting: text that fails to parse yields an empty node
list, so `QuerySelectorAll("<")` returns zero matches instead of throwing. The
binding rejects `null`/empty selector strings with `ArgumentNullException`; that is
the only validation available.

---

## 10. `SetInnerRml` parses the string as RML, and whitespace-only text is dropped

`Factory::InstanceElementText` treats input containing `<` as markup and returns
early for whitespace-only input:

```cpp
// Source/Core/Factory.cpp:339
const bool only_white_space = std::all_of(text.begin(), text.end(), &StringUtilities::IsWhitespace);
if (only_white_space)
	return true;
```

So `SetInnerRml("   ")` leaves the element empty, and malformed markup is silently
mangled rather than rejected:

```
SetInnerRml("<")            -> GetInnerRml() == "<< body=\"\" />"
SetInnerRml("a &amp; b")    -> "a &amp; b"      (entity kept as-is)
SetInnerText("a < b & c")   -> "a &lt; b &amp; c"  (escaped correctly)
```

**Workaround:** use `SetInnerText` for text (it escapes for you), `SetInnerRml` only
for well-formed markup, and check the result with `GetInnerRml()` if the input is
not under your control.

Elements with a custom node handler do not behave like ordinary elements here. Setting
`SetInnerRml` on a `<textarea>` appends a DOM child but leaves the `value` attribute
— the content the widget actually edits — untouched, because the parser stores
textarea data there (`Source/Core/Elements/XMLNodeHandlerTextArea.cpp`). Use the
form-control value API instead. The same warning applies to `<select>`, whose options
are non-DOM children (see §2).

---

## 11. RmlUi must be initialised before almost any other call

`Rml::Initialise` installs the default interfaces and global state; nearly every
other entry point dereferences it. Calling into an uninitialised library is a hard
crash, not an exception:

```
Rml.LoadFontFace("nope.ttf")   without Rml.Initialise()  -> AccessViolationException (fatal)
Rml.Shutdown()                 without Rml.Initialise()  -> AccessViolationException (fatal)
Rml.RemoveContext("x")         without Rml.Initialise()  -> AccessViolationException (fatal)
Rml.CreateContext(...)         without Rml.Initialise()  -> null
```

`Initialise` uses `RMLUI_ASSERTMSG(!initialised, ...)`, which is compiled out in
release builds, so a second `Initialise()` returns `true` harmlessly. `Shutdown`
followed by `Initialise` also works.

**Workaround:** initialise once, from one place, before touching anything else.
`RmlUiFixture` owns that for the test suites. Do not rely on the library to detect
misuse.

---

## 12. `Element::Focus()` succeeds for any element with `focus: auto`

Focus is not restricted to form controls. `Element::Focus`
(`Source/Core/Element.cpp:1181`) only refuses when the computed `focus` property is
`none` (which `StyleSheetSpecification.cpp:376` defaults to `auto`) or the element
has no context. Measured: `div`, `p` and even the document body all return `true`
from `Focus()` and become `Context.GetFocusElement()`.

There is no built-in "focusable" predicate — RmlUi's own shell code checks
`tabindex`/`autofocus` itself. `FocusFlag.Auto` on `ElementDocument.Show` uses the
`autofocus` attribute (`FocusFlag` is declared in
`Include/RmlUi/Core/ElementDocument.h:22`); that is a separate mechanism from
`Element.Focus()`.

---

## 13. The `rml` root tag is not an element, and `<body>` becomes the document

Documents are parsed with the context's `documents_base_tag` as the root element
(`Include/RmlUi/Core/Context.h:303`, default `"body"`), so
`Factory::InstanceDocumentStream` creates the document *as* the root
(`Source/Core/Factory.cpp:429`). The `<rml>` and `<head>` tags only route parsing —
`XMLNodeHandlerHead` returns `nullptr` for every element it handles, i.e. it
constructs nothing (`Source/Core/XMLNodeHandlerHead.cpp:94`). Verified:

```
doc.TagName                -> "body"
doc <=> doc.GetElementById("body")      // the <body> attributes went onto the document
doc.GetElementsByTagName("rml")         -> 0
doc.GetElementsByTagName("body")        -> 0    // the document is the body, not a child of it
doc.GetInnerRml()                       -> "<div id=\"d\">x</div>"
ctx.GetRootElement().TagName            -> "#root"
ctx.GetRootElement().GetChild(0)        -> the ElementDocument
```

`XMLNodeHandlerBody` copies the `<body>` element's attributes onto the
`ElementDocument` itself (`Source/Core/XMLNodeHandlerBody.cpp:20`), which is why
`id="body"` makes the *document* answer to that id.

**Workaround:** do not look for `rml` in queries. Use `Context.GetRootElement()` for
the context's outer node (an anonymous `#root`) and treat `ElementDocument` — which
is itself an `Element` and a child of `#root` — as the document body. Note
`GetElementsByTagName("body")` returns nothing, and `GetParentNode()` of any
top-level element returns the `ElementDocument`.

---

## 14. Text nodes are real children with the tag `#text`

`<div id="plain">p</div>` gives `plain.NumChildren == 1`, and `plain.GetChild(0)` is
an `ElementText` wrapper with `TagName == "#text"` and `GetInnerRml() == "p"`.
`GetChild`, `GetElementsByTagName("#text")` and `GetInnerRml` all see text nodes,
but the selector walkers skip them explicitly:

```cpp
// Source/Core/Element.cpp:1549  (QuerySelector)
// Source/Core/Element.cpp:1572  (QuerySelectorAll)
if (child->GetTagName() == "#text")
	continue;
```

So `NumChildren` counts text nodes, `GetElementsByTagName("#text")` finds them, and
`QuerySelectorAll("*")` does not. Use `ElementText` when you need the text value.

---

## 15. Geometry accessors are only meaningful after a layout pass

`GetOffsetWidth`, `GetClientHeight`, `GetScrollHeight`, `GetAbsoluteLeft`, … read the
cached box and offset fields rather than recomputing anything:

```cpp
// Source/Core/Element.cpp:977
float Element::GetOffsetLeft()
{
	return relative_offset_base.x + relative_offset_position.x;
}

// Source/Core/Element.cpp:987
float Element::GetOffsetWidth() { return GetBox().GetSize(BoxArea::Border).x; }
```

Consequently, before the first layout they return `0` (not `NaN`), and after a style
change they return the previous values until the next `Context.Update()` — see §4.
`GetOffsetParent()` is a plain accessor for the cached `offset_parent`
(`Source/Core/Element.cpp:972`) with no null-guard or fallback, so it reflects
whatever the last definition/layout pass computed.

`GetOffsetLeft/Top` are the position within that offset parent;
`GetAbsoluteLeft/Top` are relative to the context origin.

---

## 16. The density ratio scales `dp`, not `px`

`Context::SetDensityIndependentPixelRatio` looks like a global UI scale, but it only
affects lengths written in `dp` and PPI units, plus the intrinsic size of some form
controls. `px` is a physical unit and is never multiplied by it:

```cpp
// Source/Core/ComputeProperty.cpp:52
float ComputeLength(NumericValue value, float font_size, float document_font_size, float dp_ratio, Vector2f vp_dimensions)
{
	if (Any(value.unit & Unit::PPI_UNIT))
		return ComputePPILength(value, dp_ratio);

	switch (value.unit)
	{
	case Unit::PX: return value.number;              // <- ratio not applied
	case Unit::EM: return value.number * font_size;
	case Unit::REM: return value.number * document_font_size;
	case Unit::DP: return value.number * dp_ratio;   // <- only dp scales
	default: break;
	}
```

`font-size` goes through the same function (`Source/Core/ComputeProperty.cpp:117`), so a
sheet that declares `font-size: 12px` keeps a 12-physical-pixel em box at every ratio.

This matters because text is rasterised at the computed size: the glyph atlas is built
from `(int)font_size` (`Source/Core/ElementStyle.cpp:1000`), with no scaling afterwards.
Setting a ratio of 2 therefore does *not* sharpen an existing pixel-based sheet — it
leaves the glyphs at the same 12 px bitmap and simply changes how much screen area
`dp` lengths occupy. To get text rasterised at the display's real resolution the styles
themselves have to use `dp`.

Two related details:

- The ratio is per context and is applied when computed values are recalculated, so it
  must be set before the document is loaded (or followed by `Context::Update()`).
- Glyph quads are snapped to whole pixels (`(position + box.origin).Round()`,
  `Source/Core/FontEngineDefault/FontFaceLayer.h:57`), so at ratio 1 the atlas is sampled
  1:1 and filtering never blurs it. Blurriness at that ratio is a *contrast* problem —
  the coverage values in the atlas blended linearly — not a sampling problem.

---

## Reporting

If an issue here is wrong, or a binding-side workaround is missing, open an issue at
`https://github.com/RickyYCheng/RmlUi.Net`. Upstream RmlUi issues belong at
`https://github.com/mikke89/RmlUi`.
