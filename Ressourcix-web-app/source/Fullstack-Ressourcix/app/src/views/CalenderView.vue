<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>Abwesenheitskalender</h1>
    </div>

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

        <v-select
          v-model="departmentFilter"
          :items="departmentOptions"
          label="Abteilung"
          clearable
          density="compact"
          hide-details
          class="filter-select"
        />
        <v-select
          v-model="educationFilter"
          :items="educationOptions"
          label="Ausbildung"
          clearable
          density="compact"
          hide-details
          class="filter-select"
        />
      </div>

      <!-- Legende -->
      <div class="d-flex flex-wrap ga-4 px-4 pb-3 text-caption text-medium-emphasis">
        <span>🕒 Ausstehend</span>
        <span>✅ Genehmigt</span>
        <span>❌ Abgelehnt</span>
        <span>🏖 Bezogen</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--weekend" />Wochenende</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--overlap" />Überschneidungen (grün → rot)</span>
      </div>

      <!-- Tagesraster: Mitarbeiter als Zeilen, Kalendertage als scrollbare Spalten -->
      <div ref="scrollHost" class="calendar-scroll" @scroll="onScroll">
        <table class="calendar-table">
          <thead>
            <tr>
              <th class="name-col">Mitarbeiter</th>
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
                {{ row.employee.firstName }} {{ row.employee.lastName }}
                <span class="text-caption text-medium-emphasis">
                  [{{ row.plannedDays }}/{{ row.employee.vacationDaysEntitled }}] {{ row.remainingSymbol }}
                </span>
              </td>
              <td
                v-for="cell in row.cells"
                :key="cell.iso"
                :style="{ backgroundColor: cell.color }"
                :class="{ clickable: cell.entry }"
                @click="cell.entry && openDetail(row.employee, cell.entry)"
              >{{ cell.icon }}</td>
            </tr>
          </tbody>
        </table>
      </div>
    </v-card>

    <!-- Detail-Popup für einen einzelnen Ferieneintrag -->
    <v-dialog v-model="detailDialogOpen" max-width="420">
      <v-card v-if="selectedDetail">
        <v-card-title>{{ selectedDetail.employee.firstName }} {{ selectedDetail.employee.lastName }}</v-card-title>
        <v-card-text>
          <v-chip :color="STATUS_COLOR[selectedDetail.entry.status]" size="small" variant="tonal" class="mb-3">
            {{ selectedDetail.entry.status }}
          </v-chip>
          <div>Abteilung: {{ selectedDetail.employee.department }}</div>
          <div>Ausbildung: {{ selectedDetail.employee.education }}</div>
          <div>Zeitraum: {{ formatDate(selectedDetail.entry.startDate) }} – {{ formatDate(selectedDetail.entry.endDate) }}</div>
          <div>Tage: {{ daysBetweenInclusive(selectedDetail.entry.startDate, selectedDetail.entry.endDate) }}</div>
        </v-card-text>
        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="detailDialogOpen = false">Schliessen</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, nextTick } from "vue";

// ===== Domain-Typen =====

type Department = "Aussendienst" | "Admin" | "Planung";
type Education = "Lehrling" | "EFZ" | "Dipl. Pflegefachfrau HF";
type VacationStatus = "Ausstehend" | "Genehmigt" | "Abgelehnt" | "Bezogen";

interface Employee {
  id: number;
  firstName: string;
  lastName: string;
  department: Department;
  education: Education;
  vacationDaysEntitled: number;
}

interface VacationEntry {
  id: number;
  employeeId: number;
  startDate: string; // ISO-Format yyyy-mm-dd
  endDate: string;
  status: VacationStatus;
}

interface DayCell {
  iso: string;
  color: string;
  icon: string;
  entry: VacationEntry | null;
}

interface EmployeeRow {
  employee: Employee;
  plannedDays: number;
  remainingSymbol: string;
  cells: DayCell[];
}

// ===== Hardcodierte Mock-Daten =====

