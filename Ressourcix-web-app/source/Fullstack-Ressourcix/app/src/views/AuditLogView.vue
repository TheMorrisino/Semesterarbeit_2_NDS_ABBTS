<template>
  <div>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-history" class="mr-2" />{{ t('auditlog.untertitel') }}</span>
        <v-btn variant="outlined" prepend-icon="mdi-download" :disabled="!entries.length" @click="exportCsv">
          {{ t('auditlog.exportCsv') }}
        </v-btn>
      </v-card-title>

      <v-divider />

      <v-list v-if="entries.length" lines="two">
        <v-list-item
          v-for="entry in entries"
          :key="entry.id"
        >
          <template #prepend>
            <v-avatar :color="typeInfo(entry.action).bg" size="36" rounded="lg">
              <v-icon :icon="typeInfo(entry.action).icon" :color="typeInfo(entry.action).color" size="20" />
            </v-avatar>
          </template>

          <v-list-item-title class="font-weight-bold">
            {{ entry.summary }}
          </v-list-item-title>

          <v-list-item-subtitle>
            {{ t('auditlog.by') }} {{ entry.actor}} · {{ formatDate(entry.timestamp) }}
          </v-list-item-subtitle>
        </v-list-item>
      </v-list>

      <div v-else class="pa-6 text-center text-medium-emphasis">
        {{ t('auditlog.leer') }}
      </div>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from "vue";
import { storeToRefs } from "pinia";
import { useI18n } from "vue-i18n";
import { useAuditLogStore, type AuditLogAction } from "@/stores/auditLog";

const { t } = useI18n();
const auditLogStore = useAuditLogStore();
const { entries } = storeToRefs(auditLogStore);

function typeInfo(action: AuditLogAction) {
  const map: Record<AuditLogAction, { icon: string; color: string; bg: string }> = {
    RequestCreated: { icon: "mdi-plus", color: "warning", bg: "orange-lighten-4" },
    RequestUpdated: { icon: "mdi-pencil", color: "info", bg: "blue-lighten-4" },
    RequestDeleted: { icon: "mdi-delete", color: "error", bg: "red-lighten-4" },
    EmployeeCreated: { icon: "mdi-account-plus", color: "info", bg: "blue-lighten-4" },
    EmployeeUpdated: { icon: "mdi-account-edit", color: "info", bg: "blue-lighten-4" },
    EmployeeStatusChanged: { icon: "mdi-account-switch", color: "grey", bg: "grey-lighten-3" },
    EmployeeDeleted: { icon: "mdi-account-remove", color: "error", bg: "red-lighten-4" },
  };
  return map[action];
}

function formatDate(iso: string) {
  return new Date(iso).toLocaleString("de-CH", {
    day: "2-digit", month: "2-digit", year: "numeric",
    hour: "2-digit", minute: "2-digit",
  });
}

function exportCsv() {
  const rows = entries.value.map((e) =>
    [e.summary, e.reference, e.actor, e.timestamp].join(";")
  );
  const csv = ["Aktion;Referenz;Benutzer;Zeitpunkt", ...rows].join("\n");
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "audit-log.csv";
  a.click();
  URL.revokeObjectURL(url);
}

onMounted(() => auditLogStore.load());
</script>
