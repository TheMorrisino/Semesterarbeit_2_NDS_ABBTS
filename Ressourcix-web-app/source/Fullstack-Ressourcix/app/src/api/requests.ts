import { httpClient } from "./httpClient";
import type { Request, RequestStatus } from "@/stores/request";

type CreateRequestPayload = Pick<Request, "employeeId" | "from" | "until" | "days" | "overlap" | "type" | "remark">;

export const requestsApi = {
  list: (status?: "open") => httpClient.get<Request[]>(status ? `/api/requests?status=${status}` : "/api/requests"),
  create: (payload: CreateRequestPayload) => httpClient.post<Request>("/api/requests", payload),
  update: (id: string, until: string, status: RequestStatus) =>
    httpClient.put<void>(`/api/requests/${id}`, { until, status }),
  approve: (id: string) => httpClient.put<void>(`/api/requests/${id}/approve`),
  reject: (id: string) => httpClient.put<void>(`/api/requests/${id}/reject`),
  remove: (id: string) => httpClient.delete<void>(`/api/requests/${id}`),
};