const departmentOptions: Department[] = ["Aussendienst", "Admin", "Planung"];
const educationOptions: Education[] = ["Lehrling", "EFZ", "Dipl. Pflegefachfrau HF"];

const STATUS_ICON: Record<VacationStatus, string> = {
  Ausstehend: "🕒",
  Genehmigt: "✅",
  Abgelehnt: "❌",
  Bezogen: "🏖",
};

const STATUS_COLOR: Record<VacationStatus, string> = {
  Ausstehend: "orange",
  Genehmigt: "green",
  Abgelehnt: "red",
  Bezogen: "blue",
};

const employees: Employee[] = [
  { id: 1, firstName: "Morris", lastName: "Meier", department: "Aussendienst", education: "EFZ", vacationDaysEntitled: 25 },
  { id: 2, firstName: "Pedro", lastName: "Santos", department: "Planung", education: "EFZ", vacationDaysEntitled: 25 },
  { id: 3, firstName: "Tiago", lastName: "de Sousa", department: "Admin", education: "Dipl. Pflegefachfrau HF", vacationDaysEntitled: 25 },
  { id: 4, firstName: "Lena", lastName: "Brunner", department: "Aussendienst", education: "Lehrling", vacationDaysEntitled: 22 },
  { id: 5, firstName: "Rafael", lastName: "Koch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 6, firstName: "Rafqweael", lastName: "Kofsch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 7, firstName: "Raqewfael", lastName: "Kosdfch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 8, firstName: "Raqewfael", lastName: "Kosafch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 9, firstName: "Rafdaael", lastName: "Koafsch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 10, firstName: "Raadsfael", lastName: "Kocafsh", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 11, firstName: "Radasfael", lastName: "Koafch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 12, firstName: "Radasfael", lastName: "Kfsoch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 13, firstName: "Racyxfael", lastName: "Koasfch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 14, firstName: "Racyxfael", lastName: "Kocfash", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 15, firstName: "Racxfael", lastName: "Kofsach", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 16, firstName: "Rafycxael", lastName: "Kofsach", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 17, firstName: "Racyxfael", lastName: "Kafsdoch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 18, firstName: "Racyxfael", lastName: "Koasfdch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 19, firstName: "Rayx fael", lastName: "Kofsach", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 20, firstName: "Ra fael", lastName: "Kafsoch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 21, firstName: "Raycxfael", lastName: "Kocfsadh", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 22, firstName: "Racyxfael", lastName: "Kofsadch", department: "Admin", education: "EFZ", vacationDaysEntitled: 20 },
  { id: 23, firstName: "Saycxra", lastName: "Frei", department: "Planung", education: "Dipl. Pflegefachfrau HF", vacationDaysEntitled: 25 },
];

const vacationEntries: VacationEntry[] = [
  { id: 1, employeeId: 1, startDate: "2026-07-13", endDate: "2026-07-17", status: "Genehmigt" },
  { id: 2, employeeId: 2, startDate: "2026-07-13", endDate: "2026-07-24", status: "Genehmigt" },
  { id: 3, employeeId: 3, startDate: "2026-07-13", endDate: "2026-07-24", status: "Ausstehend" },
  { id: 4, employeeId: 4, startDate: "2026-08-03", endDate: "2026-08-14", status: "Bezogen" },
  { id: 5, employeeId: 5, startDate: "2026-07-27", endDate: "2026-07-31", status: "Genehmigt" },
  { id: 6, employeeId: 6, startDate: "2026-07-20", endDate: "2026-07-24", status: "Genehmigt" },
  { id: 7, employeeId: 7, startDate: "2026-12-21", endDate: "2026-12-31", status: "Ausstehend" },
  { id: 8, employeeId: 8, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 9, employeeId: 9, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 10, employeeId: 10, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 11, employeeId: 11, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 12, employeeId: 12, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 13, employeeId: 13, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 14, employeeId: 14, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 15, employeeId: 15, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 16, employeeId: 16, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
  { id: 17, employeeId: 17, startDate: "2026-02-02", endDate: "2026-02-03", status: "Abgelehnt" },
];

// ===== Konstanten für das Tagesraster =====

const DAY_COLUMN_WIDTH_PX = 40;
const NAME_COLUMN_WIDTH_PX = 260;
const INITIAL_WINDOW_RADIUS_DAYS = 45; // ca. 1.5 Monate in jede Richtung -> ca. 3 Monate sichtbar
const LOAD_CHUNK_DAYS = 30; // wie viele Tage beim Nachladen am Rand ergänzt werden
const MAX_VISIBLE_DAYS = 180; // Obergrenze, damit die Tabelle nicht unbegrenzt wächst
const SCROLL_EDGE_THRESHOLD_PX = 200;

// Schwellwerte für die Überschneidungs-Heatmap – später über eine Einstellungs-UI konfigurierbar
const OVERLAP_FREE_THRESHOLD = 1;
const OVERLAP_CRITICAL_THRESHOLD = 5;

const COLOR_GREEN: [number, number, number] = [210, 245, 210];
const COLOR_YELLOW: [number, number, number] = [255, 250, 200];
const COLOR_RED: [number, number, number] = [255, 200, 200];

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

// ===== Überschneidungs-Heatmap (grün -> gelb -> rot, fliessender Verlauf) =====

function rgbString(color: [number, number, number]): string {
  return `rgb(${color[0]}, ${color[1]}, ${color[2]})`;
}

function lerpColor(from: [number, number, number], to: [number, number, number], t: number): string {
  const r = Math.round(from[0] + (to[0] - from[0]) * t);
  const g = Math.round(from[1] + (to[1] - from[1]) * t);
  const b = Math.round(from[2] + (to[2] - from[2]) * t);
  return `rgb(${r}, ${g}, ${b})`;
}

function overlapColor(count: number): string {
  if (count <= OVERLAP_FREE_THRESHOLD) return rgbString(COLOR_GREEN);
  if (count >= OVERLAP_CRITICAL_THRESHOLD) return rgbString(COLOR_RED);

  const mid = (OVERLAP_FREE_THRESHOLD + OVERLAP_CRITICAL_THRESHOLD) / 2;
  if (count <= mid) {
    const t = (count - OVERLAP_FREE_THRESHOLD) / (mid - OVERLAP_FREE_THRESHOLD);
    return lerpColor(COLOR_GREEN, COLOR_YELLOW, t);
  }
  const t = (count - mid) / (OVERLAP_CRITICAL_THRESHOLD - mid);
  return lerpColor(COLOR_YELLOW, COLOR_RED, t);
}

// ===== Zustand =====

const centerDate = ref(new Date(2026, 6, 15)); // Mitte Juli 2026, passend zu den Mock-Daten
const scrollHost = ref<HTMLDivElement | null>(null);
const visibleDays = ref<Date[]>(
  buildDayRange(
    addDays(centerDate.value, -INITIAL_WINDOW_RADIUS_DAYS),
    addDays(centerDate.value, INITIAL_WINDOW_RADIUS_DAYS),
  ),
);

const departmentFilter = ref<Department | null>(null);
const educationFilter = ref<Education | null>(null);
const selectedDetail = ref<{ employee: Employee; entry: VacationEntry } | null>(null);

let isLoadingMoreDays = false;

// ===== Abgeleitete Daten =====

const toolbarLabel = computed(() =>
  new Intl.DateTimeFormat("de-CH", { month: "long", year: "numeric" }).format(centerDate.value),
);

const filteredEmployees = computed(() =>
  employees.filter(
    (employee) =>
      (!departmentFilter.value || employee.department === departmentFilter.value) &&
      (!educationFilter.value || employee.education === educationFilter.value),
  ),
);

function entriesForEmployee(employeeId: number): VacationEntry[] {
  return vacationEntries.filter((entry) => entry.employeeId === employeeId);
}

function entryOnDay(employeeId: number, day: Date): VacationEntry | undefined {
  const iso = toISODate(day);
  return entriesForEmployee(employeeId).find((entry) => iso >= entry.startDate && iso <= entry.endDate);
}

function absentCountOnDay(day: Date): number {
  const iso = toISODate(day);
  return filteredEmployees.value.filter((employee) =>
    entriesForEmployee(employee.id).some((entry) => iso >= entry.startDate && iso <= entry.endDate),
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
  return entriesForEmployee(employee.id).reduce(
    (sum, entry) => sum + daysBetweenInclusive(entry.startDate, entry.endDate),
    0,
  );
}

function remainingSymbol(employee: Employee, planned: number): string {
  const remaining = employee.vacationDaysEntitled - planned;
  if (remaining > 0) return "⏳";
  if (remaining === 0) return "✔";
  return "✖";
}

const rows = computed<EmployeeRow[]>(() =>
  filteredEmployees.value.map((employee) => {
    const planned = plannedDaysCount(employee);
    const cells: DayCell[] = visibleDays.value.map((day) => {
      const iso = toISODate(day);
      const entry = entryOnDay(employee.id, day) ?? null;
      return {
        iso,
        color: dayColorByDate.value.get(iso) ?? "",
        icon: entry ? STATUS_ICON[entry.status] : "",
        entry,
      };
    });
    return {
      employee,
      plannedDays: planned,
      remainingSymbol: remainingSymbol(employee, planned),
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

// ===== Nachladen beim Scrollen an den Rand =====
// Beim Einfügen am linken Rand muss scrollLeft um die eingefügte Breite verschoben werden,
// sonst "springt" die Ansicht sichtbar, weil sich die Spalten davor verschieben.

function onScroll() {
  const el = scrollHost.value;
  if (!el || isLoadingMoreDays) return;

  if (el.scrollLeft < SCROLL_EDGE_THRESHOLD_PX) {
    loadEarlierDays();
  } else if (el.scrollWidth - el.scrollLeft - el.clientWidth < SCROLL_EDGE_THRESHOLD_PX) {
    loadLaterDays();
  }
}

function loadEarlierDays() {
  const el = scrollHost.value;
  if (!el) return;
  isLoadingMoreDays = true;

  const firstDay = visibleDays.value[0];
  const newDays = buildDayRange(addDays(firstDay, -LOAD_CHUNK_DAYS), addDays(firstDay, -1));
  visibleDays.value = [...newDays, ...visibleDays.value].slice(0, MAX_VISIBLE_DAYS);

  nextTick(() => {
    el.scrollLeft += newDays.length * DAY_COLUMN_WIDTH_PX;
    isLoadingMoreDays = false;
  });
}

function loadLaterDays() {
  const el = scrollHost.value;
  if (!el) return;
  isLoadingMoreDays = true;

  const lastDay = visibleDays.value[visibleDays.value.length - 1];
  const newDays = buildDayRange(addDays(lastDay, 1), addDays(lastDay, LOAD_CHUNK_DAYS));
  const combined = [...visibleDays.value, ...newDays];
  const overflow = Math.max(0, combined.length - MAX_VISIBLE_DAYS);
  visibleDays.value = combined.slice(overflow);

  nextTick(() => {
    if (overflow > 0) el.scrollLeft -= overflow * DAY_COLUMN_WIDTH_PX;
    isLoadingMoreDays = false;
  });
}

// ===== Detail-Popup =====

function openDetail(employee: Employee, entry: VacationEntry) {
  selectedDetail.value = { employee, entry };
}

const detailDialogOpen = computed({
  get: () => selectedDetail.value !== null,
  set: (open: boolean) => {
    if (!open) selectedDetail.value = null;
  },
});

onMounted(() => {
  nextTick(() => centerScroll());
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
  overflow-x: auto;
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
</style>
