# Release checklist

1. Check `VersionPrefix` and `VersionSuffix` in
   [`RmlUi.Net.csproj`](RmlUi.Net/RmlUi.Net.csproj). Use a new version for each
   candidate. Check the release notes and the pinned sources in
   [third-party notices](THIRD-PARTY-NOTICES.md).
2. On the intended commit, run [CI](.github/workflows/ci.yml) and confirm all five
   native build jobs and all five package-consumer jobs pass. Inspect the
   package for one managed assembly, five RID-specific libraries, the README
   and all license files. If a platform did not run, do not publish.
3. In GitHub, configure the `nuget` environment to allow only `main` and require
   approval where available. Set `NUGET_USER` to the package owner's NuGet.org
   username. On NuGet.org create a
   [Trusted Publishing policy](https://learn.microsoft.com/nuget/nuget-org/trusted-publishing)
   for owner `RickyYCheng`, repository `RmlUi.Net`, workflow `release.yml` and
   environment `nuget`. Scope it to `RickyYC.RmlUi.Net`. The first publication
   needs permission to publish a new package, not only new versions.
4. Start [Release](.github/workflows/release.yml) from `main` and enter the exact
   package version. The workflow runs CI again and publishes the same package
   tested by that run. Keep its checksum and artifact with the verification
   results.
5. After NuGet.org indexes the version, confirm that the public feed resolves
   `RickyYC.RmlUi.Net` to the intended package before changing downstream
   consumers. An existing public version is not replaceable; fix it with a
   new version.
