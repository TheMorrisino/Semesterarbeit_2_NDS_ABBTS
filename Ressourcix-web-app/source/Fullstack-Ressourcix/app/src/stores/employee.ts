import { defineStore } from "pinia";
import { employeesApi } from "@/api/employees";

// Organisatorische Abteilung eines Mitarbeitenden (getrennt von der Ausbildung/Qualifikation)
export enum Department {
  It = "It",
  HumanResources = "HumanResources",
  Finance = "Finance",
}

// Ausbildung/Qualifikation, unabhängig von der Abteilung
export enum Qualification {
  GeneralIt = "GeneralIt",
  GeneralHr = "GeneralHr",
  GeneralFinance = "GeneralFinance",
  NursingFaGe = "NursingFaGe", // Fachfrau/Fachmann Gesundheit (FaGe)
  HousekeepingEfz = "HousekeepingEfz", // Hauswirtschaft EFZ
  SocialPedagogyHf = "SocialPedagogyHf", // HF Sozialpädagogik
  NursingAssistanceSbbk = "NursingAssistanceSbbk", // Pflegeassistenz SBBK
  Medicine = "Medicine", // Arzt/Ärztin
  PhysiotherapyBsc = "PhysiotherapyBsc", // BSc Physiotherapie
  OccupationalTherapyBsc = "OccupationalTherapyBsc", // BSc Ergotherapie
  SpitexBasicCourse = "SpitexBasicCourse", // Spitex-spezifischer Grundkurs
}

export interface Employee {
  id: string;
  name: string;
  role: string;
  workload: number;
  vacationDays: number;
  isActive: boolean;
  department: Department;
  education: Qualification;
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
  },
});
