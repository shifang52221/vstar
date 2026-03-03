$ErrorActionPreference = "Stop"

dotnet test tests/VStartNext.Core.Tests/VStartNext.Core.Tests.csproj -v minimal
dotnet build VStartNext.sln -c Release

Write-Host "Verification passed"
