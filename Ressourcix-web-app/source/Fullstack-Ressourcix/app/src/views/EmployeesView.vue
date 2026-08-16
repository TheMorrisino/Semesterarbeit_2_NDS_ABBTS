<template>
  <div>
    <v-card>
      <v-card-title class="d-flex justify-space-between align-center">
        <span><v-icon class="mr-2" icon="mdi-badge-account" />{{ t('employee.manage') }}</span>

        <div class="d-flex align-center ga-3">
          <v-text-field
            v-model="search"
            density="compact"
            hide-details
            :placeholder="t('common.search')"
            prepend-inner-icon="mdi-magnify"
            style="min-width: 130px"
          />

          <v-btn v-if="authStore.isAdmin" color="primary" @click="openCreateDialog">{{ t('employee.newcapture') }}</v-btn>
        </div>
      </v-card-title>

      <v-data-table
        :headers="headers"
        item-value="id"
        :items="employeeStore.employees"
        :loading="employeeStore.loading"
        :search="search"
      >
        <template #item.name="{ item }">
          <div class="d-flex align-center ga-2">
            <v-avatar :color="avatarColor(item.name)" size="32">
              <span class="text-caption">{{ initials(item.name) }}</span>
            </v-avatar>
            {{ item.name }}
          </div>
        </template>

        <template #item.workload="{ item }">{{ item.workload }}%</template>

        <template #item.isActive="{ item }">
          <v-chip :color="item.isActive ? 'success' : 'default'" size="small" variant="tonal">
            <v-icon icon="mdi-circle" size="8" start />
            {{ item.isActive ? t('common.activ') : t('common.Disabled') }}
          </v-chip>
        </template>

        <template #item.action="{ item }">
          <v-btn icon="mdi-information-outline" size="small" variant="text" @click="openDetailDialog(item)" />

          <v-btn
            v-if="authStore.isAdmin"
            icon="mdi-pencil"
            size="small"
            variant="text"
            @click="openEditDialog(item)"
          />

          <v-btn
            v-if="authStore.isAdmin"
            :icon="item.isActive ? 'mdi-pause' : 'mdi-play'"
            size="small"
            variant="text"
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
              :label="t('common.name')"
              :rules="[(v) => !!v || t('employee.nameRequired')]"
            />

            <v-text-field
              v-model="newEntry.username"
              :disabled="editingId !== null"
              :label="t('employee.username')"
              :rules="[(v) => !!v || t('employee.usernameRequired')]"
            />

            <v-select
              v-model="newEntry.role"
              :items="[t('role.employee'), t('role.admin'), t('role.planner')]"
              :label="t('employee.role')"
              :rules="[(v) => !!v || t('employee.roleRequired')]"
            />
            <!-- Berechtigungslevel wird ausschliesslich anhand der Rolle vergeben, nicht direkt editierbar -->
            <div class="text-caption text-medium-emphasis mb-3">
              {{ t('employee.permissionLevel') }}: {{ derivedPermissionLevel }}
            </div>

            <v-text-field
              v-model.number="newEntry.workload"
              :label="t('employee.workloadPercent')"
              :rules="[(v) => (v > 0 && v <= 100) || t('employee.workloadRange')]"
              type="number"
            />

            <v-text-field
              v-model.number="newEntry.vacationDays"
              :label="t('employee.vacationDays')"
              step="1"
              type="number"
            />
          </v-form>
        </v-card-text>

        <v-card-actions>
          <v-btn v-if="editingId" color="error" variant="text" @click="deleteEntry">{{ t('common.delete') }}</v-btn>
          <v-spacer />
          <v-btn variant="text" @click="closeDialog">{{ t('common.cancel') }}</v-btn>
          <v-btn color="primary" :disabled="!formValid" @click="save">{{ t('common.save') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <!-- Detailansicht: alle Attribute eines Mitarbeiters, einzeln kopierbar -->
    <v-dialog v-model="detailDialogOpen" max-width="480">
      <v-card v-if="detailEntry">
        <v-card-title>{{ t('employee.details') }} – {{ detailEntry.name }}</v-card-title>

        <v-card-text>
          <v-list density="compact">
            <v-list-item v-for="field in detailFields" :key="field.key">
              <v-list-item-title class="text-caption text-medium-emphasis">{{ field.label }}</v-list-item-title>

              <v-list-item-subtitle class="d-flex align-center ga-2">
                <span class="detail-value">{{ field.value }}</span>

                <v-btn
                  icon="mdi-content-copy"
                  size="x-small"
                  :title="t('employee.copy')"
                  variant="text"
                  @click="copyValue(field.value)"
                />
              </v-list-item-subtitle>
            </v-list-item>
          </v-list>
        </v-card-text>

        <v-card-actions>
          <v-spacer />
          <v-btn variant="text" @click="detailDialogOpen = false">{{ t('common.close') }}</v-btn>
        </v-card-actions>
      </v-card>
    </v-dialog>

    <v-snackbar v-model="copySnackbar" :timeout="1500">{{ t('employee.copied') }}</v-snackbar>
  </div>
</template>

<script setup lang="ts">
  import { computed, onMounted, ref } from 'vue'
  import { useI18n } from 'vue-i18n'
  import { useAuthStore } from '@/stores/auth'
  import { type Employee, useEmployeeStore } from '@/stores/employee'
  import { avatarColor, initialsFor } from '@/utils/initials'

  const { t } = useI18n()
  const employeeStore = useEmployeeStore()
  const authStore = useAuthStore()
  const search = ref('')

  const headers = [
    { title: t('common.name'), key: 'name' },
    { title: t('employee.username'), key: 'username' },
    { title: t('employee.role'), key: 'role' },
    { title: t('employee.permissionLevel'), key: 'permissionLevel' },
    { title: t('employee.workload'), key: 'workload' },
    { title: t('employee.vacationDays'), key: 'vacationDays' },
    { title: t('employee.isActive'), key: 'isActive' },
    { title: t('common.action'), key: 'action', sortable: false },
  ]

  const initials = initialsFor

  async function toggleActive (item: Employee) {
    await employeeStore.toggleActive(item.id)
  }

  // --- Detailansicht (alle Attribute, einzeln kopierbar) ---
  const detailDialogOpen = ref(false)
  const detailEntry = ref<Employee | null>(null)
  const copySnackbar = ref(false)

  const detailFields = computed(() => {
    if (!detailEntry.value) return []
    const e = detailEntry.value
    return [
      { key: 'id', label: t('employee.id'), value: e.id },
      { key: 'name', label: t('common.name'), value: e.name },
      { key: 'username', label: t('employee.username'), value: e.username },
      { key: 'role', label: t('employee.role'), value: e.role },
      { key: 'permissionLevel', label: t('employee.permissionLevel'), value: String(e.permissionLevel) },
      { key: 'workload', label: t('employee.workload'), value: String(e.workload) },
      { key: 'vacationDays', label: t('employee.vacationDays'), value: String(e.vacationDays) },
      { key: 'isActive', label: t('employee.isActive'), value: e.isActive ? t('common.activ') : t('common.Disabled') },
      { key: 'mustChangePassword', label: t('employee.mustChangePassword'), value: String(e.mustChangePassword ?? false) },
    ]
  })

  function openDetailDialog (item: Employee) {
    detailEntry.value = item
    detailDialogOpen.value = true
  }

  async function copyValue (value: string) {
    await navigator.clipboard.writeText(value)
    copySnackbar.value = true
  }

  // --- Dialog-Logik (ein Dialog für Erfassen + Bearbeiten) ---
  const dialogOpen = ref(false)
  const formValid = ref(false)
  const formRef = ref()
  const editingId = ref<string | null>(null)

  function emptyEntry () {
    return {
      name: '',
      username: '',
      role: '',
      workload: 100,
      vacationDays: 25,
    }
  }
  const newEntry = ref(emptyEntry())

  // Berechtigungslevel ist kein eigenes Formularfeld mehr, sondern folgt zwingend aus der Rolle.
  function permissionLevelForRole (role: string): number {
    return role === t('role.employee') ? 1 : 5
  }
  const derivedPermissionLevel = computed(() => permissionLevelForRole(newEntry.value.role))

  function openCreateDialog () {
    editingId.value = null
    newEntry.value = emptyEntry()
    dialogOpen.value = true
  }

  function openEditDialog (item: Employee) {
    editingId.value = item.id
    newEntry.value = {
      name: item.name,
      username: item.username,
      role: item.role,
      workload: item.workload,
      vacationDays: item.vacationDays,
    }
    dialogOpen.value = true
  }

  function closeDialog () {
    dialogOpen.value = false
    newEntry.value = emptyEntry()
    editingId.value = null
    formRef.value?.reset()
  }

  async function deleteEntry () {
    if (!editingId.value) return
    if (!confirm(t('employee.confirmDelete', { name: newEntry.value.name }))) return

    await employeeStore.remove(editingId.value)
    closeDialog()
  }

  async function save () {
    const permissionLevel = derivedPermissionLevel.value
    if (editingId.value === null) {
      await employeeStore.create({
        ...newEntry.value,
        permissionLevel,
        isActive: true,
      })
    } else {
      const existing = employeeStore.employees.find(e => e.id === editingId.value)
      await employeeStore.update(editingId.value, {
        ...newEntry.value,
        permissionLevel,
        isActive: existing?.isActive ?? true,
      })
    }

    closeDialog()
  }

  onMounted(() => employeeStore.load())
</script>

<style scoped>
.detail-value {
  font-family: monospace;
  word-break: break-all;
}
</style>
