# Native library

See [Build and test](../README.md#build-and-test) for commands and dependencies.
Use a separate CMake build directory for each architecture. The RmlUi patches
apply to the copy in the build directory; the submodule checkout is unchanged.

The library exposes the C ABI used by RmlUi.Net. It is not a standalone C++
distribution of RmlUi. When an interop signature changes, update the matching
managed declaration and the ABI version on both sides. Export parity detects
missing symbols, not incorrect parameter types. Run the runtime tests as well.
