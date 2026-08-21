<template>
  <div>
    <div class="d-flex flex-wrap align-center justify-space-between ga-3 mb-4">
      <div>
        <h1 class="dash-heading">{{ t("dashboard.greeting", { name: currentEmployee?.name ?? "–" }) }}</h1>
        <div class="dash-subtext">{{ todayLabel }}</div>
      </div>

      <v-btn color="primary" prepend-icon="mdi-plus" to="/calender">{{ t("dashboard.newRequest") }}</v-btn>
    </div>

    <!-- Ferien-Statuszeile: bezieht sich auf der eingelogte Mitarbeiter (erster in der Liste) -->
    <v-row class="mb-1" dense>
      <v-col cols="6" sm="3">
        <v-card>
          <v-card-text>
            <div class="stat-label">{{ t("teamview.entitlement") }}</div>
            <div class="stat-value">{{ entitledDays }}<small>{{ t("absences.day") }}</small></div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="3">
        <v-card>
          <v-card-text>
            <div class="stat-label">{{ t("teamview.taken") }}</div>
            <div class="stat-value">{{ takenDays }}<small>{{ t("absences.day") }}</small></div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="3">
        <v-card>
          <v-card-text>
            <div class="stat-label">{{ t("teamview.planned") }}</div>
            <div class="stat-value">{{ plannedDays }}<small>{{ t("absences.day") }}</small></div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="6" sm="3">
        <v-card>
          <v-card-text>
            <div class="stat-label">{{ t("teamview.remaining") }}</div>
            <div class="stat-value stat-value--accent">{{ remainingDays }}<small>{{ t("absences.day") }}</small></div>
          </v-card-text>
        </v-card>
      </v-col>
    </v-row>

    <v-row class="mb-1" dense>
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title>{{ t("dashboard.teamThisWeek") }}</v-card-title>

          <v-card-text>
            <div class="week-strip">
              <div
                v-for="day in weekDays"
                :key="day.iso"
                class="day-cell"
                :class="{ 'day-cell--weekend': day.isWeekend }"
                :style="{ backgroundColor: day.color }"
              >
                <div class="dow">{{ day.dow }}</div>
                <div class="dom">{{ day.dom }}</div>
                <div class="cnt">{{ day.isWeekend ? "–" : `${day.count} ${t("dashboard.absentToday")}` }}</div>
              </div>
            </div>
          </v-card-text>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card>
          <v-card-title class="d-flex justify-space-between align-center">
            {{ t("dashboard.openApprovals") }}
            <v-chip v-if="openApprovals.length > 0" color="warning" size="small" variant="tonal">{{ openApprovals.length }}</v-chip>
          </v-card-title>

          <v-list v-if="openApprovals.length > 0" density="compact" class="approvals-list">
            <v-list-item v-for="request in openApprovals" :key="request.id">
              <v-list-item-title class="item-title">{{ employeeName(request.employeeId) }}</v-list-item-title>

              <v-list-item-subtitle class="item-sub">
                {{ formatDate(request.from) }} – {{ formatDate(request.until) }} &middot; {{ request.days }} {{ t("absences.day") }}
              </v-list-item-subtitle>

              <template #append>
                <v-chip v-if="request.overlap" color="error" size="x-small" variant="tonal">{{ t("approval.overlaps") }}</v-chip>
              </template>
            </v-list-item>
          </v-list>

          <div v-else class="list-empty">{{ t("dashboard.noApprovals") }}</div>
        </v-card>
      </v-col>
    </v-row>

    <v-card class="mb-4">
      <v-card-title>{{ t("dashboard.overlapChart") }}</v-card-title>
      <v-card-subtitle class="chart-hint">{{ t("dashboard.overlapChartHint") }}</v-card-subtitle>

      <v-card-text>
        <OverlapChart :points="overlapChartPoints" />
      </v-card-text>
    </v-card>

    <v-row dense>
      <v-col cols="12" md="6">
        <v-card>
          <v-card-title class="d-flex justify-space-between align-center">
            {{ t("dashboard.recentRequests") }}
            <RouterLink class="view-all-link" to="/absences">{{ t("dashboard.viewAll") }} →</RouterLink>
          </v-card-title>

          <v-list v-if="recentRequests.length > 0" density="compact">
            <v-list-item v-for="request in recentRequests" :key="request.id">
              <v-list-item-title class="item-title">{{ employeeName(request.employeeId) }}</v-list-item-title>
              <v-list-item-subtitle class="item-sub">{{ formatDate(request.from) }} – {{ formatDate(request.until) }}</v-list-item-subtitle>

              <template #append>
                <v-chip :color="statusMeta(request.status, t).color" size="small" variant="tonal">{{ statusMeta(request.status, t).label }}</v-chip>
              </template>
            </v-list-item>
          </v-list>

          <div v-else class="list-empty">{{ t("dashboard.noRequests") }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" md="6">
        <v-card>
          <v-card-title class="d-flex justify-space-between align-center">
            {{ t("dashboard.recentActivity") }}
            <RouterLink class="view-all-link" to="/auditlog">{{ t("dashboard.viewAll") }} →</RouterLink>
          </v-card-title>

          <v-list v-if="recentActivity.length > 0" density="compact">
            <v-list-item v-for="entry in recentActivity" :key="entry.id">
              <template #prepend>
                <v-avatar :color="auditLogActionMeta[entry.action].bg" rounded="lg" size="30">
                  <v-icon :color="auditLogActionMeta[entry.action].color" :icon="auditLogActionMeta[entry.action].icon" size="16" />
                </v-avatar>
              </template>

              <v-list-item-title class="item-title">{{ entry.summary }}</v-list-item-title>
              <v-list-item-subtitle class="item-sub">{{ entry.actor }} &middot; {{ timeAgo(entry.timestamp) }}</v-list-item-subtitle>
            </v-list-item>
          </v-list>

          <div v-else class="list-empty">{{ t("dashboard.noActivity") }}</div>
        </v-card>
      </v-col>
    </v-row>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted } from 'vue'
  import { useI18n } from 'vue-i18n'
  import OverlapChart, { type OverlapChartPoint } from '@/components/OverlapChart.vue'
  import { useEmployeeName } from '@/composables/useEmployeeName'
  import { auditLogActionMeta, useAuditLogStore } from '@/stores/auditLog'
  import { useAuthStore } from '@/stores/auth'
  import { useEmployeeStore } from '@/stores/employee'
  import { RequestStatus, useRequestStore } from '@/stores/request'
  import { daysBetweenInclusive, formatDate, startOfWeek, toISODate } from '@/utils/date'
  import { isEmployeeAbsentOn } from '@/utils/overlap'
  import { overlapColor } from '@/utils/overlapHeatmap'
  import { statusMeta } from '@/utils/statusMeta'
  import { weekdayLabels } from '@/utils/weekday'

  const { t } = useI18n()
  const employeeStore = useEmployeeStore()
  const requestStore = useRequestStore()
  const auditLog = useAuditLogStore()

  const WEEKDAY_LABELS = computed(() => weekdayLabels(t))
  const OVERLAP_CHART_DAYS = 14
  const authStore = useAuthStore()

  // ===== Datumshilfsfunktionen =====

  function timeAgo (iso: string): string {
    const diffMinutes = Math.round((Date.now() - new Date(iso).getTime()) / 60_000)
    if (diffMinutes < 1) return t('time.justNow')
    if (diffMinutes < 60) return t('time.minutesAgo', { n: diffMinutes })
    const diffHours = Math.round(diffMinutes / 60)
    if (diffHours < 24) return t('time.hoursAgo', { n: diffHours })
    const diffDays = Math.round(diffHours / 24)
    return diffDays === 1 ? t('time.oneDayAgo') : t('time.daysAgo', { n: diffDays })
  }

  // ===== Überschneidungen: wie viele Mitarbeitende sind an einem Tag gleichzeitig abwesend =====

  function absentCountOnDay (iso: string): number {
    return employeeStore.employees.filter(employee =>
      isEmployeeAbsentOn(requestStore.requests, employee.id, iso),
    ).length
  }

  // ===== Ferien-Statuszeile (Platzhalter-Mitarbeitender, siehe Kommentar oben im Template) =====

  const currentEmployee = computed(() => employeeStore.employees.find(e => e.name === authStore.user?.name) ?? null)

  const entitledDays = computed(() => (currentEmployee.value ? Math.round(currentEmployee.value.vacationDays) : 0))

  const ownRequests = computed(() =>
    currentEmployee.value ? requestStore.requests.filter(r => r.employeeId === currentEmployee.value!.id) : [],
  )

  const takenDays = computed(() =>
    ownRequests.value
      .filter(r => r.status === RequestStatus.Taken)
      .reduce((sum, r) => sum + daysBetweenInclusive(r.from, r.until), 0),
  )

  const plannedDays = computed(() =>
    ownRequests.value
      .filter(r => r.status === RequestStatus.Open || r.status === RequestStatus.Approved)
      .reduce((sum, r) => sum + daysBetweenInclusive(r.from, r.until), 0),
  )

  const remainingDays = computed(() => entitledDays.value - takenDays.value - plannedDays.value)

  // ===== Team diese Woche =====

  const weekDays = computed(() => {
    const start = startOfWeek(new Date())
    return Array.from({ length: 7 }, (_, i) => {
      const date = new Date(start)
      date.setDate(date.getDate() + i)
      const iso = toISODate(date)
      const count = absentCountOnDay(iso)
      const dayOfWeek = date.getDay()
      return {
        iso,
        dow: WEEKDAY_LABELS.value[dayOfWeek],
        dom: date.getDate(),
        count,
        isWeekend: dayOfWeek === 0 || dayOfWeek === 6,
        color: overlapColor(count),
      }
    })
  })

  // ===== Überschneidungsdiagramm: nächste 14 Tage =====

  const overlapChartPoints = computed<OverlapChartPoint[]>(() => {
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    return Array.from({ length: OVERLAP_CHART_DAYS }, (_, i) => {
      const date = new Date(today)
      date.setDate(date.getDate() + i)
      const iso = toISODate(date)
      const dayOfWeek = date.getDay()
      return {
        iso,
        label: date.toLocaleDateString('de-CH', { day: '2-digit', month: '2-digit' }),
        value: absentCountOnDay(iso),
        isWeekend: dayOfWeek === 0 || dayOfWeek === 6,
      }
    })
  })

  // ===== Listen =====

  const openApprovals = computed(() => requestStore.requests.filter(r => r.status === RequestStatus.Open),)

  const recentRequests = computed(() => {
    // toSorted() bräuchte lib ES2023 (Projekt-Ziel ist ES2022, siehe tsconfig) - Kopie + sort() genügt hier.
    // eslint-disable-next-line unicorn/no-array-sort
    const sorted = [...requestStore.requests].sort(
      (a, b) => new Date(b.submittedOn).getTime() - new Date(a.submittedOn).getTime(),
    )
    return sorted.slice(0, 5)
  })

  const recentActivity = computed(() => auditLog.entries.slice(0, 5))

  const { employeeName } = useEmployeeName()

  const todayLabel = computed(() =>
    new Intl.DateTimeFormat('de-CH', { weekday: 'long', day: '2-digit', month: 'long', year: 'numeric' }).format(new Date()),
  )

  onMounted(() => {
    // ohne Status-Filter laden, da sowohl die Wochenübersicht als auch das Diagramm
    // alle Anträge (nicht nur offene) für die Überschneidungs-Zählung brauchen
    Promise.all([employeeStore.load(), requestStore.load(), auditLog.load()])
  })
