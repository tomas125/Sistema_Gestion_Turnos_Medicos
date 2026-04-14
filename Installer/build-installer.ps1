#Requires -Version 5.1
<#
.SYNOPSIS
  Publica MedicalTracker (win-x64, self-contained) y genera el instalador con Inno Setup 6.

.DESCRIPTION
  Requiere: .NET SDK 8, Inno Setup 6 (ISCC.exe).
  Salida: Installer\Output\MedicalTracker_Setup_<version>.exe
#>
Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$here = $PSScriptRoot
$repoRoot = Split-Path -Parent $here
$csproj = Join-Path $repoRoot "MedicalTracker\MedicalTracker.csproj"
$publishDir = Join-Path $here "publish"
$iss = Join-Path $here "MedicalTracker.iss"

if (-not (Test-Path $csproj)) {
    Write-Error "No se encontró el proyecto: $csproj"
}

Write-Host "Publicando aplicación (Release, win-x64, self-contained)..." -ForegroundColor Cyan
dotnet publish $csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $publishDir `
    -p:PublishReadyToRun=true

if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$exe = Join-Path $publishDir "MedicalTracker.exe"
if (-not (Test-Path $exe)) {
    Write-Error "No se generó MedicalTracker.exe en $publishDir"
}

$candidates = @(
    Join-Path ${env:ProgramFiles(x86)} "Inno Setup 6\ISCC.exe"
    Join-Path $env:ProgramFiles "Inno Setup 6\ISCC.exe"
)
$iscc = $candidates | Where-Object { Test-Path $_ } | Select-Object -First 1

if (-not $iscc) {
    Write-Error @"
No se encontró Inno Setup 6 (ISCC.exe).
Instale Inno Setup desde https://jrsoftware.org/isdl.php y vuelva a ejecutar este script,
o compile manualmente: ISCC.exe `"$iss`"
"@
}

$icon = Join-Path $repoRoot "IMAGENES\logo.ico"
if (-not (Test-Path $icon)) {
    Write-Error "Falta el icono para el instalador: $icon"
}

Write-Host "Compilando instalador con: $iscc" -ForegroundColor Cyan
& $iscc $iss
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

$outDir = Join-Path $here "Output"
Write-Host "Listo. Revise: $outDir" -ForegroundColor Green
