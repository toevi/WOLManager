# build-installer.ps1 - builds an UNSIGNED WOLManager-Setup-*.exe.
#
# For anyone who wants to build their own installer from source. It:
#   1. runs dotnet publish (Release, win-x64, self-contained, single file),
#   2. compiles the installer with Inno Setup (ISCC) -> Installer\Output\.
#
# The result is UNSIGNED (Authenticode). Official signed releases are produced by
# the author with a separate, private script holding the certificate - not included here.
#
# Requirements: .NET SDK 9, Inno Setup 6 (https://jrsoftware.org/isdl.php).
# Usage:  pwsh .\build-installer.ps1

$ErrorActionPreference = 'Stop'

$Root       = $PSScriptRoot
$Csproj     = Join-Path $Root 'WOLManager.csproj'
$PublishDir = Join-Path $Root 'publish'
$IssFile    = Join-Path $Root 'Installer\WOLManager.iss'

function Find-ISCC {
    $candidates = @(
        "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe",
        "${env:ProgramFiles}\Inno Setup 6\ISCC.exe"
    )
    foreach ($c in $candidates) { if (Test-Path $c) { return $c } }
    $cmd = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    if ($cmd) { return $cmd.Source }
    throw 'ISCC.exe not found - install Inno Setup 6: https://jrsoftware.org/isdl.php'
}

Write-Host '=== dotnet publish (Release) ===' -ForegroundColor Cyan
dotnet publish $Csproj -c Release -r win-x64 --self-contained true `
    -p:PublishSingleFile=true -o $PublishDir
if ($LASTEXITCODE -ne 0) { throw "dotnet publish returned $LASTEXITCODE" }

$ISCC = Find-ISCC
Write-Host "`n=== ISCC (compile, unsigned) ===" -ForegroundColor Cyan
Write-Host "ISCC: $ISCC"
& $ISCC $IssFile
if ($LASTEXITCODE -ne 0) { throw "ISCC returned $LASTEXITCODE" }

Write-Host "`nDone (unsigned): $(Join-Path $Root 'Installer\Output')" -ForegroundColor Green
