<template>
  <div>
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
          {{ formatDatum(item.von) }} – {{ formatDatum(item.bis) }}
        </template>

        <template #item.hinweis="{ item }">
          <v-chip :color="item.ueberschneidung ? 'error' : 'success'" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.ueberschneidung ? t('approval.ueberschneidung') : t('approval.keine') }}
          </v-chip>
        </template>

        <template #item.eingereichtAm="{ item }">
          {{ vorTagen(item.eingereichtAm) }}
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
import { ref, onMounted } from "vue";
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

const headers = [
  { title: t('approval.mitarbeiter'), key: 'mitarbeiter' },
  { title: t('approval.zeitraum'), key: 'zeitraum', sortable: false },
  { title: t('approval.tage'), key: 'tage' },
  { title: t('approval.hinweis'), key: 'hinweis' },
  { title: t('approval.eingereicht'), key: 'eingereichtAm' },
  { title: t('approval.entscheidung'), key: 'entscheidung', sortable: false },
];

const mitarbeitende = ref<Mitarbeitender[]>([]);
const offeneAntraege = ref<Antrag[]>([]);

async function ladeMitarbeitende() {
  const res = await fetch('/api/mitarbeitende');
  mitarbeitende.value = await res.json();
}

async function ladeOffeneAntraege() {
  const res = await fetch('/api/antraege?status=offen');
  offeneAntraege.value = await res.json();
}

function mitarbeiterName(id: string): string {
  return mitarbeitende.value.find((m) => m.id === id)?.name ?? t('approval.unbekannt');
}

function initialen(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase();
}

function avatarColor(name: string) {
  const farben = ['purple', 'red', 'blue', 'teal', 'indigo'];
  return farben[name.length % farben.length];
}

function formatDatum(iso: string) {
  return new Date(iso).toLocaleDateString('de-CH', { day: '2-digit', month: '2-digit' });
}

function vorTagen(iso: string) {
  const tage = Math.round((Date.now() - new Date(iso).getTime()) / 86_400_000);
  if (tage <= 0) return t('approval.heute');
  return tage === 1 ? t('approval.vor1Tag') : t('approval.vorNTagen', { n: tage });
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