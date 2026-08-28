$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

# 0. Docker oder Podman?
$ContainerChoice = Read-Host "Container-Tool wählen: [1] Docker  [2] Podman (Enter = 1)"
switch ($ContainerChoice) {
  { $_ -in @("", "1") } { $ContainerTool = "docker" }
  "2" { $ContainerTool = "podman" }
  default {
    Write-Host "Ungültige Eingabe '$ContainerChoice', verwende Docker"
    $ContainerTool = "docker"
  }
}

# 1. Frontend-Abhängigkeiten installieren
Write-Host "==> Frontend-Abhängigkeiten installieren"
Push-Location app
npm install
Pop-Location

# 2. PostgreSQL lokal neu aufsetzen (verwirft ein evtl. vorhandenes Volume,
#    damit ein alter Migrationsstand nicht mit dem aktuellen Code kollidiert)
Write-Host "==> PostgreSQL neu aufsetzen ($ContainerTool compose)"
& $ContainerTool compose down -v
& $ContainerTool compose up -d

# 3. .NET-Tools installieren und Datenbank migrieren
Write-Host "==> .NET-Tools installieren und Datenbank migrieren"
dotnet tool restore
dotnet tool run dotnet-ef database update

# 4. Lokales HTTPS-Zertifikat vertrauen (nur falls noch nicht vorhanden/vertraut)
dotnet dev-certs https --check --trust *> $null
if ($LASTEXITCODE -eq 0) {
  Write-Host "==> Lokales HTTPS-Zertifikat bereits vorhanden und vertraut, überspringe"
} else {
  Write-Host "==> Lokales HTTPS-Zertifikat vertrauen"
  dotnet dev-certs https --trust
  if ($LASTEXITCODE -ne 0) {
    Write-Host "⚠️  Zertifikat wurde erzeugt, konnte aber nicht für alle Clients (z.B. OpenSSL/curl) vertraut werden."
    Write-Host "    Das Backend läuft trotzdem; Details: https://aka.ms/dev-certs-trust"
  }
}

# 5. Backend bauen
Write-Host "==> Backend bauen"
dotnet build

# 6. Backend starten (bindet den Vite-Dev-Server automatisch mit ein)
Write-Host "==> Backend starten"
dotnet run --launch-profile https
