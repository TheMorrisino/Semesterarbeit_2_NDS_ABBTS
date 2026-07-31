<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>{{ t('approval.titel') }}</h1>
    </div>

    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-clipboard-check" class="mr-2" />{{ t('approval.offeneAntraege') }}</span>
        <v-chip color="warning" size="small" variant="tonal">
          {{ offeneAntraege.length }} {{ t('approval.ausstehend') }}
        </v-chip>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="offeneAntraege"
        item-value="id"
      >
        <template #item.mitarbeiter="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(mitarbeiterName(item.mitarbeiterId))">
              <span class="text-caption">{{ initialen(mitarbeiterName(item.mitarbeiterId)) }}</span>
            </v-avatar>
            {{ mitarbeiterName(item.mitarbeiterId) }}
          </div>
        </template>

        <template #item.zeitraum="{ item }">
          {{ item.von }} – {{ item.bis }}
        </template>

        <template #item.hinweis="{ item }">
          <v-chip :color="item.ueberschneidung ? 'error' : 'success'" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.ueberschneidung ? t('approval.ueberschneidung') : t('approval.keine') }}
          </v-chip>
        </template>

        <template #item.entscheidung="{ item }">
          <v-btn icon="mdi-check" size="small" variant="tonal" color="success" class="mr-1" @click="genehmigen(item)" />
          <v-btn icon="mdi-close" size="small" variant="tonal" color="error" @click="ablehnen(item)" />
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
  id: number;
  mitarbeiterId: string;
  von: string;
  bis: string;
  tage: number;
  ueberschneidung: boolean;
  eingereichtVor: string;
}

const headers = [
  { title: t('approval.mitarbeiter'), key: 'mitarbeiter' },
  { title: t('approval.zeitraum'), key: 'zeitraum', sortable: false },
  { title: t('approval.tage'), key: 'tage' },
  { title: t('approval.hinweis'), key: 'hinweis' },
  { title: t('approval.eingereicht'), key: 'eingereichtVor' },
  { title: t('approval.entscheidung'), key: 'entscheidung', sortable: false },
];

const mitarbeitende = ref<Mitarbeitender[]>([]);
const offeneAntraege = ref<Antrag[]>([]);

// --- Mitarbeiter aus dem Backend-Store laden ---
async function ladeMitarbeitende() {
  const res = await fetch('/api/mitarbeitende');
  mitarbeitende.value = await res.json();
}

// --- Offene Anträge laden (Endpoint musst du ggf. noch im Backend ergänzen) ---
async function ladeOffeneAntraege() {
  const res = await fetch('/api/antraege?status=offen');
  offeneAntraege.value = await res.json();
}

// --- Auflösung: mitarbeiterId -> Name ---
function mitarbeiterName(id: string): string {
  const m = mitarbeitende.value.find((m) => m.id === id);
  return m?.name ?? t('approval.unbekannt');
}

function initialen(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase();
}

function avatarColor(name: string) {
  const farben = ['purple', 'red', 'blue', 'teal', 'indigo'];
  return farben[name.length % farben.length];
}

async function genehmigen(item: Antrag) {
  await fetch(`/api/antraege/${item.id}/genehmigen`, { method: 'PUT' });
  await ladeOffeneAntraege();
}

async function ablehnen(item: Antrag) {
  await fetch(`/api/antraege/${item.id}/ablehnen`, { method: 'PUT' });
  await ladeOffeneAntraege();
}

onMounted(async () => {
  await Promise.all([ladeMitarbeitende(), ladeOffeneAntraege()]);
});
</script>