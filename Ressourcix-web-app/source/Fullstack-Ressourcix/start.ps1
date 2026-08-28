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

# 1. PostgreSQL lokal starten
Write-Host "==> PostgreSQL starten ($ContainerTool compose)"
& $ContainerTool compose up -d

# 2. Produktions-Build (installiert Frontend-Abhängigkeiten, baut Frontend + Backend
#    in Release-Konfiguration, kopiert alles nach bin/Release/net10.0/publish)
Write-Host "==> Produktions-Build (dotnet publish -c Release)"
dotnet publish -c Release

# 3. Publish-Output starten (Production-Umgebung + Connection String von aussen,
#    siehe README Abschnitt "Publish-Output starten".
#    ACHTUNG: ASPNETCORE_ENVIRONMENT=Development NICHT verwenden - das aktiviert
#    zusätzlich zur appsettings.Development.json auch den Vite-Dev-Server-Proxy
#    von Mumrich.SpaDevMiddleware, der dann vergeblich auf 127.0.0.1:3000 wartet,
#    weil im Publish-Output kein Dev-Server läuft.)
Write-Host "==> Publish-Output starten"
Set-Location bin/Release/net10.0/publish
$env:ConnectionStrings__AppDb = "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw"
# Kestrels Default-Port 5000 liegt unter Windows oft in einem von Hyper-V/WSL2/Docker Desktop
# reservierten Portbereich ("SocketException 10013: Zugriff verweigert"). Deshalb explizit auf
# den im Projekt etablierten HTTP-Port binden (siehe README, launchSettings.json).
$env:ASPNETCORE_URLS = "http://localhost:5167"
dotnet Fullstack-Ressourcix.dll
