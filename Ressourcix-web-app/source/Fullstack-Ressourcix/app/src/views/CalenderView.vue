<template>
  <div>
    <v-card>
      <!-- Toolbar: Jahr-/Monat-/Wochen-Navigation und Filter nach Abteilung/Ausbildung -->
      <div class="d-flex flex-wrap align-center ga-2 pa-4">
        <div class="d-flex align-center ga-1">
          <v-btn icon="mdi-chevron-double-left" size="small" variant="text" title="Ein Jahr zurück" @click="jumpYears(-1)" />
          <v-btn icon="mdi-chevron-left" size="small" variant="text" title="Ein Monat zurück" @click="jumpMonths(-1)" />
          <v-btn icon="mdi-menu-left" size="small" variant="text" title="Eine Woche zurück" @click="jumpWeeks(-1)" />
          <span class="text-subtitle-1 font-weight-medium mx-2 toolbar-label">{{ toolbarLabel }}</span>
          <v-btn icon="mdi-menu-right" size="small" variant="text" title="Eine Woche vor" @click="jumpWeeks(1)" />
          <v-btn icon="mdi-chevron-right" size="small" variant="text" title="Ein Monat vor" @click="jumpMonths(1)" />
          <v-btn icon="mdi-chevron-double-right" size="small" variant="text" title="Ein Jahr vor" @click="jumpYears(1)" />
        </div>

        <v-spacer />

      </div>

      <!-- Legende -->
      <div class="d-flex flex-wrap ga-4 px-4 pb-3 text-caption text-medium-emphasis">
        <span>🕒 Ausstehend</span>
        <span>✅ Genehmigt</span>
        <span>❌ Abgelehnt</span>
        <span>🏖 Bezogen</span>
        <span>🚫 Storniert</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--weekend" />Wochenende</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--overlap" />Überschneidungen (grün → rot)</span>
      </div>

      <!-- Tagesraster: Mitarbeiter als Zeilen, Kalendertage als scrollbare Spalten -->
      <div ref="scrollHost" class="calendar-scroll">
        <table class="calendar-table">
          <thead>
            <tr>
              <th class="name-col">Mitarbeiter [Ist/Soll]</th>
              <th
                v-for="day in visibleDays"
                :key="toISODate(day)"
                :class="{ 'is-weekend': isWeekend(day) }"
              >
                <div class="day-number">{{ day.getDate() }}</div>
                <div class="day-weekday">{{ weekdayLabel(day) }}</div>
              </th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="row in rows" :key="row.employee.id">
              <td class="name-col">
                {{ row.employee.name }}
                <span class="text-caption text-medium-emphasis">
                  [{{ row.plannedDays }}/{{ row.entitledDays }}] {{ row.remainingSymbol }}
                </span>
              </td>
              <td
                v-for="cell in row.cells"
                :key="cell.iso"
                :style="{ backgroundColor: cell.color }"
                class="clickable"
                @click="handleCellClick(row.employee, cell)"
              >{{ cell.icon }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </v-card>

    <!-- Antrag stellen / bearbeiten: Startdatum ist immer der angeklickte Tag und fix -->
    <v-dialog v-model="entryDialogOpen" max-width="460">
      <v-card v-if="entryDialog">
        <v-card-title>
          {{ entryDialog.mode === "create" ? "Antrag stellen" : "Antrag bearbeiten" }}
          – {{ entryDialog.employee.name }}
        </v-card-title>
        <v-card-text>
          <div class="mb-3 text-body-2">
            Start: <strong>{{ formatDate(entryDialog.startDate) }}</strong>
          </div>

          <v-text-field
            v-model="entryDialog.endDate"
            type="date"
            label="Ende"
            :min="entryDialog.startDate"
            density="compact"
          />

          <v-select
            v-if="entryDialog.mode === 'create'"
            v-model="entryDialog.type"
            :items="absenceTypeOptions"
            item-title="title"
            item-value="value"
            label="Typ"
            density="compact"
          />

          <v-text-field
            v-if="entryDialog.mode === 'create'"
            v-model="entryDialog.remark"
            label="Bemerkung (optional)"
            density="compact"
          />

          <v-select
            v-if="entryDialog.mode === 'edit'"
            v-model="entryDialog.status"
            :items="statusOptions"
            item-title="title"
            item-value="value"
            label="Status"
            density="compact"
          />

          <div v-if="dialogEndDateError" class="text-error text-body-2 mt-1">{{ dialogEndDateError }}</div>

          <!-- informativer Hinweis, Überschneidungen mit Kollegen blockieren das Speichern nicht (BR-01.04) -->
          <div v-if="overlappingColleagues.length" class="overlap-warning mt-3">
            <strong>Überschneidung erkannt:</strong>
            {{ overlappingColleagues.join(", ") }}
            {{ overlappingColleagues.length === 1 ? "ist" : "sind" }} im gewählten Zeitraum ebenfalls abwesend.
          </div>
        </v-card-text>
        <v-card-actions>
          <v-btn v-if="entryDialog.mode === 'edit'" variant="text" color="error" @click="deleteEntry">Löschen</v-btn>
          <v-spacer />
          <v-btn variant="text" @click="entryDialogOpen = false">Abbrechen</v-btn>
          <v-btn variant="tonal" color="primary" :disabled="!!dialogEndDateError" @click="saveEntry">Speichern</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick, watch } from "vue";
