<template>
  <div>
    <div class="d-flex justify-end mb-4">
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
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuditLogStore } from '@/stores/auditLog'

interface Mitarbeitender {
  id: string
  name: string
  rolle: string
  pensumProzent: number
  ferienwochen: number
  istAktiv: boolean
}

const { t } = useI18n()
const auditLog = useAuditLogStore()
const suche = ref('')

// Hardcodierte Mock-Daten, kein Backend vorhanden
const mitarbeitende = ref<Mitarbeitender[]>([
  { id: '1', name: 'Morris Meier', rolle: 'Mitarbeitende', pensumProzent: 100, ferienwochen: 5, istAktiv: true },
  { id: '2', name: 'Pedro Santos', rolle: 'Planner/Leitung', pensumProzent: 100, ferienwochen: 5, istAktiv: true },
  { id: '3', name: 'Lena Brunner', rolle: 'Mitarbeitende', pensumProzent: 80, ferienwochen: 4.4, istAktiv: true },
  { id: '4', name: 'Rafael Koch', rolle: 'Mitarbeitende', pensumProzent: 60, ferienwochen: 3.3, istAktiv: false },
])

function naechsteId(): string {
  const maxId = Math.max(0, ...mitarbeitende.value.map((m) => Number(m.id)))
  return String(maxId + 1)
}

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

function toggleAktiv(item: Mitarbeitender) {
  item.istAktiv = !item.istAktiv
  auditLog.log('mitarbeiterStatusGeaendert', 'Mitarbeiter Status geändert', `${item.name}: ${item.istAktiv ? 'Aktiviert' : 'Deaktiviert'}`)
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

function speichern() {
  const istBearbeitung = bearbeitetesId.value !== null

  if (istBearbeitung) {
    const item = mitarbeitende.value.find((m) => m.id === bearbeitetesId.value)
    if (item) {
      Object.assign(item, neuerEintrag.value)
      auditLog.log('mitarbeiterGeaendert', 'Mitarbeiter bearbeitet', item.name)
    }
  } else {
    const neu: Mitarbeitender = { id: naechsteId(), ...neuerEintrag.value, istAktiv: true }
    mitarbeitende.value.push(neu)
    auditLog.log('mitarbeiterErfasst', 'Mitarbeiter erfasst', neu.name)
  }

  dialogSchliessen()
}
</script>