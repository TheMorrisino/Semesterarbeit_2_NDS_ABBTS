import { RequestStatus } from '@/stores/request'
import { daysBetweenInclusive } from './date'

// "Geplant" = offene oder genehmigte Anträge eines Mitarbeiters. Eine einzige Implementierung,
// da zuvor jede Ansicht ihre eigene, leicht abweichende Berechnung hatte.
export function plannedDaysForEmployee (
  requests: { employeeId: string, status: RequestStatus, from: string, until: string }[],
  employeeId: string,
): number {
  return requests
    .filter(r => r.employeeId === employeeId && (r.status === RequestStatus.Open || r.status === RequestStatus.Approved))
    .reduce((sum, r) => sum + daysBetweenInclusive(r.from, r.until), 0)
}