import { useRoute, useRouter } from "vue-router";
import { useEmployeeStore, Department, Qualification, type Employee } from "@/stores/employee";
import { useRequestStore, RequestStatus, AbsenceType, type Request } from "@/stores/request";
import { useAuditLogStore } from "@/stores/auditLog";
import { overlapColor } from "@/utils/overlapHeatmap";

// ===== Domain-Typen (Employee/Request kommen aus den Stores, hier nur UI-lokaler Zustand) =====

type EntryDialogMode = "create" | "edit";

interface EntryDialogState {
  mode: EntryDialogMode;
  employee: Employee;
  entryId: string | null; // null, solange der Antrag noch nicht gespeichert wurde
  startDate: string;
  endDate: string;
  type: AbsenceType;
  remark: string;
  status: RequestStatus;
}

interface DayCell {
  iso: string;
  color: string;
  icon: string;
  entry: Request | null;
}

interface EmployeeRow {
  employee: Employee;
  plannedDays: number;
  entitledDays: number;
  remainingSymbol: string;
  cells: DayCell[];
}

// ===== Anzeige-Labels für die Backend-Enums =====

const DEPARTMENT_LABELS: Record<Department, string> = {
  [Department.It]: "IT",
  [Department.HumanResources]: "Human Resources",
  [Department.Finance]: "Finance",
};

const QUALIFICATION_LABELS: Record<Qualification, string> = {
  [Qualification.GeneralIt]: "Allgemein IT",
  [Qualification.GeneralHr]: "Allgemein HR",
  [Qualification.GeneralFinance]: "Allgemein Finance",
  [Qualification.NursingFaGe]: "Fachfrau/Fachmann Gesundheit (FaGe)",
  [Qualification.HousekeepingEfz]: "Hauswirtschaft EFZ",
  [Qualification.SocialPedagogyHf]: "HF Sozialpädagogik",
  [Qualification.NursingAssistanceSbbk]: "Pflegeassistenz SBBK",
  [Qualification.Medicine]: "Arzt/Ärztin",
  [Qualification.PhysiotherapyBsc]: "BSc Physiotherapie",
  [Qualification.OccupationalTherapyBsc]: "BSc Ergotherapie",
  [Qualification.SpitexBasicCourse]: "Spitex-Grundkurs",
};

const STATUS_LABELS: Record<RequestStatus, string> = {
  [RequestStatus.Open]: "Ausstehend",
  [RequestStatus.Approved]: "Genehmigt",
  [RequestStatus.Rejected]: "Abgelehnt",
  [RequestStatus.Taken]: "Bezogen",
  [RequestStatus.Cancelled]: "Storniert",
};

const departmentOptions = Object.values(Department).map((value) => ({ title: DEPARTMENT_LABELS[value], value }));
const educationOptions = Object.values(Qualification).map((value) => ({ title: QUALIFICATION_LABELS[value], value }));
const statusOptions = Object.values(RequestStatus).map((value) => ({ title: STATUS_LABELS[value], value }));

const absenceTypeOptions: { title: string; value: AbsenceType }[] = [
  { title: "Ferien", value: AbsenceType.Vacation },
  { title: "Kompensation", value: AbsenceType.Compensation },
  { title: "Unbezahlter Urlaub", value: AbsenceType.UnpaidLeave },
];

