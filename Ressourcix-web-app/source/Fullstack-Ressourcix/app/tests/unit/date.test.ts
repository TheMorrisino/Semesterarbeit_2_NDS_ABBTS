import { describe, expect, it } from 'vitest'
import {
  addDays,
  addMonths,
  addYears,
  buildDayRange,
  daysBetweenInclusive,
  formatDate,
  isWeekend,
  startOfWeek,
  toISODate,
} from '../../src/utils/date'

describe('toISODate', () => {
  it('formats a date as YYYY-MM-DD with zero-padding', () => {
    expect(toISODate(new Date(2026, 0, 5))).toBe('2026-01-05')
  })
})

describe('formatDate', () => {
  it('formats an ISO date as DD.MM.YYYY', () => {
    expect(formatDate('2026-01-05')).toBe('05.01.2026')
  })
})

describe('daysBetweenInclusive', () => {
  it('counts both start and end day', () => {
    expect(daysBetweenInclusive('2026-07-13', '2026-07-13')).toBe(1)
    expect(daysBetweenInclusive('2026-07-13', '2026-07-24')).toBe(12)
  })
})

describe('addDays/addMonths/addYears', () => {
  it('adds days without mutating the input', () => {
    const input = new Date(2026, 0, 31)
    const result = addDays(input, 1)
    expect(toISODate(result)).toBe('2026-02-01')
    expect(toISODate(input)).toBe('2026-01-31')
  })

  it('adds months, rolling over into the next year', () => {
    expect(toISODate(addMonths(new Date(2026, 11, 15), 1))).toBe('2027-01-15')
  })

  it('adds years', () => {
    expect(toISODate(addYears(new Date(2026, 5, 1), 1))).toBe('2027-06-01')
  })
})

describe('isWeekend', () => {
  it('returns true for Saturday and Sunday', () => {
    expect(isWeekend(new Date(2026, 6, 18))).toBe(true) // Samstag
    expect(isWeekend(new Date(2026, 6, 19))).toBe(true) // Sonntag
  })

  it('returns false for a weekday', () => {
    expect(isWeekend(new Date(2026, 6, 20))).toBe(false) // Montag
  })
})

describe('startOfWeek', () => {
  it('returns the Monday of the given week', () => {
    expect(toISODate(startOfWeek(new Date(2026, 6, 18)))).toBe('2026-07-13')
  })

  it('stays on Monday if the date already is one', () => {
    expect(toISODate(startOfWeek(new Date(2026, 6, 13)))).toBe('2026-07-13')
  })
})

describe('buildDayRange', () => {
  it('builds an inclusive list of days between start and end', () => {
    const range = buildDayRange(new Date(2026, 6, 13), new Date(2026, 6, 15))
    expect(range.map(d => toISODate(d))).toEqual(['2026-07-13', '2026-07-14', '2026-07-15'])
  })

  it('returns a single day when start equals end', () => {
    const range = buildDayRange(new Date(2026, 6, 13), new Date(2026, 6, 13))
    expect(range.map(d => toISODate(d))).toEqual(['2026-07-13'])
  })
})
