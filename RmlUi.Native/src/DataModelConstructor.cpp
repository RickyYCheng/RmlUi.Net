#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/DataStructHandle.h"
#include <string>
#include <sstream>
#include <unordered_map>

typedef void (*dataEventFunc)(Rml::DataModelHandle* model_handle, Rml::Event& evt, Rml::Variant** variants, int num_variants);

// Storage for string arrays bound through rml_DataModelConstructor_BindStringArray.
// RmlUi binds data variables by pointer, so the backing vector must outlive the binding.
static std::unordered_map<Rml::DataModelConstructor*, std::unordered_map<std::string, Rml::Vector<Rml::String>>> stringArrayPool;

// Storage for the fake struct handles returned by rml_DataModelConstructor_RegisterStruct.
// A struct type can only be registered once per data type register.
static std::unordered_map<Rml::DataModelConstructor*, Rml::StructHandle<FakeStruct>> structHandlePool;

RMLUI_CAPI bool rml_DataModelConstructor_BindFloat(Rml::DataModelConstructor* data_model, const char* name, float* data) {
  return data_model->Bind(name, data);
}

RMLUI_CAPI bool rml_DataModelConstructor_BindUInt(Rml::DataModelConstructor* data_model, const char* name, unsigned int* data) {
  return data_model->Bind(name, data);
}

RMLUI_CAPI bool rml_DataModelConstructor_BindInt(Rml::DataModelConstructor* data_model, const char* name, int* data) {
  return data_model->Bind(name, data);
}

RMLUI_CAPI bool rml_DataModelConstructor_BindBool(Rml::DataModelConstructor* data_model, const char* name, bool* data) {
  return data_model->Bind(name, data);
}

RMLUI_CAPI bool rml_DataModelConstructor_BindString(Rml::DataModelConstructor* data_model, const char* name, Rml::String* data) {
  return data_model->Bind(name, data);
}

RMLUI_CAPI bool rml_DataModelConstructor_BindStringArray(Rml::DataModelConstructor* data_model, const char* name, const char** data, int count) {
  if (data_model == nullptr || name == nullptr || (data == nullptr && count > 0) || count < 0) {
    return false;
  }

  auto& arrays = stringArrayPool[data_model];
  if (arrays.find(name) != arrays.end()) {
    return false;
  }
  Rml::Vector<Rml::String> values;
  values.reserve(static_cast<size_t>(count));
  for (int i = 0; i < count; ++i) {
    values.emplace_back(data[i] != nullptr ? data[i] : "");
  }

  auto* types = data_model->GetDataTypeRegister();
  using StringArray = Rml::Vector<Rml::String>;
  types->RegisterDefinition(Rml::Family<StringArray>::Id(),
      Rml::MakeUnique<Rml::ArrayDefinition<StringArray>>(types->GetDefinition<Rml::String>()));
  auto inserted = arrays.emplace(name, std::move(values));
  if (!data_model->Bind(name, &inserted.first->second)) {
    arrays.erase(inserted.first);
    return false;
  }

  return true;
}

RMLUI_CAPI Rml::StructHandle<FakeStruct>* rml_DataModelConstructor_RegisterStruct(Rml::DataModelConstructor* data_model, const char* name) {
  // The struct type is registered globally on the data type register; the name argument
  // only exists for ABI compatibility with the managed binding.
  (void)name;

  if (data_model == nullptr) {
    return nullptr;
  }

  auto it = structHandlePool.find(data_model);
  if (it == structHandlePool.end()) {
    auto* types = data_model->GetDataTypeRegister();
    types->RegisterDefinition(Rml::Family<FakeStruct>::Id(), Rml::MakeUnique<Rml::StructDefinition>());
    auto* definition = static_cast<Rml::StructDefinition*>(types->GetDefinition<FakeStruct>());
    it = structHandlePool.try_emplace(data_model, types, definition).first;
  }

  return &it->second;
}

RMLUI_CAPI bool rml_DataModelConstructor_BindVariable(Rml::DataModelConstructor* data_model, const char* name, Rml::DataVariable* data) {
  return data_model->BindCustomDataVariable(name, *data);
}

Rml::DataEventFunc MakeEventCallbackWrapper(::dataEventFunc func) {
  return [func](Rml::DataModelHandle model_handle, Rml::Event &event, const Rml::VariantList &variants) {
    int size = static_cast<int>(variants.size());
    Rml::Vector<Rml::Variant*> pointer_array(size);

    for (int i = 0; i < size; ++i) {
      pointer_array[i] = const_cast<Rml::Variant*>(&variants[i]);
    }

    func(&model_handle, event, pointer_array.data(), size);
  };
}

RMLUI_CAPI bool rml_DataModelConstructor_BindEventCallback(Rml::DataModelConstructor* data_model, const char* name, ::dataEventFunc func) {
  return data_model->BindEventCallback(name, MakeEventCallbackWrapper(func));
}

RMLUI_CAPI Rml::DataModelHandle* rml_DataModelConstructor_GetModelHandle(Rml::DataModelConstructor* data_model) {
    return new Rml::DataModelHandle(data_model->GetModelHandle());
}

RMLUI_CAPI void rml_DataModelHandle_Free(Rml::DataModelHandle* handle_ptr) {
  delete handle_ptr;
}

RMLUI_CAPI void rml_DataModelConstructor_Free(Rml::DataModelConstructor* instance) {
  stringArrayPool.erase(instance);
  structHandlePool.erase(instance);
  delete instance;
}