const STATUS_ICON: Record<RequestStatus, string> = {
  [RequestStatus.Open]: "🕒",
  [RequestStatus.Approved]: "✅",
  [RequestStatus.Rejected]: "❌",
  [RequestStatus.Taken]: "🏖",
  [RequestStatus.Cancelled]: "🚫",
};

// ===== Konstanten für das Tagesraster =====

const DAY_COLUMN_WIDTH_PX = 40;
const NAME_COLUMN_WIDTH_PX = 260;
const INITIAL_WINDOW_RADIUS_DAYS = 45; // ca. 1.5 Monate in jede Richtung -> ca. 3 Monate sichtbar

const WEEKDAY_LABELS = ["So", "Mo", "Di", "Mi", "Do", "Fr", "Sa"];

// ===== Datumshilfsfunktionen =====

function addDays(date: Date, days: number): Date {
  const result = new Date(date);
  result.setDate(result.getDate() + days);
  return result;
}

function addMonths(date: Date, months: number): Date {
  const result = new Date(date);
  result.setMonth(result.getMonth() + months);
  return result;
}

function addYears(date: Date, years: number): Date {
  const result = new Date(date);
  result.setFullYear(result.getFullYear() + years);
  return result;
}

function buildDayRange(start: Date, end: Date): Date[] {
  const days: Date[] = [];
  let cursor = new Date(start);
  while (cursor <= end) {
    days.push(new Date(cursor));
    cursor = addDays(cursor, 1);
  }
  return days;
}

function toISODate(date: Date): string {
  const year = date.getFullYear();
  const month = String(date.getMonth() + 1).padStart(2, "0");
  const day = String(date.getDate()).padStart(2, "0");
  return `${year}-${month}-${day}`;
}

function formatDate(iso: string): string {
  const [year, month, day] = iso.split("-");
  return `${day}.${month}.${year}`;
}

function weekdayLabel(date: Date): string {
  return WEEKDAY_LABELS[date.getDay()];
}

function isWeekend(date: Date): boolean {
  const day = date.getDay();
  return day === 0 || day === 6;
}

function daysBetweenInclusive(startIso: string, endIso: string): number {
  const start = new Date(startIso);
  const end = new Date(endIso);
  return Math.round((end.getTime() - start.getTime()) / 86_400_000) + 1;
}

// ===== Zustand =====

const route = useRoute();
const router = useRouter();
const employeeStore = useEmployeeStore();
const requestStore = useRequestStore();
const auditLog = useAuditLogStore();

const centerDate = ref(new Date(2026, 6, 15)); // Mitte Juli 2026, passend zu den Seed-Daten des Backends
const scrollHost = ref<HTMLDivElement | null>(null);
const visibleDays = ref<Date[]>(
  buildDayRange(
    addDays(centerDate.value, -INITIAL_WINDOW_RADIUS_DAYS),
    addDays(centerDate.value, INITIAL_WINDOW_RADIUS_DAYS),
  ),
);
const departmentFilter = ref<Department | null>(null);
const educationFilter = ref<Qualification | null>(null);
const entryDialog = ref<EntryDialogState | null>(null);

// ===== Abgeleitete Daten =====

const toolbarLabel = computed(() =>
  new Intl.DateTimeFormat("de-CH", { month: "long", year: "numeric" }).format(centerDate.value),
);

const filteredEmployees = computed(() =>
  employeeStore.employees.filter(
    (employee) =>
      (!departmentFilter.value || employee.department === departmentFilter.value) &&
      (!educationFilter.value || employee.education === educationFilter.value),
  ),
);

function requestsForEmployee(employeeId: string): Request[] {
  return requestStore.requests.filter((request) => request.employeeId === employeeId);
}

function requestOnDay(employeeId: string, day: Date): Request | undefined {
  const iso = toISODate(day);
  return requestsForEmployee(employeeId).find((request) => iso >= request.from && iso <= request.until);
}

function absentCountOnDay(day: Date): number {
  const iso = toISODate(day);
  return filteredEmployees.value.filter((employee) =>
    requestsForEmployee(employee.id).some((request) => iso >= request.from && iso <= request.until),
  ).length;
}

// Heatmap-Farbe pro Tag einmal pro Spalte berechnen, statt pro Zelle x Mitarbeiter
const dayColorByDate = computed(() => {
  const colors = new Map<string, string>();
  for (const day of visibleDays.value) {
    colors.set(toISODate(day), overlapColor(absentCountOnDay(day)));
  }
  return colors;
});

