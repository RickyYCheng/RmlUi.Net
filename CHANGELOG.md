# Changelog

Toutes les modifications notables de ce projet sont documentées ici.

## 1.1.0 — 2026-08-13

### Restauration des 51 exports natifs manquants (parité exports/imports : 100 %)

Le package 1.0.1 publiait un `RmlUiNative.dll` compilé depuis un commit antérieur aux
sources managées : 51 points d'entrée P/Invoke n'avaient pas d'export natif et levaient
`EntryPointNotFoundException` à l'exécution.

- 48 exports étaient déjà présents dans les sources natives de ce dépôt ; ils sont
  désormais effectivement compilés dans la DLL publiée.
- 3 exports étaient réellement absents du côté natif et ont été implémentés :
  - `rml_DataModelConstructor_BindStringArray`
  - `rml_DataModelConstructor_UnbindStringArray`
  - `rml_DataModelConstructor_RegisterStruct`
- Nouvel export utilitaire `rml_String_Free` : les chaînes allouées côté natif
  (`strdup`/`malloc`) sont maintenant libérées par l'allocateur correspondant au lieu
  de `Marshal.FreeHGlobal` (allocateur incompatible, corruption de tas possible).
- Résultat vérifié par l'outil de parité `tools/RmlUiExportParity` :
  **174 entrées DllImport ↔ 205 exports natifs**, sur win-x64 et win-x86.
  L'outil analyse les tables d'exports PE, ELF et Mach-O et échoue si une entrée
  managée n'a pas d'export natif (étape bloquante de la CI).

### Correction de la gestion des chaînes vides

- `Element.SetAttribute` et `Element.SetProperty` acceptent désormais une valeur vide
  (`""`), qui est une valeur légitime ; ils lèvent toujours `ArgumentNullException`
  pour `null`.

### Correctifs de robustesse du binding natif

- `rml_ElementDocument_CreateElement` / `CreateTextNode` ne renvoient plus l'adresse
  d'un temporaire (`ElementPtr`) : les éléments détachés sont conservés dans un pool
  jusqu'à leur rattachement (`rml_Element_AppendChild`, `rml_Element_ReplaceChild`).
- `rml_Element_RemoveChild` conserve la propriété rendue par RmlUi 6.0 (`ElementPtr`)
  afin qu'un enfant retiré puisse être ré-attaché sans être détruit.
- `rml_Element_SetInnerText` est réellement implémenté (suppression des nœuds texte
  existants + ajout du nouveau nœud).
- `rml_ElementDocument_AddStyleSheetContainer` ne crashe plus lorsque le document n'a
  pas encore de conteneur de feuilles de style.
- `rml_Shutdown` libère les éléments détachés restants avant `Rml::Shutdown`, afin que
  les callbacks managés des instanciers s'exécutent pendant que le côté managé est
  encore vivant (plus de crash au déchargement du processus).
- Gardes de pointeurs nuls ajoutées sur les exports propriétés/dictionnaire/éléments :
  plus de déréférencement silencieux de pointeur nul.
- `rml_Property_GetString` et `rml_Element_GetPropertyString` renvoient la valeur avec
  son unité (`"50px"` et non `"50"`) via `Property::ToString()`.
- Toutes les chaînes natives utilisent l'allocateur portable `RMLUI_DUP_STR`
  (`_strdup` sous MSVC, `strdup` ailleurs) : le code natif compile sous GCC/Clang.
- Compilation native avec `/W4 /WX` (MSVC) et `-Wall -Wextra -Werror` (GCC/Clang) :
  zéro avertissement.

### Version de RmlUi embarquée

- Le sous-module `RmlUi.Native/ext/RmlUi` est épinglé sur **RmlUi 6.2** (tag `6.2`),
  mis à niveau depuis 6.0. Les changements 6.1/6.2 sont additifs pour le binding
  (entrées tactiles `ProcessTouch*`, surcharges `PropertyId` des animations,
  `Element::Contains`, correctifs de stabilité) : aucun export n'a changé de
  signature.
- FreeType embarqué : 2.12.1 (sous-module), compilé depuis les sources pour toutes
  les plateformes.

### Plateformes et packaging

- Artefacts natifs : `win-x64`, `win-x86`, `linux-x64` (`libRmlUiNative.so`),
  `osx-x64` (binaire universel arm64+x86_64 : `RmlUiNative.dylib`).
- Le `.nupkg` contient les quatre RID dans `runtimes/<rid>/native/`.
- Garde-fou de packaging : `dotnet pack` échoue si l'un des quatre artefacts natifs
  est manquant (cible MSBuild `ValidateRuntimeArtifacts`).
- Version explicite dans le `.csproj` (`VersionPrefix`), GitVersion retiré : les
  versions sont fixées à la main, reproductibles localement.

### CI et tests

- Pipeline GitHub Actions :
  - build natif + parité exports/imports sur chaque plateforme (`native-build.yml`) ;
  - tests de fumée sur Windows/Linux/macOS (`ci.yml`) ;
  - packaging avec vérification des quatre artefacts (`ci.yml`, `release.yml`).
- Suite de tests de fumée `tests/RmlUi.Net.SmokeTests` : 22 tests exerçant toutes les
  API publiques, dont les 51 exports restaurés ; une fenêtre Win32 masquée est créée
  avec `--window` sur Windows.

### Compatibilité de l'API managée

- Aucun type, méthode ou énumération publique n'a été supprimé ni renommé.
- Annotations de nullabilité corrigées (ex. `Element.OwnerDocument` renvoie
  `ElementDocument?`) : impact limité aux métadonnées, pas au code consommateur.
- `InternalsVisibleTo("RmlUi.Net.SmokeTests")` ajouté pour permettre aux tests de
  couvrir les wrappers P/Invoke internes.

## 1.0.1 — 2025-02-28

- Version initiale publiée (désynchronisée entre DLL managé et DLL natif : 51
  entrées P/Invoke sans export natif — corrigé en 1.1.0).
