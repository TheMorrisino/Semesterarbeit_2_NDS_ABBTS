<template>
  <div>
    <v-card>
      <!-- Toolbar: Jahr-/Monat-/Wochen-Navigation und Filter nach Abteilung/Ausbildung -->
      <div class="d-flex flex-wrap align-center ga-2 pa-4">
        <div class="d-flex align-center ga-1">
          <v-btn
            icon="mdi-chevron-double-left"
            size="small"
            :title="t('calendar.jumpYearBack')"
            variant="text"
            @click="jumpYears(-1)"
          />

          <v-btn
            icon="mdi-chevron-left"
            size="small"
            :title="t('calendar.jumpMonthBack')"
            variant="text"
            @click="jumpMonths(-1)"
          />

          <v-btn
            icon="mdi-menu-left"
            size="small"
            :title="t('calendar.jumpWeekBack')"
            variant="text"
            @click="jumpWeeks(-1)"
          />

          <span class="text-subtitle-1 font-weight-medium mx-2 toolbar-label">{{ toolbarLabel }}</span>

          <v-btn
            icon="mdi-menu-right"
            size="small"
            :title="t('calendar.jumpWeekForward')"
            variant="text"
            @click="jumpWeeks(1)"
          />

          <v-btn
            icon="mdi-chevron-right"
            size="small"
            :title="t('calendar.jumpMonthForward')"
            variant="text"
            @click="jumpMonths(1)"
          />

          <v-btn
            icon="mdi-chevron-double-right"
            size="small"
            :title="t('calendar.jumpYearForward')"
            variant="text"
            @click="jumpYears(1)"
          />
        </div>

        <v-spacer />

      </div>

      <!-- Legende -->
      <div class="d-flex flex-wrap ga-4 px-4 pb-3 text-caption text-medium-emphasis">
        <span>🕒 {{ t('status.open') }}</span>
        <span>✅ {{ t('status.approved') }}</span>
        <span>❌ {{ t('status.rejected') }}</span>
        <span>🏖 {{ t('status.taken') }}</span>
        <span>🚫 {{ t('status.cancelled') }}</span>
        <span>⏳ {{ t('calendar.legendDaysRemaining') }}</span>
        <span>✔ {{ t('calendar.legendDaysUsedUp') }}</span>
        <span>✖ {{ t('calendar.legendDaysExceeded') }}</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--weekend" />{{ t('calendar.legendWeekend') }}</span>
        <span class="d-flex align-center ga-1"><span class="legend-swatch legend-swatch--overlap" />{{ t('calendar.legendOverlap') }}</span>
      </div>

      <!-- Tagesraster: Mitarbeiter als Zeilen, Kalendertage als scrollbare Spalten -->
      <div ref="scrollHost" class="calendar-scroll">
        <table class="calendar-table">
          <thead>
            <tr>
              <th class="name-col">{{ xs ? t('calendar.columnHeaderMobile') : t('calendar.columnHeaderDesktop') }}</th>

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
            <tr v-for="row in rows" :key="row.employee.id" :class="{ 'is-inactive-employee': !row.employee.isActive }">
              <td class="name-col" :title="xs ? row.employee.name : undefined">
                <template v-if="xs">
                  {{ row.label }}
                </template>

                <template v-else>
                  {{ row.employee.name }}
                  <span v-if="!row.employee.isActive" class="text-caption">({{ t('employee.isActiveDisabledShort') }})</span>

                  <span class="text-caption text-medium-emphasis">
                    [{{ row.plannedDays }}/{{ row.entitledDays }}] {{ row.remainingSymbol }}
                  </span>
                </template>
              </td>

              <td
                v-for="cell in row.cells"
                :key="cell.iso"
                class="clickable"
                :style="{ backgroundColor: cell.color }"
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
          {{ entryDialog.mode === "create" ? t('calendar.dialogTitleCreate') : t('calendar.dialogTitleEdit') }}
          – {{ entryDialog.employee.name }}
        </v-card-title>

        <v-card-text>
          <div class="mb-3 text-body-2">
            {{ t('calendar.start') }} <strong>{{ formatDate(entryDialog.startDate) }}</strong>
          </div>

          <v-text-field
            v-model="entryDialog.endDate"
            data-testid="request-end-date"
            density="compact"
            :disabled="entryDialog.status !== RequestStatus.Open"
            :label="t('calendar.endDateLabel')"
            :min="entryDialog.startDate"
            type="date"
          />

          <v-text-field
            v-if="entryDialog.mode === 'create'"
            v-model="entryDialog.remark"
            data-testid="calendar-remark"
            density="compact"
            :label="t('calendar.remarkLabel')"
          />

          <v-select
            v-if="entryDialog.mode === 'edit'"
            v-model="entryDialog.status"
            density="compact"
            :disabled="!authStore.isAdmin"
            :hint="!authStore.isAdmin ? t('calendar.statusHint') : undefined"
            item-title="title"
            item-value="value"
            :items="statusOptions"
            :label="t('absences.status')"
            persistent-hint
          />

          <div v-if="dialogForeignEmployeeError" class="text-error text-body-2 mt-1">{{ dialogForeignEmployeeError }}</div>
          <div v-if="dialogEndDateError" class="text-error text-body-2 mt-1">{{ dialogEndDateError }}</div>
          <div v-if="dialogStatusError" class="text-error text-body-2 mt-1">{{ dialogStatusError }}</div>

          <!-- informativer Hinweis, Überschneidungen mit Kollegen blockieren das Speichern nicht (BR-01.04) -->
          <div v-if="overlappingColleagues.length > 0" class="overlap-warning mt-3">
            <strong>{{ t('calendar.overlapDetected') }}</strong>
            {{ overlappingColleagues.join(", ") }}
            {{ t('calendar.overlapSentence', overlappingColleagues.length) }}
          </div>
        </v-card-text>

        <v-card-actions>
          <v-btn
            v-if="entryDialog.mode === 'edit'"
            color="error"
            :disabled="!!dialogForeignEmployeeError"
            variant="text"
            @click="deleteEntry"
          >{{ t('common.delete') }}</v-btn>

          <v-spacer />
          <v-btn variant="text" @click="entryDialogOpen = false">{{ t('common.cancel') }}</v-btn>

          <v-btn
            variant="tonal"
            color="primary"
            data-testid="calendar-save-entry"
            :disabled="!!dialogEndDateError || !!dialogForeignEmployeeError"
            @click="saveEntry"
          >
            {{ t('common.save') }}
          </v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useI18n } from 'vue-i18n'
  import { useRoute, useRouter } from 'vue-router'
  import { useDisplay } from 'vuetify'
  import { useEntryDialog } from '@/composables/useEntryDialog'
  import { useVisibleDayCount } from '@/composables/useVisibleDayCount'
  import { useAuthStore } from '@/stores/auth'
  import { type Employee, useEmployeeStore } from '@/stores/employee'
  import { type Request, RequestStatus, useRequestStore } from '@/stores/request'
  import { addDays, addMonths, addYears, buildDayRange, daysBetweenInclusive, formatDate, isWeekend, toISODate } from '@/utils/date'
  import { uniqueInitials } from '@/utils/initials'
  import { isEmployeeAbsentOn } from '@/utils/overlap'
  import { overlapColor } from '@/utils/overlapHeatmap'
  import { statusMeta } from '@/utils/statusMeta'
  import { weekdayLabels } from '@/utils/weekday'

  const { t } = useI18n()

  // ===== Domain-Typen (Employee/Request kommen aus den Stores, hier nur UI-lokaler Zustand) =====

  interface DayCell {
    iso: string
    color: string
    icon: string
    entry: Request | null
  }

  interface EmployeeRow {
    employee: Employee
    label: string
    plannedDays: number
    entitledDays: number
    remainingSymbol: string
    cells: DayCell[]
  }

  const statusOptions = computed(() =>
    Object.values(RequestStatus).map(value => ({ title: statusMeta(value, t).label, value })),
  )

  // ===== Konstanten für das Tagesraster =====

  const DAY_COLUMN_WIDTH_PX = 40
  const NAME_COLUMN_WIDTH_DESKTOP_PX = 260
  const NAME_COLUMN_WIDTH_MOBILE_PX = 56 // nur Platz für das Kürzel (siehe uniqueInitials)
  const FALLBACK_VISIBLE_DAY_COUNT = 30 // Platzhalter, bis der Container einmal vermessen wurde

  const WEEKDAY_LABELS = computed(() => weekdayLabels(t))

  // ===== Datumshilfsfunktionen =====

  function weekdayLabel (date: Date): string {
    return WEEKDAY_LABELS.value[date.getDay()]
  }

  // ===== Zustand =====

  const route = useRoute()
  const router = useRouter()
  const employeeStore = useEmployeeStore()
  const requestStore = useRequestStore()
  const authStore = useAuthStore()

  // Bewusst Vuetifys "xs"-Breakpoint (< 600px, echte Handy-Breite) statt des generischen "mobile"-Flags
  // (das standardmässig schon ab < 1280px greift, also auch Tablets/kleine Laptops erfassen würde).
  // Unterhalb davon zeigen wir nur noch das Kürzel statt des vollen Namens, damit mehr Platz für Tagesspalten bleibt.
  const { xs } = useDisplay()
  const nameColumnWidthPx = computed(() => (xs.value ? NAME_COLUMN_WIDTH_MOBILE_PX : NAME_COLUMN_WIDTH_DESKTOP_PX))

  const centerDate = ref(new Date()) // Mitte Juli 2026, passend zu den Seed-Daten des Backends

  const { scrollHost, visibleDayCount } = useVisibleDayCount(nameColumnWidthPx, DAY_COLUMN_WIDTH_PX, FALLBACK_VISIBLE_DAY_COUNT)

  const visibleDays = computed<Date[]>(() =>
    buildDayRange(centerDate.value, addDays(centerDate.value, visibleDayCount.value - 1)),
  )

  const {
    entryDialog,
    entryDialogOpen,
    openEditDialog,
    handleCellClick,
    dialogEndDateError,
    dialogStatusError,
    dialogForeignEmployeeError,
    overlappingColleagues,
    saveEntry,
    deleteEntry,
  } = useEntryDialog()

  // ===== Abgeleitete Daten =====

  const toolbarLabel = computed(() =>
    new Intl.DateTimeFormat('de-CH', { month: 'long', year: 'numeric' }).format(centerDate.value),
  )

  const filteredEmployees = computed(() => employeeStore.employees)

  // Kürzel je Mitarbeiter für die Mobile-Ansicht, konsistent mit der Mitarbeiterverwaltung (initialsFor);
  // bei Kollisionen wird ab der zweiten Person eine Zahl angehängt (siehe uniqueInitials).
  const employeeLabels = computed(() => uniqueInitials(filteredEmployees.value))

  function requestsForEmployee (employeeId: string): Request[] {
    return requestStore.requests.filter(request => request.employeeId === employeeId)
  }

  function requestOnDay (employeeId: string, day: Date): Request | undefined {
    const iso = toISODate(day)
    return requestsForEmployee(employeeId).find(request => iso >= request.from && iso <= request.until)
  }

  function absentCountOnDay (day: Date): number {
    const iso = toISODate(day)
    return filteredEmployees.value.filter(employee =>
      isEmployeeAbsentOn(requestStore.requests, employee.id, iso),
    ).length
  }

  // Heatmap-Farbe pro Tag einmal pro Spalte berechnen, statt pro Zelle x Mitarbeiter
  const dayColorByDate = computed(() => {
    const colors = new Map<string, string>()
    for (const day of visibleDays.value) {
      colors.set(toISODate(day), overlapColor(absentCountOnDay(day)))
    }
    return colors
  })

  function plannedDaysCount (employee: Employee): number {
    return requestsForEmployee(employee.id).reduce(
      (sum, request) => sum + daysBetweenInclusive(request.from, request.until),
      0,
    )
  }

  function remainingSymbol (entitledDays: number, plannedDays: number): string {
    const remaining = entitledDays - plannedDays
    if (remaining > 0) return '⏳'
    if (remaining === 0) return '✔'
    return '✖'
  }

  const rows = computed<EmployeeRow[]>(() =>
    filteredEmployees.value.map(employee => {
      const planned = plannedDaysCount(employee)
      const entitled = employee.vacationDays
      const cells: DayCell[] = visibleDays.value.map(day => {
        const iso = toISODate(day)
        const request = requestOnDay(employee.id, day) ?? null
        return {
          iso,
          color: dayColorByDate.value.get(iso) ?? '',
          icon: request ? statusMeta(request.status, t).icon : '',
          entry: request,
        }
      })
      return {
        employee,
        label: employeeLabels.value[employee.id] ?? '',
        plannedDays: planned,
        entitledDays: entitled,
        remainingSymbol: remainingSymbol(entitled, planned),
        cells,
      }
    }),
  )

  // ===== Navigation (Jahr/Monat/Woche) =====

  function jumpTo (newCenter: Date) {
    centerDate.value = newCenter
  }

  function jumpYears (delta: number) {
    jumpTo(addYears(centerDate.value, delta))
  }

  function jumpMonths (delta: number) {
    jumpTo(addMonths(centerDate.value, delta))
  }

  function jumpWeeks (delta: number) {
    jumpTo(addDays(centerDate.value, delta * 7))
  }

  // ===== Antrag stellen / bearbeiten (Dialog-Zustand + Validierung siehe useEntryDialog) =====

  // Springt zu einem per Query-Param übergebenen Antrag (z.B. von der Genehmigungen-Seite aus)
  function jumpToRequest (requestId: string) {
    const request = requestStore.requests.find(r => r.id === requestId)
    const employee = request ? employeeStore.employees.find(e => e.id === request.employeeId) : undefined
    if (!request || !employee) return

    jumpTo(new Date(request.from))
    openEditDialog(employee, request)
    router.replace({ query: {} })
  }

  onMounted(async () => {
    await Promise.all([employeeStore.load(), requestStore.load()])

    const requestId = route.query.requestId
    if (typeof requestId === 'string') {
      jumpToRequest(requestId)
    }
  })
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
  /* hidden statt auto: es wird nie mehr generiert, als in den Container passt (siehe updateVisibleDayCount),
     Navigation läuft ausschliesslich über die Pfeile in der Toolbar (jumpYears/jumpMonths/jumpWeeks). */
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
  min-width: v-bind(nameColumnWidthPx + "px");
  max-width: v-bind(nameColumnWidthPx + "px");
  width: v-bind(nameColumnWidthPx + "px");
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

.is-inactive-employee {
  opacity: 0.45;
  filter: grayscale(1);
}

.overlap-warning {
  background: rgba(239, 159, 39, 0.15);
  color: rgb(133, 79, 11);
  border-radius: 8px;
  padding: 10px 12px;
  font-size: 13px;
}
</style>
