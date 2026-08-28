# Ressourcix

## 📚 Inhalt

- 🧭 [Überblick](#-überblick)
- 🧱 [Architektur und Projektstruktur](#-architektur-und-projektstruktur)
- 🔐 [Rollen und Berechtigungen](#-rollen-und-berechtigungen)
- 🧰 [Technologie-Stack](#-technologie-stack)
- ✅ [Voraussetzungen](#-voraussetzungen)
- ▶️ [Setup und Start](#️-setup-und-start)
- 📦 [Produktions-Build](#-produktions-build)
- 🛡️ [Sicherheit](#️-sicherheit)
- 🌍 [Mehrsprachigkeit](#-mehrsprachigkeit)
- 🧪 [Tests](#-tests)
- 📄 [Weiterführende Dokumente](#-weiterführende-dokumente)

## 🧭 Überblick

Ressourcix ist eine Abwesenheits- und Ferienverwaltung für KMU (Semesterarbeit 2, NDS ABBTS). Mitarbeitende erfassen Ferienanträge im Kalender, Admins/Planer:innen verwalten Mitarbeitende, genehmigen oder lehnen Anträge ab, und jede relevante Änderung wird revisionssicher im Audit-Log protokolliert.

Kernfunktionen:

- **Kalender**: Mitarbeiter als Zeilen, Tage als Spalten; Ferienanträge per Klick erfassen/bearbeiten, Überschneidungs-Heatmap, responsives Layout (zeigt auf Handy-Breite nur noch Kürzel statt Vollname, damit mehr Tagesspalten sichtbar bleiben).
- **Dashboard**: eigener Ferienstatus (Anspruch/Bezogen/Geplant/Verbleibend), Teamübersicht der aktuellen Woche, Überschneidungsdiagramm, letzte Anträge/Aktivitäten.
- **Mitarbeiterverwaltung** (nur Admin): Mitarbeitende anlegen/bearbeiten/deaktivieren/löschen. Das Berechtigungslevel wird ausschliesslich aus der gewählten Rolle abgeleitet, nicht direkt editierbar.
- **Genehmigungen** (nur Admin): offene Anträge einsehen, genehmigen/ablehnen.
- **Audit-Log**: jede Mutation (Anträge, Mitarbeitende) wird serverseitig als Nebeneffekt der jeweiligen Aktion protokolliert — nicht über einen frei aufrufbaren Endpoint, damit Einträge nicht gefälscht oder unterdrückt werden können.

## 🧱 Architektur und Projektstruktur

Backend (ASP.NET Core Minimal API) und Frontend (Vue 3 SPA) leben in diesem Ordner; im Dev-Betrieb bindet `Mumrich.SpaDevMiddleware` den Vite-Dev-Server ein, in Produktion wird der gebaute Frontend-Output mitgeliefert.

Bewusst **keine** volle Clean-Architecture-Schichtung (Use-Cases/Entities/Repositories) — für ein CRUD-Projekt dieser Grösse wäre das Overengineering. Stattdessen eine schlanke 3-Schichten-Struktur `Endpoints → Services → Data`, mit `Program.cs` nur noch als Composition Root:

```
Fullstack-Ressourcix/
├─ Program.cs                 # Composition Root: DI-Registrierung, Middleware-Pipeline, Endpoint-Mapping
├─ AppSettings.cs             # Konfigurationsbindung (SPA-Middleware)
├─ GlobalExceptionHandler.cs  # zentrales Fehlerhandling (IExceptionHandler)
├─ Endpoints/                 # Minimal-API-Endpoints als IEndpointRouteBuilder-Extensions (Auth/Employee/Request/AuditLog)
├─ Dtos/                      # Request-/Response-DTOs (Wire-Format, camelCase)
├─ Auth/AuthHelpers.cs        # Claims-Helper (BuildPrincipal, IsAdmin, CanActOnRequest, ...)
├─ Models/                    # Employee, AbsenceRequest, AuditLogEntry (+ zugehörige Enums), EmployeeRoles
├─ Services/                  # EmployeeStore, RequestsStore, AuthStore, AuditLogStore, AuditSummaryBuilder (EF-Core-Datenzugriff)
├─ Data/AppDbContext.cs       # DbContext + Seed-Daten (4 Beispiel-Mitarbeitende)
├─ Migrations/                # EF-Core-Migrationen
├─ appsettings*.json          # Konfiguration (Connection String, Default-Passwort)
├─ docker-compose.yml         # lokales PostgreSQL für die Entwicklung
└─ app/                       # Vue-3-Frontend, siehe app/README.md
```

## 🔐 Rollen und Berechtigungen

Es gibt zwei Berechtigungsstufen, serverseitig über eine ASP.NET-Core-Authorization-Policy erzwungen (nicht nur im Frontend versteckt):

| | Mitarbeiter (Level 1) | Admin / Planer (Level 5) |
|---|---|---|
| Kalender | Nur **eigene** Anträge erfassen/bearbeiten/löschen; Status **nicht** ändern (Feld im Dialog gesperrt) | Anträge für jeden Mitarbeiter, inkl. Statusänderung |
| Genehmigungen | Keine Einsicht (View + Nav-Link ausgeblendet, Route-Guard) | Volle Einsicht, genehmigen/ablehnen |
| Abwesenheiten | Nur eigene Einträge | Nur eigene Einträge |
| Mitarbeiterverwaltung | Einsicht, keine Schreibaktionen (Buttons ausgeblendet) | Volle Verwaltung |
| Audit-Log | Einsicht | Einsicht |

Das Berechtigungslevel eines Mitarbeitenden wird **ausschliesslich** aus der Rolle abgeleitet — über eine explizite Zuordnung in `Models/EmployeeRoles.cs` (`Mitarbeiter` → 1, `Planer/Leitung` → 5). Eine unbekannte Rolle wird abgelehnt (`400 Bad Request`) statt still auf die höchste Berechtigungsstufe zu fallen. Weder Formular noch API erlauben es, das Level direkt zu setzen.

## 🧰 Technologie-Stack

**Backend**: ASP.NET Core (net10.0) Minimal API, Entity Framework Core mit Npgsql (PostgreSQL), Cookie-Authentication (`HttpOnly`, `Secure`, `SameSite=Strict`), `Microsoft.AspNetCore.RateLimiting` fürs Login, `Mumrich.SpaDevMiddleware` zur Frontend-Anbindung im Dev-Betrieb.

**Frontend** (`app/`): Vue 3 + Vite, Vuetify, Pinia, Vue Router, vue-i18n (DE/EN/FR), UnoCSS. Details siehe [app/README.md](app/README.md).

## ✅ Voraussetzungen

- ![.NET](https://img.shields.io/badge/.NET%20SDK-512BD4?logo=dotnet&logoColor=white) .NET SDK kompatibel mit `net10.0`
- ![Node.js](https://img.shields.io/badge/Node.js-5FA04E?logo=node.js&logoColor=white) Node.js (aktuelles LTS) + npm
- ![Docker](https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white) Docker (für lokales PostgreSQL via Docker Compose)

```bash
dotnet --version
node --version
npm --version
```

## ▶️ Setup und Start

```bash
# 1. Frontend-Abhängigkeiten installieren
cd app
npm install
cd ..

# 2. PostgreSQL lokal starten (statt docker compose geht z.B. auch podman compose)
docker compose up -d

# 3. .NET-Tools installieren und Datenbank migrieren
dotnet tool restore
dotnet tool run dotnet-ef database update

# 4. Lokales HTTPS-Zertifikat vertrauen (einmalig pro Rechner)
dotnet dev-certs https --trust

# 5. Backend starten (bindet den Vite-Dev-Server automatisch mit ein)
dotnet run --launch-profile https
```

App öffnen: **https://localhost:7057** (HTTP-Fallback: http://localhost:5167).

**Default-Logins** (Passwort muss beim ersten Login geändert werden, Regel: ≥8 Zeichen, Gross-/Kleinbuchstaben, Zahl, Sonderzeichen):

| Benutzername | Rolle | Passwort |
|---|---|---|
| `pedro.santos` | Admin/Planer | `Ressourcix#2026` |
| `morris.meier`, `lena.brunner`, `tiago.desousa` | Mitarbeiter | `Ressourcix#2026` |

Alternativ mit C# Dev Kit in VS Code: Ordner `source/Fullstack-Ressourcix` öffnen, Run-and-Debug-Profil `http` oder `https` wählen, `F5`.

Alle Schritte 1–5 stehen auch gebündelt in [`install.sh`](install.sh) (Bash) bzw. [`install.ps1`](install.ps1) (PowerShell):

**Bash**
```bash
chmod +x install.sh
./install.sh
```

**PowerShell**
```powershell
.\install.ps1
```

> ⚠️ **Datenbank-Volume vs. Migrationsstand**: `docker compose up -d` behält das Postgres-Volume über Neustarts hinweg bei (Named Volume, siehe `docker-compose.yml`). Nach einem Pull mit neuen/geänderten Migrationen kann das lokale Volume dadurch einen älteren Schema-Stand enthalten als der Code erwartet. Anzeichen dafür:
> - `dotnet-ef database update` schlägt mit `relation "..." already exists` fehl (die alten Tabellen existieren schon, `__EFMigrationsHistory` kennt aber nicht die aktuelle Migrations-ID).
> - Login/andere Requests schlagen zur Laufzeit mit `column e."Id" does not exist` (bzw. `Hint: ... e.id`) fehl — die im Volume liegenden Spalten (z.B. camelCase) passen nicht mehr zum aktuellen Modell (PascalCase).
>
> Fix: Volume komplett verwerfen und die aktuelle Migration sauber neu anwenden (seedet die Testdaten neu):
>
> ```bash
> docker compose down -v
> docker compose up -d
> dotnet tool run dotnet-ef database update
> ```

## 📦 Produktions-Build

Ein einziger Befehl installiert Abhängigkeiten und kompiliert Front- und Backend für den Produktivbetrieb (Release-Konfiguration, kein Debug, Frontend minimiert):

```bash
dotnet publish -c Release
```

Das MSBuild-`<SpaRoot>`-Element in `Fullstack-Ressourcix.csproj` sorgt dafür, dass dieser eine Befehl automatisch:

1. `npm install` im Ordner `app/` ausführt (Frontend-Abhängigkeiten),
2. `npm run build` ausführt (Type-Check + minimierter Vite-Production-Build, Output in `app/dist/`),
3. das Backend in der **Release**-Konfiguration kompiliert (Optimierungen an, `DEBUG`-Symbol aus),
4. den fertigen Frontend-Build sowie alle Backend-Artefakte gemeinsam nach `bin/Release/net10.0/publish/` kopiert — von dort aus ist die App direkt lauffähig (`dotnet Fullstack-Ressourcix.dll`).

`.NET`- und `npm`-Toolchain müssen wie in [Voraussetzungen](#-voraussetzungen) beschrieben installiert sein; eine laufende PostgreSQL-Instanz wird für den Build selbst nicht benötigt, nur zur Laufzeit.

### Publish-Output starten

`appsettings.Development.json` (und damit der lokale Connection String / das Default-Passwort) wird **nur** geladen, wenn `ASPNETCORE_ENVIRONMENT=Development` gesetzt ist. Startet man `dotnet Fullstack-Ressourcix.dll` direkt ohne diese Variable, läuft die App als **Production** und `appsettings.json` allein enthält keinen Connection String — die App bricht dann beim Start mit `ConnectionStrings:AppDb ist nicht konfiguriert.` ab. Das ist beabsichtigt, damit Dev-Zugangsdaten nicht automatisch als Produktivkonfiguration greifen.

Für einen echten Produktionsstart die Konfiguration von aussen mitgeben, z.B. per Umgebungsvariable (`__` statt `:` für verschachtelte Keys):

 **Bash**
```bash
cd bin/Release/net10.0/publish
ConnectionStrings__AppDb="Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw" \
  dotnet Fullstack-Ressourcix.dll
```

**Powershell** 
```powershell
cd bin/Release/net10.0/publish
$env:ConnectionStrings__AppDb = "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw"
dotnet Fullstack-Ressourcix.dll
```
 

(PostgreSQL muss dafür laufen, siehe [Setup und Start](#️-setup-und-start)).

Datenbank starten, Produktions-Build und Start des Publish-Outputs (mit lokalem Connection String) sind gebündelt in [`start.sh`](start.sh) (Bash) bzw. [`start.ps1`](start.ps1) (PowerShell):

**Bash**
```bash
chmod +x start.sh
./start.sh
```

**PowerShell**
```powershell
.\start.ps1
```

## 🛡️ Sicherheit

- **Autorisierung** wird über zwei Policies (`ActiveSession`, `Admin`) serverseitig auf jedem Endpoint erzwungen, nicht nur im Frontend versteckt. Mitarbeitende dürfen Anträge nur für sich selbst anlegen/bearbeiten/löschen; einen Status ändern können ausschliesslich Admins, unabhängig davon, wem der Antrag gehört.
- **Rate Limiting**: max. 5 Login-Versuche pro Minute und IP (`429 Too Many Requests` danach).
- **Passwortrichtlinie**: mindestens 8 Zeichen, Gross-/Kleinbuchstaben, Zahl und Sonderzeichen — clientseitig und serverseitig identisch geprüft.
- **Audit-Log**: Einträge entstehen ausschliesslich serverseitig als Teil der jeweiligen Mutation (kein `POST`-Endpoint dafür), damit sie nicht gefälscht oder ausgelassen werden können.
- **Globales Exception-Handling**: unbehandelte Fehler werden strukturiert geloggt und liefern dem Client eine einheitliche, in Produktion nichtssagende Fehlermeldung (keine Stacktraces/interne Details nach aussen).
- **Bekannter offener Punkt** (bewusst nicht in dieser Semesterarbeit behoben): `appsettings.Development.json` enthält das lokale DB-Passwort und das Seed-Default-Passwort im Klartext im Repo — für eine echte Produktivumgebung müsste das in User-Secrets/Umgebungsvariablen ausgelagert werden.
- **Bekannter offener Punkt** (bewusst nicht in dieser Semesterarbeit behoben): Die Überschneidungsprüfung in `RequestsStore` (Create/Update) liest den aktuellen Datenbestand und schreibt danach, ohne Transaktion oder DB-Constraint dazwischen (TOCTOU). Erstellen zwei Personen im selben Sekundenbruchteil sich überschneidende Anträge, kann die Überschneidung theoretisch unentdeckt bleiben. Für eine echte Produktivumgebung wäre das über eine serialisierbare Transaktion (mit Retry) oder eine DB-seitige Exclusion-Constraint zu lösen; angesichts der Nutzungsintensität dieser Anwendung ist das Risiko hier vernachlässigbar.

## 🌍 Mehrsprachigkeit

Deutsch, Englisch und Französisch sind vollständig hinterlegt (`app/src/i18n/{de,en,fr}.json`, jeweils identische Key-Struktur). Die Sprache wird beim Start anhand des Browsers (`navigator.language`) gewählt, Fallback ist Englisch; es gibt aktuell keinen manuellen Sprachumschalter im UI.

## 🧪 Tests

**Frontend**: Vitest-Tests in `app/tests/`, ausführen mit `npm run test` im Ordner `app/`.
- `tests/unit/` — reine Utility-Funktionen aus `src/utils/` (Datumshilfsfunktionen, Initialen/Avatarfarben, Überschneidungslogik, Status-Metadaten) sowie der `useEmployeeStore`-Pinia-Store (`employee.spec.ts`, API-Modul gemockt).
- `tests/integration/` — `employeesApi` gegen ein gemocktes `fetch` (`employee-api.spec.ts`).

**Backend**: Aktuell existiert **kein automatisiertes Backend-Testprojekt** (xUnit o.ä.). Endpoints/Stores werden manuell im Browser bzw. über die API verifiziert. Das ist die grösste verbleibende Qualitätslücke des Projekts.

**E2E**: End-to-End-Tests mit [Playwright](https://playwright.dev) im Schwesterordner `../Fullstack-Ressourcix-E2E` (eigenes npm-Projekt, nicht Teil dieses Ordners). Sie starten keinen eigenen Server, sondern erwarten die laufende App unter `http://localhost:5167` (`baseURL` in `playwright.config.ts`) — Backend also vorher wie in [Setup und Start](#️-setup-und-start) beschrieben starten (HTTP-Profil reicht).

```bash
cd ../Fullstack-Ressourcix-E2E
npm install
npx playwright install   # einmalig: Browser-Binaries herunterladen
npx playwright test              # headless, alle Tests
npx playwright test --headed     # mit sichtbarem Browser
npx playwright test --ui         # interaktiver UI-Modus
npx playwright show-report       # letzten HTML-Report öffnen
```

## 📄 Weiterführende Dokumente

- [app/README.md](app/README.md) — Frontend-Struktur, Scripts, Vuetify-spezifische Hinweise.
