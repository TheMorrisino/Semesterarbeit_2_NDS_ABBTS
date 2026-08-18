# Ressourcix – Frontend

Vue-3-SPA des Ressourcix-Backends (siehe [../README.md](../README.md) für den Gesamtüberblick über Architektur, Rollen und Setup). Läuft im Dev-Betrieb als eigener Vite-Dev-Server, den das Backend per `Mumrich.SpaDevMiddleware` einbindet — separat gestartet wird dieser Ordner normalerweise nicht.

## 🧱 Stack

- Framework: Vue 3 + Vite
- UI-Bibliothek: Vuetify
- State Management: Pinia
- Routing: Vue Router
- i18n: vue-i18n (Deutsch/Englisch/Französisch, siehe `src/i18n/`)
- Styling: UnoCSS + Vuetify-Preset
- Sprache: TypeScript

## 📁 Projektstruktur

- `src/main.ts` — Einstiegspunkt, registriert Pinia/Router/Vuetify/i18n
- `src/App.vue` — Wurzelkomponente (Navigation, Login-Weiche)
- `src/views/` — Seiten (Dashboard, Kalender, Abwesenheiten, Genehmigungen, Mitarbeiter, Team, Audit-Log, Login, Passwort ändern)
- `src/components/` — wiederverwendbare Komponenten (z.B. `OverlapChart.vue`, `AppUserInfo.vue`)
- `src/composables/` — extrahierte Vue-Composables mit testbarer View-Logik (`useEntryDialog` für Dialog-Zustand/Validierung, `useVisibleDayCount` für die Grid-Virtualisierung des Kalenders)
- `src/stores/` — Pinia-Stores (`auth`, `employee`, `request`, `auditLog`)
- `src/api/` — typisierte Fetch-Wrapper pro Ressource, alle über `src/api/httpClient.ts`
- `src/router/` — Routing inkl. Auth-/Rollen-Guards
- `src/i18n/` — Übersetzungsdateien `de.json`/`en.json`/`fr.json` (identische Key-Struktur)
- `src/utils/` — reine, unit-getestete Hilfsfunktionen: `date.ts` (Datumsarithmetik), `overlap.ts` (Überschneidungslogik), `overlapHeatmap.ts` (Heatmap-Farben), `statusMeta.ts` (Status → Label/Farbe/Icon), `initials.ts` (Kürzel/Avatarfarben) — jeweils mit `*.test.ts` daneben
- `src/plugins/` — Plugin-Setup (Vuetify, i18n)
- `src/styles/` — globale Styles/Theme
- `public/` — statische Dateien

## 💿 Installieren

```bash
npm install
```

## 🚀 Entwicklung

Normalerweise über das Backend gestartet (`dotnet run --launch-profile https` im übergeordneten Ordner). Für einen isolierten Frontend-Dev-Server ohne Backend (z.B. reine UI-Arbeit, ohne funktionierende API-Calls):

```bash
npm run dev
```

## 🏗️ Build

```bash
npm run build
```

## 🧪 Verfügbare Scripts

- `npm run dev` — Vite-Dev-Server
- `npm run build` — Type-Check + Produktions-Build
- `npm run build-only` — Produktions-Build ohne vorherigen Type-Check
- `npm run preview` — gebauten Build lokal ausliefern
- `npm run type-check` — `vue-tsc --build --force`
- `npm run lint` / `npm run lint:fix` — ESLint
- `npm run test` — Vitest-Unit-Tests für `src/utils/` (einmaliger Lauf, kein Watch-Modus)
- `npm run mcp` / `npm run mcp:revert` — Ruler-MCP-Konfiguration anwenden/zurücksetzen

## 🧩 Empfohlene VS-Code-Erweiterungen

Hinterlegt in `.vscode/extensions.json`: **Vue - Official** (`vue.volar`) und **Vuetify** (`vuetifyjs.vuetify-vscode`).
