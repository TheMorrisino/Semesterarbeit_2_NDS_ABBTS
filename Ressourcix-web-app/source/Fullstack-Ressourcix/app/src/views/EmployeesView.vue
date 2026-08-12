<template>
  <div>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon icon="mdi-badge-account" class="mr-2" />{{ t('employee.manage') }}</span>
        <div class="d-flex align-center ga-3">
          <v-text-field
            v-model="search"
            prepend-inner-icon="mdi-magnify"
            :placeholder="t('common.search')"
            density="compact"
            hide-details
            style="min-width: 130px"
          />
          <v-btn color="primary" @click="openCreateDialog">{{ t('employee.newcapture') }}</v-btn>
        </div>
      </v-card-title>

      <v-data-table
        :headers="headers"
        :items="employeeStore.employees"
        :search="search"
        :loading="employeeStore.loading"
        item-value="id"
      >
        <template #item.name="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar size="32" :color="avatarColor(item.name)">
              <span class="text-caption">{{ initials(item.name) }}</span>
            </v-avatar>
            {{ item.name }}
          </div>
        </template>

        <template #item.workload="{ item }">{{ item.workload }}%</template>

        <template #item.isActive="{ item }">
          <v-chip :color="item.isActive ? 'success' : 'default'" size="small" variant="tonal">
            <v-icon start icon="mdi-circle" size="8" />
            {{ item.isActive ? t('common.activ') : t('common.Disabled') }}
          </v-chip>
        </template>

        <template #item.action="{ item }">
          <v-btn icon="mdi-pencil" size="small" variant="text" @click="openEditDialog(item)" />
          <v-btn
            :icon="item.isActive ? 'mdi-pause' : 'mdi-play'"
            size="small" variant="text"
            @click="toggleActive(item)"
          />
        </template>
      </v-data-table>
    </v-card>

    <!-- EIN Dialog für "Erfassen" UND "Bearbeiten" -->
    <v-dialog v-model="dialogOpen" max-width="480">
      <v-card>
        <v-card-title>
          {{ editingId ? t('employee.empedit') : t('employee.empcapture') }}
        </v-card-title>

        <v-card-text>
          <v-form ref="formRef" v-model="formValid">
            <v-text-field
              v-model="newEntry.name"
              label="Name"
              :rules="[(v) => !!v || 'Name ist erforderlich']"
            />
            <v-select
              v-model="newEntry.role"
              label="Rolle"
              :items="[t('employee.employee'), t('qualification.planner')]"
              :rules="[(v) => !!v || t('Rolle ist erforderlich')]"
            />
            <v-text-field
              v-model.number="newEntry.workload"
              label="Pensum (%)"
              type="number"
              :rules="[(v) => (v > 0 && v <= 100) || 'Zwischen 1 und 100']"
            />
            <v-text-field
              v-model.number="newEntry.vacationDays"
              label="Ferienwochen"
              type="number"
              step="1"
            />
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-btn v-if="editingId" variant="text" color="error" @click="deleteEntry">Löschen</v-btn>
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">Abbrechen</v-btn>
          <v-btn color="primary" :disabled="!formValid" @click="save">Speichern</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useI18n } from 'vue-i18n'
import { useEmployeeStore,  type Employee } from '@/stores/employee'
import { useAuditLogStore } from '@/stores/auditLog'

const { t } = useI18n()
const employeeStore = useEmployeeStore()
const auditLog = useAuditLogStore()
const search = ref('')

const headers = [
  { title: t('common.name'), key: 'name' },
  { title: t('employee.role'), key: 'role' },
  { title: t('employee.workload'), key: 'workload' },
  { title: t('employee.vacationDays'), key: 'vacationDays' },
  { title: t('employee.isActive'), key: 'isActive' },
  { title: t('common.action'), key: 'action', sortable: false },
]

function initials(name: string) {
  return name.split(' ').map((n) => n[0]).join('').toUpperCase()
}
function avatarColor(name: string) {
  const colors = ['purple', 'red', 'blue', 'grey']
  return colors[name.length % colors.length]
}

async function toggleActive(item: Employee) {
  await employeeStore.toggleActive(item.id)
  auditLog.log('EmployeeStatusChanged', 'Mitarbeiter Status geändert', item.id)
}

// --- Dialog-Logik (ein Dialog für Erfassen + Bearbeiten) ---
const dialogOpen = ref(false)
const formValid = ref(false)
const formRef = ref()
const editingId = ref<string | null>(null)

const emptyEntry = () => ({
  name: '',
  role: '',
  workload: 100,
  vacationDays: 5,
})
const newEntry = ref(emptyEntry())

function openCreateDialog() {
  editingId.value = null
  newEntry.value = emptyEntry()
  dialogOpen.value = true
}

function openEditDialog(item: Employee) {
  editingId.value = item.id
  newEntry.value = {
    name: item.name,
    role: item.role,
    workload: item.workload,
    vacationDays: item.vacationDays,
  }
  dialogOpen.value = true
}

function closeDialog() {
  dialogOpen.value = false
  newEntry.value = emptyEntry()
  editingId.value = null
  formRef.value?.reset()
}

async function deleteEntry() {
  if (!editingId.value) return
  if (!confirm(`${newEntry.value.name} wirklich löschen?`)) return

  await employeeStore.remove(editingId.value)
  auditLog.log('EmployeeDeleted', 'Mitarbeiter gelöscht', editingId.value)
  closeDialog()
}

async function save() {
  if (editingId.value !== null) {
    const existing = employeeStore.employees.find((e) => e.id === editingId.value)
    await employeeStore.update(editingId.value, {
      ...newEntry.value,
      isActive: existing?.isActive ?? true,
    })
    auditLog.log('EmployeeUpdated', 'Mitarbeiter bearbeitet', editingId.value)
  } else {
    const created = await employeeStore.create({
      ...newEntry.value,
      isActive: true,
    })
    auditLog.log('EmployeeCreated', 'Mitarbeiter erfasst', created.id)
  }

  closeDialog()
}

onMounted(() => employeeStore.load())
</script>
