import { httpClient } from "./httpClient";
import type { AuditLogEntry, AuditLogAction } from "@/stores/auditLog";

interface CreateAuditLogPayload {
  action: AuditLogAction;
  summary: string;
  reference: string;
}

export const auditLogApi = {
  list: () => httpClient.get<AuditLogEntry[]>("/api/auditlog"),
  create: (payload: CreateAuditLogPayload) => httpClient.post<AuditLogEntry>("/api/auditlog", payload),
};
