import { defineStore } from 'pinia'
import { auditLogApi } from '@/api/auditLog'

// Welche Aktionen den Nutzer interessieren (BR-01.07 "Nachvollziehbarkeit von Mutationen") -
// bewusst kein technisches/Debug-Logging, nur fachliche Mutationen an Ferienanträgen und Mitarbeitenden.
// Namen entsprechen 1:1 dem AuditLogAction-Enum im Backend.
export type AuditLogAction
  = | 'RequestCreated'
    | 'RequestUpdated'
    | 'RequestDeleted'
    | 'EmployeeCreated'
    | 'EmployeeUpdated'
    | 'EmployeeStatusChanged'
    | 'EmployeeDeleted'

export interface AuditLogEntry {
  id: string
  action: AuditLogAction
  summary: string
  reference: string // Guid des betroffenen Requests/Employees
  actor: string
  timestamp: string // ISO
}

export const useAuditLogStore = defineStore('auditLog', {
  state: () => ({
    entries: [] as AuditLogEntry[],
    loading: false,
  }),
  actions: {
    async load () {
      this.loading = true
      try {
        const entries = await auditLogApi.list()
        // toSorted() bräuchte lib ES2023 (Projekt-Ziel ist ES2022, siehe tsconfig) - Kopie + sort() genügt hier.
        // eslint-disable-next-line unicorn/no-array-sort
        this.entries = [...entries].sort(
          (a, b) => new Date(b.timestamp).getTime() - new Date(a.timestamp).getTime(),
        )
      } finally {
        this.loading = false
      }
    },
  },
})
