import { describe, expect, it } from 'vitest'
import { isEmployeeAbsentOn, isRequestActiveOn, rangesOverlap } from '../../src/utils/overlap'

describe('rangesOverlap', () => {
  it('returns true when ranges overlap', () => {
    expect(rangesOverlap('2026-07-13', '2026-07-20', '2026-07-18', '2026-07-25')).toBe(true)
  })

  it('returns true when one range fully contains the other', () => {
    expect(rangesOverlap('2026-07-01', '2026-07-31', '2026-07-13', '2026-07-20')).toBe(true)
  })

  it('returns true when ranges touch on a single boundary day', () => {
    expect(rangesOverlap('2026-07-13', '2026-07-20', '2026-07-20', '2026-07-25')).toBe(true)
  })

  it('returns false when ranges do not overlap', () => {
    expect(rangesOverlap('2026-07-01', '2026-07-05', '2026-07-10', '2026-07-15')).toBe(false)
  })
})

describe('isEmployeeAbsentOn', () => {
  const requests = [
    { employeeId: 'emp-1', from: '2026-07-13', until: '2026-07-20' },
    { employeeId: 'emp-2', from: '2026-08-01', until: '2026-08-05' },
  ]

  it('returns true when the day falls within one of the employee\'s requests', () => {
    expect(isEmployeeAbsentOn(requests, 'emp-1', '2026-07-15')).toBe(true)
  })

  it('returns true on the boundary days', () => {
    expect(isEmployeeAbsentOn(requests, 'emp-1', '2026-07-13')).toBe(true)
    expect(isEmployeeAbsentOn(requests, 'emp-1', '2026-07-20')).toBe(true)
  })

  it('returns false outside the request range', () => {
    expect(isEmployeeAbsentOn(requests, 'emp-1', '2026-07-21')).toBe(false)
  })

  it('returns false for a different employee\'s request', () => {
    expect(isEmployeeAbsentOn(requests, 'emp-2', '2026-07-15')).toBe(false)
  })

  it('returns false when the employee has no requests at all', () => {
    expect(isEmployeeAbsentOn(requests, 'emp-3', '2026-07-15')).toBe(false)
  })
})

describe('isRequestActiveOn', () => {
  it('returns true when the day is the request\'s last day, regardless of time of day', () => {
    const request = { from: '2026-07-13', until: '2026-07-20' }
    expect(isRequestActiveOn(request, '2026-07-20')).toBe(true)
  })

  it('returns false the day after the request ends', () => {
    const request = { from: '2026-07-13', until: '2026-07-20' }
    expect(isRequestActiveOn(request, '2026-07-21')).toBe(false)
  })
})