</script>

<style scoped>
.dash-heading {
  margin: 0 0 2px;
  font-size: 21px;
  font-weight: 700;
  letter-spacing: -0.01em;
}

.dash-subtext {
  font-size: 12.5px;
  opacity: 0.6;
}

.stat-label {
  font-size: 11px;
  text-transform: uppercase;
  letter-spacing: 0.05em;
  opacity: 0.6;
  font-weight: 600;
}

.stat-value {
  font-family: "Roboto Mono", "SFMono-Regular", Consolas, monospace;
  font-size: 24px;
  font-weight: 700;
  margin-top: 5px;
  letter-spacing: -0.01em;
}

.stat-value small {
  font-family: "Roboto", "Segoe UI", sans-serif;
  font-size: 13px;
  font-weight: 500;
  opacity: 0.6;
  margin-left: 4px;
}

.stat-value--accent {
  color: rgb(var(--v-theme-primary));
}

.chart-hint {
  opacity: 0.65;
  font-size: 12px;
}

.week-strip {
  display: grid;
  grid-template-columns: repeat(7, 1fr);
  gap: 8px;
}

.day-cell {
  border-radius: 8px;
  padding: 10px 6px 8px;
  text-align: center;
  color: #0f4c4f;
}

.day-cell .dow {
  font-size: 10px;
  text-transform: uppercase;
  letter-spacing: 0.03em;
  opacity: 0.6;
}

.day-cell .dom {
  font-family: "Roboto Mono", "SFMono-Regular", Consolas, monospace;
  font-size: 15px;
  font-weight: 700;
  margin: 3px 0 4px;
}

.day-cell .cnt {
  font-family: "Roboto Mono", "SFMono-Regular", Consolas, monospace;
  font-size: 10px;
  opacity: 0.8;
}

.day-cell--weekend {
  opacity: 0.55;
}

.item-title {
  font-size: 13px;
  font-weight: 600;
}

.item-sub {
  font-size: 11.5px;
  opacity: 0.65;
  margin-top: 2px;
}

.list-empty {
  padding: 26px 16px;
  text-align: center;
  font-size: 12.5px;
  opacity: 0.6;
}

.view-all-link {
  font-size: 12px;
  font-weight: 600;
  color: rgb(var(--v-theme-primary));
  text-decoration: none;
}

.view-all-link:hover {
  text-decoration: underline;
}
.approvals-list {
  max-height: 106px;
  overflow-y: auto;
}
</style>
