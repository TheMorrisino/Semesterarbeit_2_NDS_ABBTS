<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>{{ t('teamview.titel') }}</h1>
    </div>

    <!-- Stat-Cards -->
    <v-row class="mb-4">
      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-account-group" size="18" />
            <span class="text-caption text-uppercase">{{ t('teamview.mitarbeitende') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.mitarbeitendeCount }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-account-clock" size="18" color="blue" />
            <span class="text-caption text-uppercase">{{ t('teamview.aktuellAbwesend') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.aktuellAbwesend }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-clock-outline" size="18" color="orange" />
            <span class="text-caption text-uppercase">{{ t('teamview.offen') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.offen }}</div>
        </v-card>
      </v-col>

      <v-col cols="12" sm="6" md="3">
        <v-card class="pa-4">
          <div class="d-flex align-center ga-2 mb-2 text-medium-emphasis">
            <v-icon icon="mdi-alert" size="18" color="red" />
            <span class="text-caption text-uppercase">{{ t('teamview.ueberschneidungen') }}</span>
          </div>
          <div class="text-h4 font-weight-bold">{{ stats.ueberschneidungen }}</div>
        </v-card>
      </v-col>
    </v-row>

    <!-- Team-Tabelle -->
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-account-group" class="mr-2" />{{ t('teamview.saldoStatus') }}</span>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="teamZeilen"
        item-value="id"
      >
        <template #item.mitarbeiter="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(item.name)">
              <span class="text-caption">{{ initialen(item.name) }}</span>
            </v-avatar>
            {{ item.name }}
          </div>
        </template>

        <template #item.status="{ item }">
          <v-chip :color="statusColor(item.status)" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.status }}
          </v-chip>
        </template>
      </v-data-table>
    </v-card>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from "vue";
import { useI18n } from "vue-i18n";

const { t } = useI18n();

interface Mitarbeitender {
  id: string;
  name: string;
  rolle: string;
  pensumProzent: number;
  ferienwochen: number;
  istAktiv: boolean;
}

interface Antrag {
  id: string;
  mitarbeiterId: string;
  von: string;
  bis: string;
  tage: number;
  ueberschneidung: boolean;
  status: 'Offen' | 'Genehmigt' | 'Abgelehnt';
  eingereichtAm: string;
}

interface TeamZeile {
  id: string;
  name: string;
  anspruch: number;
  bezogen: number;
  geplant: number;
  rest: number;
  status: string;
}

const headers = [
  { title: t('teamview.mitarbeiter'), key: 'mitarbeiter' },
  { title: t('teamview.anspruch'), key: 'anspruch' },
  { title: t('teamview.bezogen'), key: 'bezogen' },
  { title: t('teamview.geplant'), key: 'geplant' },
  { title: t('teamview.rest'), key: 'rest' },
  { title: t('teamview.status'), key: 'status' },
];

const mitarbeitende = ref<Mitarbeitender[]>([]);
const antraege = ref<Antrag[]>([]);

async function ladeMitarbeitende() {
  const res = await fetch('/api/employees');
  mitarbeitende.value = await res.json();
}

async function ladeAntraege() {
  const res = await fetch('/api/antraege');
  antraege.value = await res.json();
}

// --- Ferienanspruch: aus Ferienwochen -> Tage (z.B. 5 Wochen à 5 Arbeitstage = 25 Tage) ---
function anspruchTage(m: Mitarbeitender): number {
  return Math.round(m.ferienwochen * 5);
}

// --- Bezogen: genehmigte Anträge, deren Enddatum in der Vergangenheit liegt ---
function bezogenTage(mitarbeiterId: string): number {
  return antraege.value
    .filter((a) => a.mitarbeiterId === mitarbeiterId && a.status === 'Genehmigt' && new Date(a.bis) < new Date())
    .reduce((sum, a) => sum + a.tage, 0);
}

// --- Geplant: genehmigte Anträge in der Zukunft ---
function geplantTage(mitarbeiterId: string): number {
  return antraege.value
    .filter((a) => a.mitarbeiterId === mitarbeiterId && a.status === 'Genehmigt' && new Date(a.bis) >= new Date())
    .reduce((sum, a) => sum + a.tage, 0);
}

// --- Status: gibt es einen offenen Antrag für diese Person? ---
function mitarbeiterStatus(mitarbeiterId: string): string {
  const hatOffenen = antraege.value.some((a) => a.mitarbeiterId === mitarbeiterId && a.status === 'Offen');
  return hatOffenen ? t('teamview.offen') : t('teamview.geplant');
}

const teamZeilen = computed<TeamZeile[]>(() =>
  mitarbeitende.value.map((m) => {
    const anspruch = anspruchTage(m);
    const bezogen = bezogenTage(m.id);
    const geplant = geplantTage(m.id);
    return {
      id: m.id,
      name: m.name,
      anspruch,
      bezogen,
      geplant,
      rest: anspruch - bezogen - geplant,
      status: mitarbeiterStatus(m.id),
    };
  })
);

const stats = computed(() => ({
  mitarbeitendeCount: mitarbeitende.value.length,
  aktuellAbwesend: antraege.value.filter((a) => {
    const heute = new Date();
    return a.status === 'Genehmigt' && new Date(a.von) <= heute && new Date(a.bis) >= heute;
  }).length,
  offen: antraege.value.filter((a) => a.status === 'Offen').length,
  ueberschneidungen: antraege.value.filter((a) => a.ueberschneidung).length,
}));

function initialen(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase();
}

function avatarColor(name: string) {
  const farben = ['purple', 'red', 'blue', 'teal', 'indigo'];
  return farben[name.length % farben.length];
}

function statusColor(status: string) {
  return status === t('teamview.antragOffen') ? 'orange' : 'green';
}


onMounted(async () => {
  await Promise.all([ladeMitarbeitende(), ladeAntraege()]);
});
</script>