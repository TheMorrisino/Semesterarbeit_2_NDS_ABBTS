import { computed, ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/auth'
import { type Employee, useEmployeeStore } from '@/stores/employee'
import { AbsenceType, type Request, RequestStatus, useRequestStore } from '@/stores/request'
import { rangesOverlap } from '@/utils/overlap'

export type EntryDialogMode = 'create' | 'edit'

export interface EntryDialogState {
  mode: EntryDialogMode
  employee: Employee
  entryId: string | null // null, solange der Antrag noch nicht gespeichert wurde
  startDate: string
  endDate: string
  remark: string
  status: RequestStatus
}

// Dialog-Zustand + Validierung für "Antrag stellen/bearbeiten" in CalenderView: die einzige
// echte, unabhängig von der Grid-Darstellung testbare Geschäftslogik dieser View.
export function useEntryDialog () {
  const { t } = useI18n()
  const employeeStore = useEmployeeStore()
  const requestStore = useRequestStore()
  const authStore = useAuthStore()

  const entryDialog = ref<EntryDialogState | null>(null)

  const entryDialogOpen = computed({
    get: () => entryDialog.value !== null,
    set: (open: boolean) => {
      if (!open) {
        entryDialog.value = null
      }
    },
  })

  function openCreateDialog (employee: Employee, startIso: string) {
    entryDialog.value = {
      mode: 'create',
      employee,
      entryId: null,
      startDate: startIso,
      endDate: startIso,
      remark: '',
      status: RequestStatus.Open,
    }
  }

  function openEditDialog (employee: Employee, request: Request) {
    entryDialog.value = {
      mode: 'edit',
      employee,
      entryId: request.id,
      startDate: request.from,
      endDate: request.until,
      remark: request.remark ?? '',
      status: request.status,
    }
  }

  function handleCellClick (employee: Employee, cell: { entry: Request | null, iso: string }) {
    if (cell.entry) {
      openEditDialog(employee, cell.entry)
    } else {
      openCreateDialog(employee, cell.iso)
    }
  }

  // Ein Mitarbeiter darf sich nicht selbst überschneiden -> blockiert das Speichern
  function hasSelfOverlap (state: EntryDialogState): boolean {
    return requestStore.requests.some(
      request =>
        request.employeeId === state.employee.id
        && request.id !== state.entryId
        && rangesOverlap(state.startDate, state.endDate, request.from, request.until),
    )
  }

  const dialogEndDateError = computed<string | null>(() => {
    const state = entryDialog.value
    if (!state) {
      return null
    }
    if (state.endDate < state.startDate) {
      return t('calendar.validationEndBeforeStart')
    }
    if (hasSelfOverlap(state)) {
      return t('calendar.validationSelfOverlap')
    }
    return null
  })

  const dialogStatusError = computed<string | null>(() => {
    const state = entryDialog.value
    if (!state) {
      return null
    }
    if (state.status != RequestStatus.Open) {
      return t('calendar.validationStatusError')
    }
    return null
  })

  // Employees dürfen nur eigene Anträge erfassen/bearbeiten/löschen, Admins dürfen das für jeden (siehe Backend-Check).
  const dialogForeignEmployeeError = computed<string | null>(() => {
    const state = entryDialog.value
    if (!state || authStore.isAdmin) {
      return null
    }
    if (state.employee.id === authStore.user?.id) {
      return null
    }
    return t('calendar.foreignEmployeeError')
  })

  // Überschneidung mit Kollegen ist nur ein Hinweis und blockiert das Speichern nicht (BR-01.04)
  const overlappingColleagues = computed<string[]>(() => {
    const state = entryDialog.value
    if (!state) {
      return []
    }
    const names = new Set<string>()
    for (const request of requestStore.requests) {
      if (request.employeeId === state.employee.id) {
        continue
      }
      if (request.id === state.entryId) {
        continue
      }
      if (rangesOverlap(state.startDate, state.endDate, request.from, request.until)) {
        const colleague = employeeStore.employees.find(employee => employee.id === request.employeeId)
        if (colleague) {
          names.add(colleague.name)
        }
      }
    }
    return Array.from(names)
  })

  async function saveEntry () {
    const state = entryDialog.value
    if (!state || dialogEndDateError.value || dialogForeignEmployeeError.value) {
      return
    }

    if (state.mode === 'create') {
      await requestStore.create({
        employeeId: state.employee.id,
        from: state.startDate,
        until: state.endDate,
        type: AbsenceType.Vacation,
        remark: state.remark.trim() || null,
      })
    } else if (state.entryId) {
      await requestStore.update(state.entryId, state.endDate, state.status)
    }
    entryDialog.value = null
  }

  async function deleteEntry () {
    const state = entryDialog.value
    if (!state || state.mode !== 'edit' || !state.entryId || dialogForeignEmployeeError.value) {
      return
    }
    await requestStore.remove(state.entryId)
    entryDialog.value = null
  }

  return {
    entryDialog,
    entryDialogOpen,
    openCreateDialog,
    openEditDialog,
    handleCellClick,
    dialogEndDateError,
    dialogStatusError,
    dialogForeignEmployeeError,
    overlappingColleagues,
    saveEntry,
    deleteEntry,
  }
}
