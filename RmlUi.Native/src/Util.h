#pragma once

#include "RmlUi/Core.h"
#include "RmlUi/Debugger/Debugger.h"

class Util {
public:
    // Returns a stable, compiler-independent type name for the element. The
    // managed side matches on these exact strings, so they intentionally mirror
    // what MSVC's typeid().name() produces ("class Rml::..."). RmlUi's own
    // rmlui_type_name() cannot be used here: on GCC/Clang it returns mangled
    // names from the Itanium ABI.
    static const char *GetElementMarshalId(Rml::Element *element) {
        if (dynamic_cast<Rml::ElementDocument *>(element)) {
            return "class Rml::ElementDocument";
        } else if (dynamic_cast<Rml::ElementFormControlInput *>(element)) {
            return "class Rml::ElementFormControlInput";
        } else if (dynamic_cast<Rml::ElementFormControlSelect *>(element)) {
            return "class Rml::ElementFormControlSelect";
        } else if (dynamic_cast<Rml::ElementFormControlTextArea *>(element)) {
            return "class Rml::ElementFormControlTextArea";
        } else if (dynamic_cast<Rml::ElementProgress *>(element)) {
            return "class Rml::ElementProgress";
        } else if (dynamic_cast<Rml::ElementTabSet *>(element)) {
            return "class Rml::ElementTabSet";
        } else if (dynamic_cast<Rml::ElementText *>(element)) {
            return "class Rml::ElementText";
        } else if (dynamic_cast<Rml::ElementFormControl *>(element)) {
            return "class Rml::ElementFormControl";
        } else if (dynamic_cast<Rml::ElementForm *>(element)) {
            return "class Rml::ElementForm";
        }

        return "class Rml::Element";
    }
};