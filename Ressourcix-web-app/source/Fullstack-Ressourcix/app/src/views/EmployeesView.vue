<template>
  <div>
    <div class="d-flex justify-space-between align-center mb-4">
      <h1>{{ t('mitarbeitende.titel') }}</h1>
      <v-text-field
        v-model="suche"
        prepend-inner-icon="mdi-magnify"
        :placeholder="t('common.suchen')"
        density="compact"
        hide-details
        style="max-width: 280px"
      />
    </div>

    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-badge-account" class="mr-2" />{{ t('mitarbeitende.verwalten') }}</span>
        <v-btn color="primary" @click="neuErfassen">{{ t('mitarbeitende.neuErfassen') }}</v-btn>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="mitarbeitende"
        :search="suche"
        item-value="id"
      >
        <template #item.name="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(item.name)">
              <span class="text-caption">{{ initialen(item.name) }}</span>
            </v-avatar>
            {{ item.name }}
          </div>
        </template>

        <template #item.pensumProzent="{ item }">{{ item.pensumProzent }}%</template>

        <template #item.istAktiv="{ item }">
          <v-chip :color="item.istAktiv ? 'success' : 'default'" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.istAktiv ? t('common.aktiv') : t('common.deaktiviert') }}
          </v-chip>
        </template>

        <template #item.aktion="{ item }">
          <v-btn icon="mdi-pencil" size="small" variant="text" @click="bearbeiten(item)" />
          <v-btn
            :icon="item.istAktiv ? 'mdi-pause' : 'mdi-play'"
            size="small" variant="text"
            @click="toggleAktiv(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <!-- EIN Dialog für "Erfassen" UND "Bearbeiten" -->
    <v-dialog v-model="dialogOffen" max-width="480">
      <v-card>
        <v-card-title>
          {{ bearbeitetesId ? 'Mitarbeitende bearbeiten' : 'Mitarbeitende erfassen' }}
        </v-card-title>

        <v-card-text>
          <v-form ref="formRef" v-model="formGueltig">
            <v-text-field
              v-model="neuerEintrag.name"
              label="Name"
              :rules="[(v) => !!v || 'Name ist erforderlich']"
            />
            <v-select
              v-model="neuerEintrag.rolle"
              label="Rolle"
              :items="['Mitarbeitende', 'Planner/Leitung', 'IT/Wartung']"
              :rules="[(v) => !!v || 'Rolle ist erforderlich']"
            />
            <v-text-field
              v-model.number="neuerEintrag.pensumProzent"
              label="Pensum (%)"
              type="number"
              :rules="[(v) => (v > 0 && v <= 100) || 'Zwischen 1 und 100']"
            />
            <v-text-field
              v-model.number="neuerEintrag.ferienwochen"
              label="Ferienwochen"
              type="number"
              step="0.1"
            />
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="dialogSchliessen">Abbrechen</v-btn>
          <v-btn color="primary" :disabled="!formGueltig" @click="speichern">Speichern</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'

interface Mitarbeitender {
  id: string
  name: string
  rolle: string
  pensumProzent: number
  ferienwochen: number
  istAktiv: boolean
}

const { t } = useI18n()
const suche = ref('')
const mitarbeitende = ref<Mitarbeitender[]>([])

const headers = [
  { title: t('common.name'), key: 'name' },
  { title: t('mitarbeitende.rolle'), key: 'rolle' },
  { title: t('mitarbeitende.pensum'), key: 'pensumProzent' },
  { title: t('mitarbeitende.ferienwochen'), key: 'ferienwochen' },
  { title: t('mitarbeitende.konto'), key: 'istAktiv' },
  { title: t('common.aktion'), key: 'aktion', sortable: false },
]

function initialen(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase()
}
function avatarColor(name: string) {
  const farben = ['purple', 'red', 'blue', 'grey']
  return farben[name.length % farben.length]
}

async function ladeMitarbeitendeAsync() {
  const res = await fetch('/api/mitarbeitende')
  mitarbeitende.value = await res.json()
}

async function toggleAktiv(item: Mitarbeitender) {
  await fetch(`/api/mitarbeitende/${item.id}/toggle-aktiv`, { method: 'PUT' })
  await ladeMitarbeitendeAsync()
}

// --- Dialog-Logik (ein Dialog für Erfassen + Bearbeiten) ---
const dialogOffen = ref(false)
const formGueltig = ref(false)
const formRef = ref()
const bearbeitetesId = ref<string | null>(null)

const leererEintrag = () => ({
  name: '',
  rolle: '',
  pensumProzent: 100,
  ferienwochen: 5,
})
const neuerEintrag = ref(leererEintrag())

function neuErfassen() {
  bearbeitetesId.value = null
  neuerEintrag.value = leererEintrag()
  dialogOffen.value = true
}

function bearbeiten(item: Mitarbeitender) {
  bearbeitetesId.value = item.id
  neuerEintrag.value = {
    name: item.name,
    rolle: item.rolle,
    pensumProzent: item.pensumProzent,
    ferienwochen: item.ferienwochen,
  }
  dialogOffen.value = true
}

function dialogSchliessen() {
  dialogOffen.value = false
  neuerEintrag.value = leererEintrag()
  bearbeitetesId.value = null
  formRef.value?.reset()
}

async function speichern() {
  const istBearbeitung = bearbeitetesId.value !== null
  const url = istBearbeitung
    ? `/api/mitarbeitende/${bearbeitetesId.value}`
    : '/api/mitarbeitende'

  const res = await fetch(url, {
    method: istBearbeitung ? 'PUT' : 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ ...neuerEintrag.value, istAktiv: true }),
  })

  if (res.ok) {
    dialogSchliessen()
    await ladeMitarbeitendeAsync()
  } else {
    alert('Fehler beim Speichern')
  }
}

onMounted(ladeMitarbeitendeAsync)
</script>