function plannedDaysCount(employee: Employee): number {
  return requestsForEmployee(employee.id).reduce(
    (sum, request) => sum + daysBetweenInclusive(request.from, request.until),
    0,
  );
}

function remainingSymbol(entitledDays: number, plannedDays: number): string {
  const remaining = entitledDays - plannedDays;
  if (remaining > 0) return "⏳";
  if (remaining === 0) return "✔";
  return "✖";
}

const rows = computed<EmployeeRow[]>(() =>
  filteredEmployees.value.map((employee) => {
    const planned = plannedDaysCount(employee);
    const entitled = employee.vacationDays * 5;
    const cells: DayCell[] = visibleDays.value.map((day) => {
      const iso = toISODate(day);
      const request = requestOnDay(employee.id, day) ?? null;
      return {
        iso,
        color: dayColorByDate.value.get(iso) ?? "",
        icon: request ? STATUS_ICON[request.status] : "",
        entry: request,
      };
    });
    return {
      employee,
      plannedDays: planned,
      entitledDays: entitled,
      remainingSymbol: remainingSymbol(entitled, planned),
      cells,
    };
  }),
);

// ===== Navigation (Jahr/Monat/Woche) =====

function jumpTo(newCenter: Date) {
  centerDate.value = newCenter;
  visibleDays.value = buildDayRange(
    addDays(newCenter, -INITIAL_WINDOW_RADIUS_DAYS),
    addDays(newCenter, INITIAL_WINDOW_RADIUS_DAYS),
  );
  nextTick(() => centerScroll());
}

function jumpYears(delta: number) {
  jumpTo(addYears(centerDate.value, delta));
}

function jumpMonths(delta: number) {
  jumpTo(addMonths(centerDate.value, delta));
}

function jumpWeeks(delta: number) {
  jumpTo(addDays(centerDate.value, delta * 7));
}

function centerScroll() {
  const el = scrollHost.value;
  if (!el) return;
  el.scrollLeft = Math.max(0, INITIAL_WINDOW_RADIUS_DAYS * DAY_COLUMN_WIDTH_PX - el.clientWidth / 2);
}

// ===== Antrag stellen / bearbeiten =====

function handleCellClick(employee: Employee, cell: DayCell) {
  if (cell.entry) {
    openEditDialog(employee, cell.entry);
  } else {
    openCreateDialog(employee, cell.iso);
  }
}

function openCreateDialog(employee: Employee, startIso: string) {
  entryDialog.value = {
    mode: "create",
    employee,
    entryId: null,
    startDate: startIso,
    endDate: startIso,
    type: AbsenceType.Vacation,
    remark: "",
    status: RequestStatus.Open,
  };
}

function openEditDialog(employee: Employee, request: Request) {
  entryDialog.value = {
    mode: "edit",
    employee,
    entryId: request.id,
    startDate: request.from,
    endDate: request.until,
    type: request.type,
    remark: request.remark ?? "",
    status: request.status,
  };
}

const entryDialogOpen = computed({
  get: () => entryDialog.value !== null,
  set: (open: boolean) => {
    if (!open) entryDialog.value = null;
  },
});

// Ein Mitarbeiter darf sich nicht selbst überschneiden -> blockiert das Speichern
function hasSelfOverlap(state: EntryDialogState): boolean {
  return requestStore.requests.some(
    (request) =>
      request.employeeId === state.employee.id &&
      request.id !== state.entryId &&
      state.startDate <= request.until &&
      state.endDate >= request.from,
  );
}

const dialogEndDateError = computed<string | null>(() => {
  const state = entryDialog.value;
  if (!state) return null;
  if (state.endDate < state.startDate) return "Enddatum darf nicht vor dem Startdatum liegen";
  if (hasSelfOverlap(state)) return "Überschneidet sich mit einem bestehenden eigenen Antrag";
  return null;
});

// Überschneidung mit Kollegen ist nur ein Hinweis und blockiert das Speichern nicht (BR-01.04)
const overlappingColleagues = computed<string[]>(() => {
  const state = entryDialog.value;
  if (!state) return [];
  const names = new Set<string>();
  for (const request of requestStore.requests) {
    if (request.employeeId === state.employee.id) continue;
    if (request.id === state.entryId) continue;
    if (state.startDate <= request.until && state.endDate >= request.from) {
      const colleague = employeeStore.employees.find((employee) => employee.id === request.employeeId);
      if (colleague) names.add(colleague.name);
    }
  }
  return Array.from(names);
});

