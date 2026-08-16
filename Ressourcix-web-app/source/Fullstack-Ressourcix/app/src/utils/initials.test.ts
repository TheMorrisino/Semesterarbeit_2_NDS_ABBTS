import { describe, expect, it } from 'vitest'
import { avatarColor, initialsFor, uniqueInitials } from './initials'

describe('initialsFor', () => {
  it('takes one character per name part', () => {
    expect(initialsFor('Morris Meier')).toBe('MM')
  })

  it('ignores repeated whitespace between name parts', () => {
    expect(initialsFor('Morris  Meier')).toBe('MM')
  })
})

describe('uniqueInitials', () => {
  it('appends a counter starting from the second collision', () => {
    const result = uniqueInitials([
      { id: '1', name: 'Morris Meier' },
      { id: '2', name: 'Mira Muster' },
    ])
    expect(result).toEqual({ 1: 'MM', 2: 'MM2' })
  })
})

describe('avatarColor', () => {
  it('returns the same color for the same name', () => {
    expect(avatarColor('Morris Meier')).toBe(avatarColor('Morris Meier'))
  })

  it('returns one of the defined palette colors', () => {
    expect(['purple', 'red', 'blue', 'teal', 'indigo']).toContain(avatarColor('Morris Meier'))
  })
})
