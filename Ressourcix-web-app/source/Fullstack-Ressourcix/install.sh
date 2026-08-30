#!/usr/bin/env bash
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"

# 0. Docker oder Podman?
read -rp "Container-Tool wählen: [1] Docker  [2] Podman (Enter = 1): " CONTAINER_CHOICE
case "$CONTAINER_CHOICE" in
  ""|1) CONTAINER_TOOL=docker ;;
  2) CONTAINER_TOOL=podman ;;
  *)
    echo "Ungültige Eingabe '$CONTAINER_CHOICE', verwende Docker" >&2
    CONTAINER_TOOL=docker
    ;;
esac

# 1. Frontend-Abhängigkeiten installieren
echo "==> Frontend-Abhängigkeiten installieren"
(cd app && npm install)

# 2. PostgreSQL lokal neu aufsetzen (verwirft ein evtl. vorhandenes Volume,
#    damit ein alter Migrationsstand nicht mit dem aktuellen Code kollidiert)
echo "==> PostgreSQL neu aufsetzen ($CONTAINER_TOOL compose)"
$CONTAINER_TOOL compose down -v
$CONTAINER_TOOL compose up -d

# 3. .NET-Tools installieren und Datenbank migrieren
echo "==> .NET-Tools installieren und Datenbank migrieren"
dotnet tool restore
dotnet tool run dotnet-ef database update

# 4. Lokales HTTPS-Zertifikat vertrauen (nur falls noch nicht vorhanden/vertraut)
if dotnet dev-certs https --check --trust > /dev/null 2>&1; then
  echo "==> Lokales HTTPS-Zertifikat bereits vorhanden und vertraut, überspringe"
else
  echo "==> Lokales HTTPS-Zertifikat vertrauen"
  if ! dotnet dev-certs https --trust; then
    echo "⚠️  Zertifikat wurde erzeugt, konnte aber nicht für alle Clients (z.B. OpenSSL/curl) vertraut werden." >&2
    echo "    Das Backend läuft trotzdem; für OpenSSL-Trust ggf. SSL_CERT_DIR ergänzen, z.B.:" >&2
    echo "    export SSL_CERT_DIR=\"\$HOME/.aspnet/dev-certs/trust:/etc/ssl/certs\"" >&2
    echo "    Details: https://aka.ms/dev-certs-trust" >&2
  fi
fi

# 5. Backend bauen
echo "==> Backend bauen"
dotnet build

# 6. Backend starten (bindet den Vite-Dev-Server automatisch mit ein)
echo "==> Backend starten"
dotnet run --launch-profile https
