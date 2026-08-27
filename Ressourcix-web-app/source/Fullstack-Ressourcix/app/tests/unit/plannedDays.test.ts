import { describe, expect, it } from 'vitest'
import { RequestStatus } from '../../src/stores/request'
import { plannedDaysForEmployee } from '../../src/utils/plannedDays'

describe('plannedDaysForEmployee', () => {
  const requests = [
    { employeeId: 'emp-1', status: RequestStatus.Open, from: '2026-07-13', until: '2026-07-20' }, // 8 Tage
    { employeeId: 'emp-1', status: RequestStatus.Approved, from: '2026-08-01', until: '2026-08-03' }, // 3 Tage
    { employeeId: 'emp-1', status: RequestStatus.Rejected, from: '2026-09-01', until: '2026-09-10' },
    { employeeId: 'emp-1', status: RequestStatus.Cancelled, from: '2026-09-15', until: '2026-09-20' },
    { employeeId: 'emp-1', status: RequestStatus.Taken, from: '2026-06-01', until: '2026-06-05' },
    { employeeId: 'emp-2', status: RequestStatus.Open, from: '2026-07-13', until: '2026-07-20' },
  ]

  it('summiert nur offene und genehmigte Anträge des angegebenen Mitarbeiters', () => {
    expect(plannedDaysForEmployee(requests, 'emp-1')).toBe(11)
  })

  it('ignoriert abgelehnte, stornierte und bereits bezogene Anträge', () => {
    const onlyRejectedEtc = requests.filter(r => r.employeeId === 'emp-1' && r.status !== RequestStatus.Open && r.status !== RequestStatus.Approved)
    expect(plannedDaysForEmployee(onlyRejectedEtc, 'emp-1')).toBe(0)
  })

  it('gibt 0 zurück, wenn der Mitarbeiter keine Anträge hat', () => {
    expect(plannedDaysForEmployee(requests, 'emp-3')).toBe(0)
  })
})
