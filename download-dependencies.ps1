# Downloads the Thunderstore packages the project compiles against into deps/,
# at the versions pinned in manifest.json. deps/ is gitignored and CI fetches it
# itself, so run this after cloning or whenever one of these pins is bumped.
# Packages already extracted at the pinned version are skipped.
#
#   .\download-dependencies.ps1

$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue' # Invoke-WebRequest is very slow with the progress bar
[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12

$packages = @(
  'BepInEx-BepInExPack_GTFO',
  'GTFOModding-GTFO_API',
  'Amorously-AmorLib',
  'Inas07-LocalProgression',
  'GTFOModding-InjectLib'
)

$dependencies = (Get-Content (Join-Path $PSScriptRoot 'manifest.json') -Raw | ConvertFrom-Json).dependencies
$deps = Join-Path $PSScriptRoot 'deps'
New-Item -ItemType Directory -Force $deps | Out-Null

foreach ($name in $packages) {
  $dependency = $dependencies | Where-Object { $_ -match "^$([regex]::Escape($name))-[0-9.]+$" }
  if (-not $dependency) { throw "$name is not a dependency in manifest.json" }

  $version = $dependency.Substring($name.Length + 1)
  $zip = Join-Path $deps "$name.zip"
  $dir = Join-Path $deps $name
  $installedManifest = Join-Path $dir 'manifest.json'

  if ((Test-Path $installedManifest) -and
      (Get-Content $installedManifest -Raw | ConvertFrom-Json).version_number -eq $version) {
    Write-Host "   $dependency (up to date)"
    continue
  }

  Write-Host "-> $dependency"

  Invoke-WebRequest -UseBasicParsing "https://gcdn.thunderstore.io/live/repository/packages/$dependency.zip" -OutFile $zip
  if (Test-Path $dir) { Remove-Item -Recurse -Force $dir }
  Expand-Archive $zip $dir
}
