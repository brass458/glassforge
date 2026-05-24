# GlassForge dev launcher
# Run from the project root: .\run.ps1
# Optional flags:
#   -Release   build and run Release instead of Debug
#   -NoBuild   skip build, launch existing binary

param(
    [switch]$Release,
    [switch]$NoBuild
)

$Root     = $PSScriptRoot
$Config   = if ($Release) { "Release" } else { "Debug" }

# ── Update this section as the project evolves ──────────────────────────────
$Project  = "src\GlassForge.UI\GlassForge.UI.csproj"
$TFM      = "net8.0-windows"
$Exe      = "src\GlassForge.UI\bin\$Config\$TFM\GlassForge.UI.exe"
# ────────────────────────────────────────────────────────────────────────────

$ExePath = Join-Path $Root $Exe

if (-not $NoBuild) {
    Write-Host "Building ($Config)..." -ForegroundColor Cyan
    dotnet build (Join-Path $Root $Project) -c $Config --nologo -v minimal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Build failed." -ForegroundColor Red
        exit 1
    }
}

if (-not (Test-Path $ExePath)) {
    Write-Host "Executable not found: $ExePath" -ForegroundColor Red
    exit 1
}

Write-Host "Launching $Config build..." -ForegroundColor Green
Start-Process $ExePath
