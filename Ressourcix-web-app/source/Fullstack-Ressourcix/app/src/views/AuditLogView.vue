<template>
  <div>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-history" class="mr-2" />{{ t('auditlog.untertitel') }}</span>
        <v-btn variant="outlined" prepend-icon="mdi-download" :disabled="!eintraege.length" @click="exportCsv">
          {{ t('auditlog.exportCsv') }}
        </v-btn>
      </v-card-title>

      <v-divider />

      <v-list v-if="eintraege.length" lines="two">
        <v-list-item
          v-for="eintrag in eintraege"
          :key="eintrag.id"
        >
          <template #prepend>
            <v-avatar :color="typInfo(eintrag.action).bg" size="36" rounded="lg">
              <v-icon :icon="typInfo(eintrag.action).icon" :color="typInfo(eintrag.action).color" size="20" />
            </v-avatar>
          </template>

          <v-list-item-title class="font-weight-bold">
            {{ eintrag.summary }} <span class="font-weight-regular text-medium-emphasis">· {{ eintrag.reference }}</span>
          </v-list-item-title>

          <v-list-item-subtitle>
            {{ eintrag.actor }} · {{ formatDatum(eintrag.timestamp) }}
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
import { storeToRefs } from "pinia";
import { useI18n } from "vue-i18n";
import { useAuditLogStore, type AuditLogAction } from "@/stores/auditLog";

const { t } = useI18n();
const auditLogStore = useAuditLogStore();
const { entries: eintraege } = storeToRefs(auditLogStore);

function typInfo(action: AuditLogAction) {
  const map: Record<AuditLogAction, { icon: string; color: string; bg: string }> = {
    antragErfasst: { icon: "mdi-plus", color: "warning", bg: "orange-lighten-4" },
    antragGeaendert: { icon: "mdi-pencil", color: "info", bg: "blue-lighten-4" },
    antragGeloescht: { icon: "mdi-delete", color: "error", bg: "red-lighten-4" },
    mitarbeiterErfasst: { icon: "mdi-account-plus", color: "info", bg: "blue-lighten-4" },
    mitarbeiterGeaendert: { icon: "mdi-account-edit", color: "info", bg: "blue-lighten-4" },
    mitarbeiterStatusGeaendert: { icon: "mdi-account-switch", color: "grey", bg: "grey-lighten-3" },
  };
  return map[action];
}

function formatDatum(iso: string) {
  return new Date(iso).toLocaleString("de-CH", {
    day: "2-digit", month: "2-digit", year: "numeric",
    hour: "2-digit", minute: "2-digit",
  });
}

function exportCsv() {
  const zeilen = eintraege.value.map((e) =>
    [e.summary, e.reference, e.actor, e.timestamp].join(";")
  );
  const csv = ["Aktion;Referenz;Benutzer;Zeitpunkt", ...zeilen].join("\n");
  const blob = new Blob([csv], { type: "text/csv;charset=utf-8;" });
  const url = URL.createObjectURL(blob);
  const a = document.createElement("a");
  a.href = url;
  a.download = "audit-log.csv";
  a.click();
  URL.revokeObjectURL(url);
}
</script>
