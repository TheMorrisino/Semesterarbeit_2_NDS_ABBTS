<template>
  <div>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-clipboard-check" class="mr-2" />{{ t('approval.openRequest') }}</span>
        <v-chip color="warning" size="small" variant="tonal">
          {{ requestStore.requests.length }} {{ t('approval.pending') }}
        </v-chip>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="requestStore.requests"
        :loading="requestStore.loading || employeeStore.loading"
        item-value="id"
      >
        <template #item.employee="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(employeeName(item.employeeId))">
              <span class="text-caption">{{ initials(employeeName(item.employeeId)) }}</span>
            </v-avatar>
            {{ employeeName(item.employeeId) }}
          </div>
        </template>

        <template #item.period="{ item }">
          {{ formatDate(item.from) }} – {{ formatDate(item.until) }}
        </template>

        <template #item.hint="{ item }">
          <v-chip :color="item.overlap ? 'error' : 'success'" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.overlap ? t('approval.overlaps') : t('approval.none') }}
          </v-chip>
        </template>

        <template #item.submittedOn="{ item }">
          {{ daysAgo(item.submittedOn) }}
        </template>

        <template #item.decision="{ item }">
          <v-btn
            size="small"
            variant="tonal"
            color="primary"
            prepend-icon="mdi-calendar-arrow-right"
            @click="goToCalendarEntry(item)"
          >
            {{ t('approval.toCalendar') }}
          </v-btn>
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useRouter } from "vue-router";
import { useEmployeeStore } from "@/stores/employee";
import { useRequestStore, type Request } from "@/stores/request";

const { t } = useI18n();
const router = useRouter();
const employeeStore = useEmployeeStore();
const requestStore = useRequestStore();

const headers = [
  { title: t('approval.employee'), key: 'employee' },
  { title: t('approval.period'), key: 'period', sortable: false },
  { title: t('approval.day'), key: 'days' },
  { title: t('approval.note'), key: 'hint' },
  { title: t('approval.tabled'), key: 'submittedOn' },
  { title: t('approval.decision'), key: 'decision', sortable: false },
];

function employeeName(employeeId: string): string {
  return employeeStore.employees.find((e) => e.id === employeeId)?.name ?? t('approval.unknown');
}

function initials(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase();
}

function avatarColor(name: string) {
  const colors = ['purple', 'red', 'blue', 'teal', 'indigo'];
  return colors[name.length % colors.length];
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString('de-CH', { day: '2-digit', month: '2-digit' });
}

function daysAgo(iso: string) {
  const days = Math.round((Date.now() - new Date(iso).getTime()) / 86_400_000);
  if (days <= 0) return t('approval.today');
  return days === 1 ? t('approval.before1day') : t('approval.ForwardNTagen', { n: days });
}

function goToCalendarEntry(item: Request) {
  router.push({ path: '/calender', query: { requestId: item.id } });
}

onMounted(async () => {
  await Promise.all([employeeStore.load(), requestStore.load('open')]);
});
</script>
