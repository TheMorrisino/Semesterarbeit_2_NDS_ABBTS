<template>
  <div>
    <v-card>
      <v-data-table
        :headers="headers"
        :items="requestStore.requests"
        :loading="requestStore.loading || employeeStore.loading"
        item-value="id"
      >
        <template #item.employee="{ item }">
          {{ employeeName(item.employeeId) }}
        </template>

        <template #item.status="{ item }">
          <v-chip :color="statusColor(item.status)" size="small" variant="tonal">
            {{ item.status }}
          </v-chip>
        </template>

        <template #item.action="{ item }">
          <v-btn
            size="small"
            variant="outlined"
            :disabled="item.status === RequestStatus.Taken"
            @click="onAction(item)"
          >
            {{ actionLabel(item.status) }}
          </v-btn>
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { useI18n } from "vue-i18n";
import { useEmployeeStore } from "@/stores/employee";
import { useRequestStore, RequestStatus, type Request } from "@/stores/request";
import { useAuditLogStore } from "@/stores/auditLog";

const { t } = useI18n();
const employeeStore = useEmployeeStore();
const requestStore = useRequestStore();
const auditLog = useAuditLogStore();

// Es gibt noch kein Login -> es werden bewusst alle Anträge gezeigt, nicht nur "eigene".
// Sobald ein echter Login existiert, kann hier nach dem eingeloggten Mitarbeitenden gefiltert werden.

const headers = [
  { title: t('common.name'), key: 'employee' },
  { title: t('absences.typ'), key: 'type' },
  { title: t('absences.by'), key: 'from' },
  { title: t('absences.to'), key: 'until' },
  { title: t('absences.day'), key: 'days' },
  { title: t('absences.status'), key: 'status' },
  { title: t('absences.aktion'), key: 'action', sortable: false },
];

function employeeName(employeeId: string): string {
  return employeeStore.employees.find((e) => e.id === employeeId)?.name ?? t('approval.unbekannt');
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

function actionLabel(status: RequestStatus) {
  const map: Record<RequestStatus, string> = {
    [RequestStatus.Open]: 'Details',
    [RequestStatus.Approved]: 'Stornieren',
    [RequestStatus.Taken]: '—',
    [RequestStatus.Rejected]: 'Grund',
    [RequestStatus.Cancelled]: '—',
  };
  return map[status] ?? '';
}

async function onAction(item: Request) {
  if (item.status === RequestStatus.Approved) {
    await requestStore.update(item.id, item.until, RequestStatus.Cancelled);
    await auditLog.log('RequestUpdated', 'Ferienantrag storniert', item.id);
    return;
  }
  // Details-Dialog (Ausstehend) / Ablehnungsgrund (Abgelehnt) sind noch nicht umgesetzt.
  console.log('Aktion für', item);
}

onMounted(async () => {
  await Promise.all([employeeStore.load(), requestStore.load()]);
});
</script>
