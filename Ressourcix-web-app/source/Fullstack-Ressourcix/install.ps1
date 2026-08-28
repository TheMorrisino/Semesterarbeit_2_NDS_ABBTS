$ErrorActionPreference = "Stop"

Set-Location $PSScriptRoot

# 1. Frontend-Abhängigkeiten installieren
Write-Host "==> Frontend-Abhängigkeiten installieren"
Push-Location app
npm install
Pop-Location

# 2. PostgreSQL lokal starten
Write-Host "==> PostgreSQL starten (docker compose)"
docker compose up -d

# 3. .NET-Tools installieren und Datenbank migrieren
Write-Host "==> .NET-Tools installieren und Datenbank migrieren"
dotnet tool restore
dotnet tool run dotnet-ef database update

# 4. Lokales HTTPS-Zertifikat vertrauen (einmalig pro Rechner, danach ein No-Op)
Write-Host "==> Lokales HTTPS-Zertifikat vertrauen"
dotnet dev-certs https --trust

# 5. Backend bauen
Write-Host "==> Backend bauen"
dotnet build

# 6. Backend starten (bindet den Vite-Dev-Server automatisch mit ein)
Write-Host "==> Backend starten"
dotnet run --launch-profile https
