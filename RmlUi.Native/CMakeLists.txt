cmake_minimum_required(VERSION 3.14...3.23)

project(RmlUiNative VERSION 1.0 LANGUAGES CXX)

# Keep build artifacts inside the build tree so that multiple configurations
# (x86/x64, Debug/Release) never overwrite each other. Every artifact type lands
# in the same bin/ directory on every platform for uniform CI handling.
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/bin)

if(WIN32)
    set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded")
endif()

# RmlUi and freetype are built as static libraries and linked into the shared
# RmlUiNative library: position-independent code is required on Linux/macOS.
set(CMAKE_POSITION_INDEPENDENT_CODE ON)

# add external libraries
# FreeType is vendored and linked statically into RmlUiNative. Disable its
# optional system dependencies: on CI runners they may be absent, and on macOS
# the Homebrew builds are arm64-only and cannot satisfy the x86_64 slice of our
# universal binary. zlib is kept (the SDK's libz is universal) for compressed
# fonts; freetype falls back to its bundled zlib when no system zlib exists.
# The options must be seeded as CACHE variables with FORCE: freetype's own
# cmake_minimum_required predates CMake 3.13, so policy CMP0077 is OLD there
# and its option() calls wipe plain normal variables.
set(FT_DISABLE_PNG ON CACHE BOOL "Disable PNG support" FORCE)
set(FT_DISABLE_HARFBUZZ ON CACHE BOOL "Disable HarfBuzz" FORCE)
set(FT_DISABLE_BROTLI ON CACHE BOOL "Disable Brotli" FORCE)
set(FT_DISABLE_BZIP2 ON CACHE BOOL "Disable bzip2" FORCE)
set(BUILD_SHARED_LIBS OFF)
add_subdirectory("ext/freetype")

# Expose the vendored freetype build to RmlUi's find_package(Freetype) call.
if(NOT TARGET Freetype::Freetype)
    add_library(Freetype::Freetype ALIAS freetype)
endif()
set(FREETYPE_INCLUDE_DIR_ft2build "${CMAKE_CURRENT_SOURCE_DIR}/ext/freetype/include" CACHE PATH "Path to ft2build.h" FORCE)
set(FREETYPE_INCLUDE_DIR_freetype2 "${CMAKE_CURRENT_SOURCE_DIR}/ext/freetype/include" CACHE PATH "Path to freetype include dir" FORCE)
if(MSVC)
    if(CMAKE_BUILD_TYPE STREQUAL "Debug")
        set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetyped.lib" CACHE FILEPATH "Path to freetype library" FORCE)
    else()
        set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetype.lib" CACHE FILEPATH "Path to freetype library" FORCE)
    endif()
elseif(WIN32)
    set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/freetype.lib" CACHE FILEPATH "Path to freetype library" FORCE)
else()
    set(FREETYPE_LIBRARY "${CMAKE_BINARY_DIR}/bin/libfreetype.a" CACHE FILEPATH "Path to freetype library" FORCE)
endif()

# The vendored RmlUi submodule is kept pristine at the official tag 6.2. Fork
# fixes live as git patches in patches/ and are applied on top at configure
# time. Each patch is guarded by a reverse-check so repeated configures and
# already-patched trees stay no-ops.
find_package(Git REQUIRED)

function(apply_rmlui_patch patch_file)
    execute_process(
        COMMAND "${GIT_EXECUTABLE}" apply --check --reverse "${patch_file}"
        WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/ext/RmlUi"
        RESULT_VARIABLE patch_already_applied
        OUTPUT_QUIET ERROR_QUIET
    )
    if(NOT patch_already_applied EQUAL 0)
        execute_process(
            COMMAND "${GIT_EXECUTABLE}" apply "${patch_file}"
            WORKING_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/ext/RmlUi"
            RESULT_VARIABLE patch_result
            OUTPUT_VARIABLE patch_output ERROR_VARIABLE patch_error
        )
        if(NOT patch_result EQUAL 0)
            message(FATAL_ERROR "Failed to apply RmlUi patch ${patch_file}:\n${patch_output}${patch_error}")
        endif()
        message(STATUS "Applied RmlUi patch: ${patch_file}")
    endif()
endfunction()

file(GLOB RMLUI_PATCHES "${CMAKE_CURRENT_SOURCE_DIR}/patches/*.patch")
list(SORT RMLUI_PATCHES)
foreach(rmlui_patch IN LISTS RMLUI_PATCHES)
    apply_rmlui_patch("${rmlui_patch}")
endforeach()

add_subdirectory("ext/RmlUi")

add_library(${PROJECT_NAME} SHARED
    "src/Context.cpp"

    "src/Element.cpp"
    "src/ElementDocument.cpp"
    "src/ElementFormControlInput.cpp"
    "src/ElementCustom.cpp"
    "src/ElementInstancer.cpp"

    "src/Variant.cpp"
    "src/VariableDefinition.cpp"
    "src/StructHandle.cpp"
    "src/Dictionary.cpp"
    "src/DataVariable.cpp"
    "src/DataModelHandle.cpp"
    "src/DataModelConstructor.cpp"
    "src/Event.cpp"
    "src/EventInstancer.cpp"
    "src/EventListener.h"
    "src/EventListenerInstancer.cpp"

    "src/Box.cpp"
    "src/RenderInterface.cpp"
    "src/SystemInterface.cpp"
    "src/FileInterface.cpp"
    "src/Plugin.cpp"

    "src/StyleSheetContainer.cpp"
    "src/StyleSheetSpecification.cpp"
    
    "src/XMLAttributes.cpp"
    "src/PropertyDefinition.cpp"
    "src/Property.cpp"
    "src/Rml.cpp"
    "src/Debugger.cpp"
    "src/Util.h"
    "src/Buffer.cpp"
    "include/Buffer.h"
)

include_directories(${PROJECT_NAME}
    PRIVATE include
)

target_compile_definitions(${PROJECT_NAME} PUBLIC "RMLUI_STATIC_LIB")
target_compile_features(${PROJECT_NAME} PRIVATE cxx_std_17)

target_link_libraries(${PROJECT_NAME}
    freetype
    RmlUi::Core
    RmlUi::Debugger
)

# Keep the expected binary names for the .NET runtime on each platform:
#   Windows: RmlUiNative.dll
#   Linux:   libRmlUiNative.so
#   macOS:   RmlUiNative.dylib
if(APPLE)
    set_target_properties(${PROJECT_NAME} PROPERTIES PREFIX "")
endif()

# Warnings as errors for the binding sources only (vendored libraries are built with their own settings).
if(MSVC)
    target_compile_options(${PROJECT_NAME} PRIVATE /W4 /WX)
else()
    target_compile_options(${PROJECT_NAME} PRIVATE -Wall -Wextra -Werror)
endif()
