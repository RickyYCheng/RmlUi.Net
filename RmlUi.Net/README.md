# RmlUi.Net

Bindings C# pour [RmlUi](https://github.com/mikke89/RmlUi) — moteur d'interface
utilisateur natif basé sur HTML/CSS. RmlUi embarqué : **6.0**.

## Plateformes supportées

| RID | Fichier natif |
| --- | --- |
| `win-x64`, `win-x86` | `runtimes/<rid>/native/RmlUiNative.dll` |
| `linux-x64` | `runtimes/linux-x64/native/libRmlUiNative.so` |
| `osx-x64` | `runtimes/osx-x64/native/RmlUiNative.dylib` (universel arm64+x86_64) |

## Démarrage rapide

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

Restauration des 51 exports natifs manquants de la 1.0.1 (plus aucune
`EntryPointNotFoundException`), correction de `Element.SetAttribute`/`SetProperty`
pour les chaînes vides, builds natifs sur les quatre plateformes. Détails dans le
[CHANGELOG](https://github.com/Chorizite/RmlUi.Net/blob/master/CHANGELOG.md).

## Licence

MIT. RmlUi (MIT) et FreeType (FTL/GPLv2) sont embarqués sous leurs licences
respectives.
