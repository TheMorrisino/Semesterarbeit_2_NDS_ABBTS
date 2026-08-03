import { defineStore } from "pinia";

// Welche Aktionen den Nutzer interessieren (BR-01.07 "Nachvollziehbarkeit von Mutationen") -
// bewusst kein technisches/Debug-Logging, nur fachliche Mutationen an Ferienanträgen und Mitarbeitenden.
export type AuditLogAction =
  | "antragErfasst"
  | "antragGeaendert"
  | "antragGeloescht"
  | "mitarbeiterErfasst"
  | "mitarbeiterGeaendert"
  | "mitarbeiterStatusGeaendert";

export interface AuditLogEntry {
  id: number;
  action: AuditLogAction;
  summary: string;
  reference: string;
  actor: string;
  timestamp: string; // ISO
}

// Es gibt noch keinen echten Login -> fixer Platzhalter für "wer hat es getan"
const CURRENT_USER = "Tiago de Sousa";

export const useAuditLogStore = defineStore("auditLog", {
  state: () => ({
    entries: [] as AuditLogEntry[],
    nextId: 1,
  }),
  actions: {
    log(action: AuditLogAction, summary: string, reference: string) {
      this.entries.unshift({
        id: this.nextId++,
        action,
        summary,
        reference,
        actor: CURRENT_USER,
        timestamp: new Date().toISOString(),
      });
    },
  },
});
