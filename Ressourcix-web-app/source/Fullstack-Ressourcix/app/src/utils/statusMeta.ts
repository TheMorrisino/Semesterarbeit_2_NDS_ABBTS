// Gemeinsame Anzeige-Metadaten (Label/Farbe/Icon) pro RequestStatus, verwendet von
// CalenderView.vue, DashboardView.vue und AbsencesView.vue. `t` wird injiziert, damit
// diese Funktion eine reine Funktion ohne vue-i18n-Abhängigkeit bleibt.

import { RequestStatus } from '@/stores/request'

const STATUS_COLORS: Record<RequestStatus, string> = {
  [RequestStatus.Open]: 'orange',
  [RequestStatus.Approved]: 'green',
  [RequestStatus.Rejected]: 'red',
  [RequestStatus.Taken]: 'blue',
  [RequestStatus.Cancelled]: 'grey',
}

const STATUS_ICONS: Record<RequestStatus, string> = {
  [RequestStatus.Open]: '🕒',
  [RequestStatus.Approved]: '✅',
  [RequestStatus.Rejected]: '❌',
  [RequestStatus.Taken]: '🏖',
  [RequestStatus.Cancelled]: '🚫',
}

export function statusMeta (
  status: RequestStatus,
  t: (key: string) => string,
): { label: string, color: string, icon: string } {
  return {
    label: t(`status.${status.toLowerCase()}`),
    color: STATUS_COLORS[status],
    icon: STATUS_ICONS[status],
  }
}
