import { httpClient } from "./httpClient";
import type { AuditLogEntry } from "@/stores/auditLog";

// Bewusst kein create(): Audit-Log-Einträge entstehen ausschliesslich serverseitig als Nebeneffekt
// der jeweiligen Mutation (siehe EmployeeStore/RequestsStore im Backend), nicht per direktem API-Aufruf.
export const auditLogApi = {
  list: () => httpClient.get<AuditLogEntry[]>("/api/auditlog"),
};
