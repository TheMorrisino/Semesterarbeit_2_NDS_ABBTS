<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>{{ t('teamview.titel') }}</h1>
    </div>

    <!-- Stat Cards -->
    <v-row class="mb-4">
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-account-group" size="18" />
            <span class="text-caption text-uppercase">{{ t('teamview.employee') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.employeeCount }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-account-clock" size="18" color="blue" />
            <span class="text-caption text-uppercase">{{ t('teamview.currentlyAbsent') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.currentlyAbsent }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-clock-outline" size="18" color="orange" />
            <span class="text-caption text-uppercase">{{ t('teamview.open') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.offen }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-alert" size="18" color="red" />
            <span class="text-caption text-uppercase">{{ t('teamview.overlaps') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.overlaps }}</div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Team Table -->
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-account-group" class="mr-2" />{{ t('teamview.saldoStatus') }}</span>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="teamRows"
        item-value="id"
      >
        <template #item.employee="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(item.name)">
              <span class="text-caption">{{ initials(item.name) }}</span>
            </v-avatar>
            {{ item.name }}
          </div>
        </template>

        <template #item.status="{ item }">
          <v-chip :color="statusColor(item.status)" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.status }}
          </v-chip>
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

interface Employee {
  id: string;
  name: string;
  role: string;
  workloadPercent: number;
  vacationWeeks: number;
  isActive: boolean;
}

interface Request {
  id: string;
  employeeId: string;
  from: string;
  until: string;
  days: number;
  overlap: boolean;
  status: "Open" | "Approved" | "Rejected";
  submittedOn: string;
}

interface TeamRow {
  id: string;
  name: string;
  entitlement: number;
  taken: number;
  planned: number;
  remaining: number;
  status: string;
}

const headers = [
  { title: t("teamview.employee"), key: "employee" },
  { title: t("teamview.entitlement"), key: "entitlement" },
  { title: t("teamview.taken"), key: "taken" },
  { title: t("teamview.planned"), key: "planned" },
  { title: t("teamview.remaining"), key: "remaining" },
  { title: t("teamview.status"), key: "status" },
];

const employees = ref<Employee[]>([]);
const requests = ref<Request[]>([]);

async function loadEmployees() {
  const res = await fetch("/api/employees");
  employees.value = await res.json();
}

async function loadRequests() {
  const res = await fetch("/api/requests");
  requests.value = await res.json();
}

// --- Vacation entitlement: from vacation weeks -> days (e.g. 5 weeks x 5 workdays = 25 days) ---
function entitlementDays(e: Employee): number {
  return Math.round(e.vacationWeeks * 5);
}

// --- Taken: approved requests whose end date is in the past ---
function takenDays(employeeId: string): number {
  return requests.value
    .filter((r) => r.employeeId === employeeId && r.status === "Approved" && new Date(r.until) < new Date())
    .reduce((sum, r) => sum + r.days, 0);
}

// --- Planned: approved requests in the future ---
function plannedDays(employeeId: string): number {
  return requests.value
    .filter((r) => r.employeeId === employeeId && r.status === "Approved" && new Date(r.until) >= new Date())
    .reduce((sum, r) => sum + r.days, 0);
}

// --- Status: is there an open request for this person? ---
function employeeStatus(employeeId: string): string {
  const hasOpen = requests.value.some((r) => r.employeeId === employeeId && r.status === "Open");
  return hasOpen ? t("teamview.open") : t("teamview.geplant");
}

const teamRows = computed<TeamRow[]>(() =>
  employees.value.map((e) => {
    const entitlement = entitlementDays(e);
    const taken = takenDays(e.id);
    const planned = plannedDays(e.id);
    return {
      id: e.id,
      name: e.name,
      entitlement,
      taken,
      planned,
      remaining: entitlement - taken - planned,
      status: employeeStatus(e.id),
    };
  })
);

const stats = computed(() => ({
  employeeCount: employees.value.length,
  currentlyAbsent: requests.value.filter((r) => {
    const today = new Date();
    return r.status === "Approved" && new Date(r.from) <= today && new Date(r.until) >= today;
  }).length,
  offen: requests.value.filter((r) => r.status === "Open").length,
  overlaps: requests.value.filter((r) => r.overlap).length,
}));

function initials(name: string) {
  return name.split(" ").map((n) => n[0]).join("").toUpperCase();
}

function avatarColor(name: string) {
  const colors = ["purple", "red", "blue", "teal", "indigo"];
  return colors[name.length % colors.length];
}

function statusColor(status: string) {
  return status === t("teamview.open") ? "orange" : "green";
}

onMounted(async () => {
  await Promise.all([loadEmployees(), loadRequests()]);
});
</script>