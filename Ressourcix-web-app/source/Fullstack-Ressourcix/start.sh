#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

# 1. PostgreSQL lokal starten
echo "==> PostgreSQL starten (docker compose)"
docker compose up -d

# 2. Produktions-Build (installiert Frontend-Abhängigkeiten, baut Frontend + Backend
#    in Release-Konfiguration, kopiert alles nach bin/Release/net10.0/publish)
echo "==> Produktions-Build (dotnet publish -c Release)"
dotnet publish -c Release

# 3. Publish-Output starten (Production-Umgebung + Connection String von aussen,
echo "==> Publish-Output starten"
cd bin/Release/net10.0/publish
ConnectionStrings__AppDb="Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw" \
  dotnet Fullstack-Ressourcix.dll
