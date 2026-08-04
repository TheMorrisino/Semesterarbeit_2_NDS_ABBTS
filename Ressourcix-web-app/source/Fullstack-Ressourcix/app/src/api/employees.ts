import { httpClient } from "./httpClient";
import type { Employee } from "@/stores/employee";

export const employeesApi = {
  list: () => httpClient.get<Employee[]>("/api/employees"),
  create: (employee: Omit<Employee, "id">) => httpClient.post<Employee>("/api/employees", employee),
  update: (id: string, employee: Omit<Employee, "id">) => httpClient.put<void>(`/api/employees/${id}`, employee),
  toggleActive: (id: string) => httpClient.put<void>(`/api/employees/${id}/toggle-active`),
};
