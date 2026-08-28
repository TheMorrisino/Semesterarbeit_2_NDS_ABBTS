$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

# 1. PostgreSQL lokal starten
Write-Host "==> PostgreSQL starten (docker compose)"
docker compose up -d

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
dotnet Fullstack-Ressourcix.dll
