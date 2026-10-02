# Security policy

## Reporting a vulnerability

Report a vulnerability privately, not in a public issue or pull request:
[open a private report](https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload/security/advisories/new).
Only the maintainer sees it. Include the ChromeForTestingAutomatedDownload
version, the .NET version and operating system, the method involved, and
the smallest code or JSON that shows it, with credentials and private data
removed.

A confirmed vulnerability is fixed in a release on nuget.org, and the
advisory is published with it, crediting you unless you ask otherwise.

## Supported versions

Only the latest release on nuget.org receives security fixes. Older
releases are not maintained separately; update when a fix ships.

## Scope

ChromeForTestingAutomatedDownload is a library. It opens no port. What it
reaches, reads, writes and runs:

- **Network.** The model classes and `GoogleChromeLabsEndpointQueries` read
  the eight Chrome for Testing JSON files under
  `https://googlechromelabs.github.io/chrome-for-testing/`.
  `AutomatedDownload.DownloadChromeDriverAsync` also downloads the
  chromedriver ZIP file from the URL that JSON gives (today on
  `storage.googleapis.com`).
- **Files written.** Only `DownloadChromeDriverAsync` writes: the ZIP file,
  to a temporary file of its own that is deleted when it is closed, and the
  chromedriver entry extracted from it, into the caller's folder or, by
  default, the application's base directory, replacing an existing
  chromedriver. An entry whose path is absolute, names a drive or climbs
  with `..` is refused; otherwise only its file name, `chromedriver` or
  `chromedriver.exe`, is used.
- **Files read and processes started.** `LocalVersionChecking` reads the
  file version of `chrome.exe` under Program Files on Windows and runs
  nothing there. On Linux it runs `google-chrome --product-version`, which
  .NET looks for in the application's folder, then the current directory,
  then `PATH`; on macOS it runs
  `/Applications/Google Chrome.app/Contents/MacOS/Google Chrome --version`.
  `DownloadChromeDriverAsync` calls it. Nothing the library downloads is
  run by the library.

The library trusts the Chrome for Testing JSON as it is served over HTTPS.
It requires the download URL to be absolute HTTPS but does not restrict its
host, and Chrome for Testing publishes no checksums to verify a download
against. These count as vulnerabilities:

- a JSON response or download that makes the library write outside the
  download folder or its own temporary ZIP file, or write anything in the
  folder other than the chromedriver extracted from the ZIP file;
- anything that makes the library run a program other than the Chrome
  version commands above, or run what it downloaded;
- a request the library makes to anything other than the endpoints above
  and the download URL the JSON gives.

A wrong URL, a missing version or an exception is not a vulnerability on its
own; report it as an issue.

### Choosing the download folder

Pass a folder of your own to `DownloadChromeDriverAsync` rather than relying
on the default, the application's base directory, so a chromedriver your
application ships is not replaced by surprise.

### Running the version check on Linux

Make sure no file named `google-chrome` sits in your application's folder or
in the current directory, since .NET runs one found there before the one on
`PATH`.

## How the code is checked

Three workflows check every pull request and every push to `main`, and
their gates decide whether a change can merge: **CI passed**,
**Security passed** and **Malware scan passed**. A gate passes only when
every job before it did, and any unexpected finding fails it, whatever its
severity. Security also runs weekly, so new queries, rules and advisories
reach code that has not changed, and Malware scan runs daily, so new
signatures and rules reach files that have not changed.

- **Code:** CodeQL with GitHub's security-extended queries, for C#
  (extracted from a Release build of net8.0, net9.0 and net10.0) and GitHub
  Actions, and Semgrep with the default, C#, security-audit, secrets and
  GitHub Actions rule sets. Semgrep scans the library, the workflows that
  build and publish it, and the scripts in `scripts/security` that judge the
  scans. A `nosemgrep` comment cannot hide a finding. Results go to the
  repository's code scanning.
- **Workflows:** zizmor audits the GitHub Actions workflows; a finding fails
  Security.
- **Dependencies:** `dotnet list package --vulnerable --include-transitive`
  checks the packages `packages.lock.json` resolves, after the same locked
  restore CI builds with, and any known vulnerability fails Security. The
  library has no NuGet dependencies today beyond the .NET runtime.
- **Malware:** ClamAV, with signatures freshclam fetches and verifies on
  every run, and YARA-X, with the YARA Forge rules pinned to a release and
  its SHA-256, scan every file the commit holds and the .nupkg built from it
  with the locked restore, both as the archive and unpacked. On a release
  they scan the very .nupkg that is published. YARA-X runs YARA Forge's full
  rule set. A scan error fails the report as a match does. No workflow
  downloads Chrome or chromedriver, so neither is scanned.
