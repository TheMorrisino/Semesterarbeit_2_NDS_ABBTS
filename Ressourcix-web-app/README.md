# Ressourcix

Abwesenheits- und Ferienverwaltung für KMU — Semesterarbeit 2 (NDS ABBTS).

Die eigentliche Anwendung (ASP.NET-Core-Backend + Vue-3-Frontend) liegt unter [`source/Fullstack-Ressourcix`](source/Fullstack-Ressourcix). Dort befindet sich auch die vollständige Dokumentation: Architektur, Rollen/Berechtigungen, Setup, Sicherheit, Mehrsprachigkeit.

**→ [source/Fullstack-Ressourcix/README.md](source/Fullstack-Ressourcix/README.md)**

## Struktur dieses Ordners

- `source/Fullstack-Ressourcix/` — die Anwendung selbst (Backend + `app/`-Frontend)
- `.config/dotnet-tools.json` — lokale .NET-Tools (u.a. `dotnet-ef`, `csharpier`)

Der Produktions-Build läuft über einen einzigen Befehl (`dotnet publish -c Release` in `source/Fullstack-Ressourcix/`) — siehe [source/Fullstack-Ressourcix/README.md#-produktions-build](source/Fullstack-Ressourcix/README.md#-produktions-build).
