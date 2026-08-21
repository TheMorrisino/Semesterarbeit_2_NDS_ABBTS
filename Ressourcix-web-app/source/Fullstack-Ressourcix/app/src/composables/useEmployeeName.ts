import { useI18n } from 'vue-i18n'
import { useEmployeeStore } from '@/stores/employee'

// Mitarbeitername mit Fallback für unbekannte/gelöschte IDs, verwendet von AbsencesView,
// ApprovalView und DashboardView.
export function useEmployeeName () {
  const { t } = useI18n()
  const employeeStore = useEmployeeStore()

  function employeeName (employeeId: string): string {
    return employeeStore.employeeName(employeeId) ?? t('common.unknown')
  }

  return { employeeName }
}