async function saveEntry() {
  const state = entryDialog.value;
  if (!state || dialogEndDateError.value) return;

  if (state.mode === "create") {
    const created = await requestStore.create({
      employeeId: state.employee.id,
      from: state.startDate,
      until: state.endDate,
      days: daysBetweenInclusive(state.startDate, state.endDate),
      overlap: overlappingColleagues.value.length > 0,
      type: state.type,
      remark: state.remark.trim() || null,
    });
    await auditLog.log("RequestCreated", "Ferienantrag erfasst", created.id);
  } else if (state.entryId) {
    await requestStore.update(state.entryId, state.endDate, state.status);
    await auditLog.log("RequestUpdated", "Ferienantrag geändert", state.entryId);
  }
  entryDialog.value = null;
}

async function deleteEntry() {
  const state = entryDialog.value;
  if (!state || state.mode !== "edit" || !state.entryId) return;
  await requestStore.remove(state.entryId);
  await auditLog.log("RequestDeleted", "Ferienantrag gelöscht", state.entryId);
  entryDialog.value = null;
}

// Springt zu einem per Query-Param übergebenen Antrag (z.B. von der Genehmigungen-Seite aus)
function jumpToRequest(requestId: string) {
  const request = requestStore.requests.find((r) => r.id === requestId);
  const employee = request ? employeeStore.employees.find((e) => e.id === request.employeeId) : undefined;
  if (!request || !employee) return;

  jumpTo(new Date(request.from));
  openEditDialog(employee, request);
  router.replace({ query: {} });
}

onMounted(async () => {
  nextTick(() => centerScroll());
  await Promise.all([employeeStore.load(), requestStore.load()]);

  const requestId = route.query.requestId;
  if (typeof requestId === "string") {
    jumpToRequest(requestId);
  }
});
</script>

<style scoped>
.toolbar-label {
  min-width: 140px;
  text-align: center;
}

.filter-select {
  max-width: 220px;
}

.calendar-scroll {
  /* hidden statt auto: Navigation läuft ausschliesslich über die Pfeile in der Toolbar (jumpYears/jumpMonths/jumpWeeks),
     nicht über Scrollbar/Mausrad/Trackpad. Die Pfeile setzen scrollLeft weiterhin per Code (siehe centerScroll). */
  overflow-x: hidden;
  max-width: 100%;
}

.calendar-table {
  border-collapse: collapse;
  font-size: 12px;
}

.calendar-table th,
.calendar-table td {
  border: 1px solid rgba(128, 128, 128, 0.2);
  text-align: center;
  padding: 4px;
  height: 32px;
}

/* :not(.name-col), da diese Regel sonst wegen höherer Spezifität die Breite der Namensspalte überschreibt */
.calendar-table th:not(.name-col),
.calendar-table td:not(.name-col) {
  width: v-bind(DAY_COLUMN_WIDTH_PX + "px");
  min-width: v-bind(DAY_COLUMN_WIDTH_PX + "px");
  max-width: v-bind(DAY_COLUMN_WIDTH_PX + "px");
}

.name-col {
  position: sticky;
  left: 0;
  z-index: 2;
  background: rgb(var(--v-theme-surface));
  text-align: left;
  min-width: v-bind(NAME_COLUMN_WIDTH_PX + "px");
  max-width: v-bind(NAME_COLUMN_WIDTH_PX + "px");
  width: v-bind(NAME_COLUMN_WIDTH_PX + "px");
  white-space: nowrap;
  padding: 4px 8px;
}

thead .name-col {
  z-index: 3;
}

.day-number {
  font-weight: 600;
}

.day-weekday {
  font-size: 10px;
  opacity: 0.6;
}

th.is-weekend {
  background: rgba(128, 128, 128, 0.15);
}

td.clickable {
  cursor: pointer;
}

.legend-swatch {
  display: inline-block;
  width: 14px;
  height: 14px;
  border-radius: 3px;
}

.legend-swatch--weekend {
  background: rgba(128, 128, 128, 0.15);
  border: 1px solid rgba(128, 128, 128, 0.3);
}

.legend-swatch--overlap {
  background: linear-gradient(90deg, rgb(210, 245, 210), rgb(255, 250, 200), rgb(255, 200, 200));
}

.overlap-warning {
  background: rgba(239, 159, 39, 0.15);
  color: rgb(133, 79, 11);
  border-radius: 8px;
  padding: 10px 12px;
  font-size: 13px;
}
</style>
