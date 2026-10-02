# Notes for agents

<!-- repo-standards:begin. Copied from WilliamSmithEdward/repo-standards, templates/agents/AGENTS-block.md. Change it there; the weekly rescan fails a copy that differs. -->
## Releases, CI and security

These rules are the same in every WilliamSmithEdward repository.

- **How a release happens here:** pushing a `vX.Y.Z` tag runs Publish, which builds the release files in CI and creates the GitHub release with them, their signed provenance and the security reports. Any other step, such as a marketplace upload, is described elsewhere in this file.
- **Starting a workflow by hand never releases anything.** Publish and every
  release report are dry runs when started with `gh workflow run` or the Run
  workflow button. They build, scan and assemble the release files exactly
  as a release would, and upload them as the `release-preview` artifact
  instead. Run one after changing anything on the release path:
  `gh workflow run <file> --ref main`, then
  `gh run download <run-id> -n release-preview`.
- **Do not create, publish, edit or delete a release or a `v*` tag** unless
  the owner asks for it. A `v*` tag cannot be moved or deleted once pushed.
- **Every change to `main` goes through a pull request** that passes CI
  passed, Security passed and Malware scan passed. No one can push to `main`
  directly or skip the checks, admins included. Push a branch, open a pull
  request, and let it merge itself: `gh pr merge --auto --squash <number>`.
- **Pins.** Actions by full commit SHA with the version as a comment. Images
  by digest, in `.github/security/<tool>/Dockerfile`. Python tools from the
  hash-locked `.github/requirements/<purpose>.txt`, compiled from the `.in`
  beside it with
  `uv pip compile <purpose>.in --universal --generate-hashes --python-version 3.12 -o <purpose>.txt`.
  Runners are named releases, never `-latest`.
- **Updates merge themselves.** Dependabot and the Update YARA rules workflow
  open pull requests that merge once the three checks pass, except a
  third-party major version, which waits for the owner. Leave them alone
  unless asked.
- **A scanner finding is fixed or accepted with a written reason** in the
  repository's accepted list. Never silence a scanner without one.
<!-- repo-standards:end -->

## This repository

ChromeForTestingAutomatedDownload is a .NET library that reads Google's
Chrome for Testing JSON endpoints and downloads chromedriver, published to
nuget.org as `ChromeForTestingAutomatedDownload` for net8.0, net9.0 and
net10.0. What an agent working here must not break:

- **The release path.** A release starts from a `vX.Y.Z` tag that matches
  `PackageVersion` in
  `ChromeDriverAutomatedDownload/ChromeForTestingAutomatedDownload.csproj`
  (keep `AssemblyVersion` the same); Publish refuses any other. Its notes are
  the version's section of `CHANGELOG.md` (`## [X.Y.Z] - date`), written
  before the tag is pushed; without one the release fails. The package goes
  to nuget.org through trusted publishing: nuget.org's policy is bound to
  `publish.yml` and the `nuget` environment, so both keep their names, and no
  API key is stored anywhere.
- **Two READMEs that say the same things.** `README.md` is the GitHub page
  and `ChromeDriverAutomatedDownload/nugetREADME.md` is packed as the
  nuget.org readme. Change both in the same pull request. They differ only in
  the badge block, which `nugetREADME.md` leaves out because nuget.org does
  not render images from api.scorecard.dev. Links in both are absolute,
  since nuget.org does not resolve relative ones.
- **The lock file.** Restores run with `--locked-mode` against
  `ChromeDriverAutomatedDownload/packages.lock.json`. A new or changed
  package reference is restored without it once, and the updated lock file
  committed with it.
- **Three target frameworks.** The library targets net8.0, net9.0 and
  net10.0, and CI checks the package holds each one's dll and XML docs.
  Change the list only on the owner's decision, and update `ci.yml`,
  `publish.yml` and the READMEs with it.
- **Tests.** `ChromeForTestingAutomatedDownload.Tests` is an xUnit v3
  project run by Microsoft.Testing.Platform (`global.json` opts `dotnet test`
  in), on net8.0, net9.0 and net10.0. Tests never touch the network: every
  request goes through `FakeHttpHandler`, which answers from `Fixtures/` and
  refuses any other URL, and ZIP files are built in the test. They write only
  into a `TempFolder` under the system temp folder. CI runs them with
  `--fail-skips on`. A fix comes with a test that fails without it. CI also
  builds with no warnings, so every public member needs an XML doc comment.
  Compile a changed README sample against the library before committing it.
- **Nothing downloads into or runs from the working tree.** The library
  downloads chromedriver and runs the local Chrome to read its version. No
  test, sample run, workflow or script in this repository may download
  Chrome, chromedriver or chrome-headless-shell into the working tree, or
  execute anything downloaded. Run a sample that downloads only in a scratch
  folder outside the repository, and samples that read the JSON endpoints
  only, where you can.
