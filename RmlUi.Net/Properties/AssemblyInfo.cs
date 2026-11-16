using System.Runtime.CompilerServices;

// Allows the smoke test suite to exercise internal P/Invoke wrappers (such as
// Native.DataModelConstructor.BindStringArray) that have no public managed API.
[assembly: InternalsVisibleTo("RmlUi.Net.SmokeTests")]

// Same for the unit tests, which cover the P/Invoke surface directly.
[assembly: InternalsVisibleTo("RmlUi.Net.UnitTests")]
