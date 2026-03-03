# VStart Next MVP QA Checklist

## Core Flow

- [ ] Alt+Space toggles launcher visibility.
- [ ] Search returns browser apps for `ch` query.
- [ ] Launching a missing target shows readable error.
- [ ] Pinned items rank above non-pinned in catalog list.

## Stability

- [ ] App starts without crash and stays resident in tray.
- [ ] Config corruption fallback restores defaults.
- [ ] Verify script completes successfully.

## Release Gate

- [ ] `dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal`
- [ ] `dotnet build VStartNext.sln -c Release`
- [ ] `powershell -ExecutionPolicy Bypass -File scripts/verify.ps1`
