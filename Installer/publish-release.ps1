#Requires -Version 5.1
<#
.SYNOPSIS
  Publica o actualiza un Release en GitHub con el instalador (.exe) para que «Buscar actualización» pueda descargarlo.

.DESCRIPTION
  Requiere un Personal Access Token (classic) con alcance «repo»:
  https://github.com/settings/tokens

  Uso:
    $env:GH_TOKEN = "ghp_xxxxxxxx"
    .\publish-release.ps1

  Opcional: -Version "1.0.2" si difiere del manifiesto en release/manifest.json
#>
param(
    [string]$Version = "1.0.2",
    [string]$Owner = "tomas125",
    [string]$Repo = "Sistema_Gestion_Turnos_Medicos"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$token = $env:GH_TOKEN
if ([string]::IsNullOrWhiteSpace($token)) { $token = $env:GITHUB_TOKEN }
if ([string]::IsNullOrWhiteSpace($token)) {
    Write-Host @"
No hay GH_TOKEN ni GITHUB_TOKEN.

1) Creá un token en: https://github.com/settings/tokens (classic) con permiso repo.
2) En PowerShell:

   `$env:GH_TOKEN = "ghp_tu_token"
   cd `"$PSScriptRoot`"
   .\publish-release.ps1

"@ -ForegroundColor Yellow
    exit 1
}

$exeName = "MedicalTracker_Setup_$Version.exe"
$exePath = Join-Path $PSScriptRoot "Output\$exeName"
if (-not (Test-Path -LiteralPath $exePath)) {
    Write-Error "No existe el instalador: $exePath`nEjecute antes: .\build-installer.ps1"
}

$tag = "v$Version"
$api = "https://api.github.com/repos/$Owner/$Repo"
$headers = @{
    Authorization = "Bearer $token"
    Accept        = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
}

function Invoke-GitHub {
    param([string]$Method, [string]$Uri, [string]$Body = $null, [string]$ContentType = "application/json")
    $p = @{
        Uri             = $Uri
        Headers         = $headers
        Method          = $Method
        UseBasicParsing = $true
    }
    if ($null -ne $Body) {
        $p.Body = $Body
        $p.ContentType = $ContentType
    }
    return Invoke-RestMethod @p
}

# Obtener release por tag
$release = $null
try {
    $release = Invoke-GitHub -Method GET -Uri "$api/releases/tags/$tag"
} catch {
    $release = $null
}

if (-not $release) {
    $bodyObj = @{
        tag_name         = $tag
        name             = "v$Version"
        body             = "Instalador $exeName. Publicado con publish-release.ps1."
        draft            = $false
        prerelease       = $false
        generate_release_notes = $false
    }
    $release = Invoke-GitHub -Method POST -Uri "$api/releases" -Body ($bodyObj | ConvertTo-Json)
    Write-Host "Release creado: $($release.html_url)" -ForegroundColor Green
} else {
    Write-Host "Release existente: $($release.html_url)" -ForegroundColor Cyan
}

# Quitar adjunto con el mismo nombre si ya existía
foreach ($a in $release.assets) {
    if ($a.name -eq $exeName) {
        Write-Host "Eliminando asset anterior: $exeName"
        Invoke-GitHub -Method DELETE -Uri "https://api.github.com/repos/$Owner/$Repo/releases/assets/$($a.id)"
    }
}

# Refrescar release por si cambió
$release = Invoke-GitHub -Method GET -Uri "$api/releases/tags/$tag"
$uploadBase = $release.upload_url -replace '\{\?name,label\}', ''
$uploadUri = "$uploadBase" + "?name=$([uri]::EscapeDataString($exeName))"

Write-Host "Subiendo $exeName ($([math]::Round((Get-Item $exePath).Length / 1MB, 1)) MB)…"

curl.exe -sS -X POST `
    -H "Authorization: Bearer $token" `
    -H "Accept: application/vnd.github+json" `
    -H "X-GitHub-Api-Version: 2022-11-28" `
    -H "Content-Type: application/octet-stream" `
    --data-binary "@$exePath" `
    $uploadUri

if ($LASTEXITCODE -ne 0) {
    Write-Error "Falló la subida del adjunto (curl exit $LASTEXITCODE)"
}

Write-Host "`nListo. URL de descarga (latest):" -ForegroundColor Green
Write-Host "https://github.com/$Owner/$Repo/releases/latest/download/$exeName"
Write-Host "`nProbá en el navegador o con Buscar actualización en la app." -ForegroundColor Gray