- **OpenSSF Scorecard** rates the repository's security practices on every
  change to `main` and weekly, and the README badge shows the result.
  Its Code-Review and Contributors checks assume more than one
  maintainer, such as a second person approving every change, so a
  single-maintainer project cannot score full marks on them. Its Fuzzing
  check finds no C# fuzzer short of OSS-Fuzz or ClusterFuzzLite, so this
  repository has no fuzz workflow; the JSON it parses goes to
  System.Text.Json.

## Accepted findings

A finding is fixed, or accepted with a written reason in
[.github/security/accepted.toml](https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload/blob/main/.github/security/accepted.toml)
for CodeQL and Semgrep, or
[.github/security/malware-accepted.toml](https://github.com/WilliamSmithEdward/ChromeForTestingAutomatedDownload/blob/main/.github/security/malware-accepted.toml)
for ClamAV and YARA-X. A finding entry matches on the tool, the rule and
the file, and in accepted.toml also the text of the flagged line, so an
edited line needs another review; a notice entry, for a warning a tool
raises about its own scan, matches on the tool, the warning and text the
message contains. An entry that no longer matches fails the report. zizmor
keeps its exceptions in `.github/zizmor.yml` or inline beside the line they
excuse, each with its reason.

The current entries: there are none in accepted.toml or
malware-accepted.toml. zizmor's `self-repository` and `superfluous-actions`
rules are turned off in `.github/zizmor.yml`, each with its reason and when
it comes back.

## Pinning and updates

Everything the workflows run is pinned: actions to full commit SHAs,
runners to named OS releases, scanner images to digests, Python tools to
hash-locked lock files, the library's NuGet packages to
`packages.lock.json`, restored in locked mode, the YARA Forge rules to a
release and its SHA-256, and the YARA-X engine to a release and its
SHA-256. `global.json` sets the .NET SDK's floor at 10.0.400 and lets it
roll forward to a newer feature band, so CI builds with the newest .NET 10
SDK. ClamAV's signatures change too often to pin, so freshclam fetches and
verifies them on every run.

Dependabot proposes updates to the GitHub Actions, the Semgrep and ClamAV
images, the hash-locked files in `.github/requirements`, the NuGet packages
and the .NET SDK in `global.json` once a version is a week old, and at once
for a security advisory. The Update YARA rules workflow proposes new YARA
pins in `.github/security/yara.json` each week. A minor or patch update,
and the YARA pull request, merges itself once CI, Security and Malware scan
pass; a third-party major version waits for review.

## Releases

A pushed `vX.Y.Z` tag builds the .nupkg with the locked restore, checks
that the tag matches the package version (`PackageVersion` in the csproj),
and runs Security and Malware scan on the tagged commit and that package.
Nothing is published unless all of them pass. The package goes to
nuget.org through trusted publishing, so no long-lived API key exists to
leak. The GitHub release, titled with the tag, carries the .nupkg,
`ChromeForTestingAutomatedDownload-<version>-security-report.md` and
`ChromeForTestingAutomatedDownload-<version>-malware-report.md` beside the
scan results they were made from, and the provenance bundle, with the
version's section of `CHANGELOG.md` as its notes. Started by hand, the
Publish workflow is always a dry run and publishes nothing.

### Verifying a download

Releases after 1.1.3 carry a GitHub build provenance attestation for the
.nupkg CI built. Check the copy attached to the GitHub release:

```
gh attestation verify ChromeForTestingAutomatedDownload.<version>.nupkg --owner WilliamSmithEdward
```

The output names the commit and workflow run that built the file. The
signed bundle is also attached to the release as
`ChromeForTestingAutomatedDownload-<version>.sigstore.json`, so the check
works without asking GitHub for it: add
`--bundle ChromeForTestingAutomatedDownload-<version>.sigstore.json`.

nuget.org adds its own repository signature to every package it serves,
which changes the file, so the copy from nuget.org does not match the
attestation. Check that copy's signature with `dotnet nuget verify`.

## Repository settings

<!-- repo-standards:begin security-settings. Copied from WilliamSmithEdward/repo-standards, templates/security/settings-block.md. Change it there; the weekly rescan fails a copy that differs. -->
- `main` accepts changes only through a pull request that passes
  **CI passed**, **Security passed** and **Malware scan passed**. The
  ruleset has no bypass, for the owner either, and refuses force-pushes and
  deleting the branch.
- A `v*` release tag cannot be moved or deleted once pushed, except by a
  repository admin.
- A workflow that uses an action not pinned to a full commit SHA fails to
  run. Workflow tokens are read-only unless a job is granted more for
  itself.
- Secret scanning with push protection, Dependabot alerts and security
  updates, and private vulnerability reporting are on.
<!-- repo-standards:end -->
