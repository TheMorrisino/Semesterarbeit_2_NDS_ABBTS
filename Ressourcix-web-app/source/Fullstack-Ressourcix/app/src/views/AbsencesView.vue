<template>
  <div>
    <v-card>
      <v-data-table
        :headers="headers"
        :items="ownRequests"
        :loading="requestStore.loading || employeeStore.loading"
        item-value="id"
      >
        <template #item.employee="{ item }">
          {{ employeeName(item.employeeId) }}
        </template>

        <template #item.status="{ item }">
          <v-chip :color="statusColor(item.status)" size="small" variant="tonal">
            {{ t(`status.${item.status.toLowerCase()}`) }}
          </v-chip>
        </template>

        <template #item.action="{ item }">
          <v-btn
            size="small"
            variant="outlined"
            @click="openDetails(item)"
          >
            {{ t('employee.details') }}
          </v-btn>
        </template>
      </v-data-table>

      <v-dialog v-model="detailsDialog" max-width="480">
        <v-card v-if="selectedItem" :title="t('absences.detailsDialog')">
          <v-card-text>
            <div class="detail-row">
              <span class="detail-label">{{ t('common.name') }}</span>
              <span>{{ employeeName(selectedItem.employeeId) }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">{{ t('absences.typ') }}</span>
              <span>{{ selectedItem.type }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">{{ t('absences.by') }}</span>
              <span>{{ selectedItem.from }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">{{ t('absences.to') }}</span>
              <span>{{ selectedItem.until }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">{{ t('absences.day') }}</span>
              <span>{{ selectedItem.days }}</span>
            </div>
            <div class="detail-row">
              <span class="detail-label">{{ t('absences.status') }}</span>
              <v-chip :color="statusColor(selectedItem.status)" size="small" variant="tonal">
                {{ t(`status.${selectedItem.status.toLowerCase()}`) }}
              </v-chip>
            </div>
            <v-divider class="my-3" />
            <div class="detail-label mb-1">{{ t('absences.note') }}</div>
            <div>{{ selectedItem.remark || '–' }}</div>
          </v-card-text>
          <v-card-actions>
            <v-spacer />
            <v-btn variant="text" @click="detailsDialog = false">{{ t('common.close') }}</v-btn>
          </v-card-actions>
        </v-card>
      </v-dialog>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from "vue";
import { useI18n } from "vue-i18n";
import { computed } from "vue";
import { useEmployeeStore } from "@/stores/employee";
import { useRequestStore, RequestStatus, type Request } from "@/stores/request";
import { useAuthStore } from "@/stores/auth";

const { t } = useI18n();
const employeeStore = useEmployeeStore();
const requestStore = useRequestStore();
const authStore = useAuthStore();

// Beide Rollen sehen hier nur ihre eigenen Abwesenheiten (BR: Abwesenheitsview).
const ownRequests = computed(() =>
  requestStore.requests.filter((r) => r.employeeId === authStore.user?.id),
);

const headers = [
  { title: t('common.name'), key: 'employee' },
  { title: t('absences.typ'), key: 'type' },
  { title: t('absences.by'), key: 'from' },
  { title: t('absences.to'), key: 'until' },
  { title: t('absences.day'), key: 'days' },
  { title: t('absences.status'), key: 'status' },
  { title: t('absences.note'), key: 'action', sortable: false },
];

function employeeName(employeeId: string): string {
  return employeeStore.employees.find((e) => e.id === employeeId)?.name ?? t('approval.unknown');
}

function statusColor(status: RequestStatus) {
  const map: Record<RequestStatus, string> = {
    [RequestStatus.Open]: 'orange',
    [RequestStatus.Approved]: 'green',
    [RequestStatus.Taken]: 'blue',
    [RequestStatus.Rejected]: 'red',
    [RequestStatus.Cancelled]: 'grey',
  };
  return map[status] ?? 'grey';
}

const detailsDialog = ref(false);
const selectedItem = ref<Request | null>(null);

function openDetails(item: Request) {
  selectedItem.value = item;
  detailsDialog.value = true;
}

onMounted(async () => {
  await Promise.all([employeeStore.load(), requestStore.load()]);
});
</script>

<style scoped>
.detail-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 4px 0;
}

.detail-label {
  
  text-transform: uppercase;
  letter-spacing: 0.03em;
  opacity: 0.6;
}
</style>
