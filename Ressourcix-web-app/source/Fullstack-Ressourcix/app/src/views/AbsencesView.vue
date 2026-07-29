<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>{{ t('absences.titel') }}</h1>
    </div>

    <v-card>
<!--       <v-card-title class="d-flex align-center">
        <v-icon icon="mdiAccountCheck" class="mr-2" />
        {{ t('absences.titel') }}
      </v-card-title> -->

      <v-data-table
        :headers="headers"
        :items="absences"
        item-value="id"
      >
        <template #item.status="{ item }">
          <v-chip :color="statusColor(item.status)" size="small" variant="tonal">
            {{ item.status }}
          </v-chip>
        </template>

        <template #item.aktion="{ item }">
          <v-btn
            size="small"
            variant="outlined"
            :disabled="item.status === 'Bezogen'"
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
import { ref } from "vue";
import { useI18n } from "vue-i18n";



type Status = 'Ausstehend' | 'Genehmigt' | 'Bezogen' | 'Abgelehnt';

interface Absence {
  id: number;
  typ: string;
  by: string;
  to: string;
  day: number;
  status: Status;
}

const { t } = useI18n();

const headers = [
  { title: t('absences.typ'), key: 'typ' },
  { title: t('absences.by'), key: 'by' },
  { title: t('absences.to'), key: 'to' },
  { title: t('absences.day'), key: 'day' },
  { title: t('absences.status'), key: 'status' },
  { title: t('absences.aktion'), key: 'aktion', sortable: false },
];



const absences = ref<Absence[]>([
  { id: 1, typ: 'Ferien', by: '13.07.2026', to: '24.07.2026', day: 10, status: 'Ausstehend' },
  { id: 2, typ: 'Ferien', by: '22.12.2026', to: '31.12.2026', day: 6,  status: 'Genehmigt' },
  { id: 3, typ: 'Ferien', by: '06.04.2026', to: '09.04.2026', day: 4,  status: 'Bezogen' },
  { id: 4, typ: 'Ferien', by: '02.02.2026', to: '03.02.2026', day: 2,  status: 'Abgelehnt' },
]);

function statusColor(status: Status) {
  const map: Record<Status, string> = {
    Ausstehend: 'orange',
    Genehmigt: 'green',
    Bezogen: 'blue',
    Abgelehnt: 'red',
  };
  return map[status] ?? 'grey';
}

function actionLabel(status: Status) {
  const map: Record<Status, string> = {
    Ausstehend: 'Details',
    Genehmigt: 'Stornieren',
    Bezogen: '—',
    Abgelehnt: 'Grund',
  };
  return map[status] ?? '';
}

function onAction(item: Absence) {
  // TODO: je nach status Details-Dialog öffnen, stornieren, Grund anzeigen etc.
  console.log('Aktion für', item);
}
</script>