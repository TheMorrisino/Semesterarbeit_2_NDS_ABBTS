#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

# 1. Frontend-Abhängigkeiten installieren
echo "==> Frontend-Abhängigkeiten installieren"
(cd app && npm install)

# 2. PostgreSQL lokal starten
echo "==> PostgreSQL starten (docker compose)"
docker compose up -d

# 3. .NET-Tools installieren und Datenbank migrieren
echo "==> .NET-Tools installieren und Datenbank migrieren"
dotnet tool restore
dotnet tool run dotnet-ef database update

# 4. Lokales HTTPS-Zertifikat vertrauen (einmalig pro Rechner, danach ein No-Op)
echo "==> Lokales HTTPS-Zertifikat vertrauen"
dotnet dev-certs https --trust

# 5. Backend bauen
echo "==> Backend bauen"
dotnet build

# 6. Backend starten (bindet den Vite-Dev-Server automatisch mit ein)
echo "==> Backend starten"
dotnet run --launch-profile https
