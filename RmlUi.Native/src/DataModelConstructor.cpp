#include "RmlNative.h"
#include "RmlUi/Core.h"
#include "RmlUi/Core/DataStructHandle.h"
#include <string>
#include <sstream>
#include <unordered_map>

typedef void (*dataEventFunc)(Rml::DataModelHandle* model_handle, Rml::Event& evt, Rml::Variant** variants, int num_variants);

static std::unordered_map<Rml::DataModelHandle*, Rml::DataModelHandle> dataModelHandlePool;

// Storage for string arrays bound through rml_DataModelConstructor_BindStringArray.
// RmlUi binds data variables by pointer, so the backing vector must outlive the binding.
static std::unordered_map<std::string, Rml::Vector<Rml::String>> stringArrayPool;

// Storage for the fake struct handles returned by rml_DataModelConstructor_RegisterStruct.
// A struct type can only be registered once per data type register.
static std::unordered_map<const void*, Rml::StructHandle<FakeStruct>> structHandlePool;

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

static std::string GenerateKey(Rml::DataModelConstructor* data_model, const char* name) {
    std::stringstream key;
    key << data_model << "_" << name;
    return key.str();
}

RMLUI_CAPI bool rml_DataModelConstructor_BindStringArray(Rml::DataModelConstructor* data_model, const char* name, const char** data, int count) {
  if (data_model == nullptr || name == nullptr || (data == nullptr && count > 0) || count < 0) {
    return false;
  }

  const std::string key = GenerateKey(data_model, name);
  Rml::Vector<Rml::String>& values = stringArrayPool[key];
  values.clear();
  values.reserve(static_cast<size_t>(count));
  for (int i = 0; i < count; ++i) {
    values.emplace_back(data[i] != nullptr ? data[i] : "");
  }

  if (!data_model->RegisterArray<Rml::Vector<Rml::String>>()) {
    stringArrayPool.erase(key);
    return false;
  }

  if (!data_model->Bind(name, &values)) {
    stringArrayPool.erase(key);
    return false;
  }

  return true;
}

RMLUI_CAPI bool rml_DataModelConstructor_UnbindStringArray(Rml::DataModelConstructor* data_model, const char* name) {
  if (data_model == nullptr || name == nullptr) {
    return false;
  }

  // RmlUi has no API to unbind a variable from a data model; erasing our pool releases
  // the backing storage once no longer referenced by the managed binding. The data model
  // itself keeps the binding until it is destroyed.
  return stringArrayPool.erase(GenerateKey(data_model, name)) > 0;
}

RMLUI_CAPI Rml::StructHandle<FakeStruct>* rml_DataModelConstructor_RegisterStruct(Rml::DataModelConstructor* data_model, const char* name) {
  // The struct type is registered globally on the data type register; the name argument
  // only exists for ABI compatibility with the managed binding.
  (void)name;

  if (data_model == nullptr) {
    return nullptr;
  }

  const void* type_register = data_model->GetDataTypeRegister();
  if (type_register == nullptr) {
    return nullptr;
  }

  auto it = structHandlePool.find(type_register);
  if (it == structHandlePool.end()) {
    Rml::StructHandle<FakeStruct> handle = data_model->RegisterStruct<FakeStruct>();
    if (!handle) {
      return nullptr;
    }
    it = structHandlePool.emplace(type_register, handle).first;
  }

  return &it->second;
}

RMLUI_CAPI bool rml_DataModelConstructor_BindVariable(Rml::DataModelConstructor* data_model, const char* name, Rml::DataVariable* data) {
  return data_model->BindCustomDataVariable(name, *data);
}

Rml::DataEventFunc MakeEventCallbackWrapper(::dataEventFunc func) {
  return [func](Rml::DataModelHandle model_handle, Rml::Event &event, const Rml::VariantList &variants) {
    int size = static_cast<int>(variants.size());
    Rml::Variant** pointer_array = new Rml::Variant*[size];

    for (int i = 0; i < size; ++i) {
      pointer_array[i] = const_cast<Rml::Variant*>(&variants[i]);
    }

    func(&model_handle, event, pointer_array, size);

    delete[] pointer_array;
  };
}

RMLUI_CAPI bool rml_DataModelConstructor_BindEventCallback(Rml::DataModelConstructor* data_model, const char* name, ::dataEventFunc func) {
  return data_model->BindEventCallback(name, MakeEventCallbackWrapper(func));
}

RMLUI_CAPI Rml::DataModelHandle* rml_DataModelConstructor_GetModelHandle(Rml::DataModelConstructor* data_model) {
    Rml::DataModelHandle handle = data_model->GetModelHandle();
    dataModelHandlePool[&handle] = handle;
    return &dataModelHandlePool[&handle];
}

RMLUI_CAPI void rml_DataModelHandle_Free(Rml::DataModelHandle* handle_ptr) {
  auto it = dataModelHandlePool.find(handle_ptr);
  if (it != dataModelHandlePool.end()) {
    dataModelHandlePool.erase(it);
  }
}
