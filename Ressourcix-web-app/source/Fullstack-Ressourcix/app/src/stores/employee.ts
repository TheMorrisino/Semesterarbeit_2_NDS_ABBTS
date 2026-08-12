import { defineStore } from "pinia";
import { employeesApi } from "@/api/employees";



export interface Employee {
  id: string;
  name: string;
  role: string;
  workload: number;
  vacationDays: number;
  isActive: boolean;
  username: string;
  permissionLevel: number;
  mustChangePassword?: boolean;
}

export const useEmployeeStore = defineStore("employees", {
  state: () => ({
    employees: [] as Employee[],
    loading: false,
  }),
  actions: {
    async load() {
      this.loading = true;
      try {
        this.employees = await employeesApi.list();
      } finally {
        this.loading = false;
      }
    },
    async create(employee: Omit<Employee, "id">) {
      const created = await employeesApi.create(employee);
      this.employees.push(created);
      return created;
    },
    async update(id: string, employee: Omit<Employee, "id">) {
      await employeesApi.update(id, employee);
      const index = this.employees.findIndex((e) => e.id === id);
      if (index !== -1) this.employees[index] = { id, ...employee };
    },
    async toggleActive(id: string) {
      await employeesApi.toggleActive(id);
      const employee = this.employees.find((e) => e.id === id);
      if (employee) employee.isActive = !employee.isActive;
    },
    async remove(id: string) {
      await employeesApi.remove(id);
      this.employees = this.employees.filter((e) => e.id !== id);
    },
  },
});
