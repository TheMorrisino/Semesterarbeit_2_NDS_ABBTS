import { describe, expect, it } from 'vitest'
import { RequestStatus } from '@/stores/request'
import { statusMeta } from './statusMeta'

const identityT = (key: string) => key

describe('statusMeta', () => {
  it('derives the i18n key from the lowercased status', () => {
    expect(statusMeta(RequestStatus.Open, identityT).label).toBe('status.open')
    expect(statusMeta(RequestStatus.Approved, identityT).label).toBe('status.approved')
  })

  it('returns a stable color per status', () => {
    expect(statusMeta(RequestStatus.Open, identityT).color).toBe('orange')
    expect(statusMeta(RequestStatus.Approved, identityT).color).toBe('green')
    expect(statusMeta(RequestStatus.Rejected, identityT).color).toBe('red')
    expect(statusMeta(RequestStatus.Taken, identityT).color).toBe('blue')
    expect(statusMeta(RequestStatus.Cancelled, identityT).color).toBe('grey')
  })

  it('returns a stable icon per status', () => {
    expect(statusMeta(RequestStatus.Open, identityT).icon).toBe('🕒')
    expect(statusMeta(RequestStatus.Approved, identityT).icon).toBe('✅')
    expect(statusMeta(RequestStatus.Rejected, identityT).icon).toBe('❌')
    expect(statusMeta(RequestStatus.Taken, identityT).icon).toBe('🏖')
    expect(statusMeta(RequestStatus.Cancelled, identityT).icon).toBe('🚫')
  })

  it('calls the given translator with the lowercased status key', () => {
    let receivedKey = ''
    statusMeta(RequestStatus.Taken, key => {
      receivedKey = key
      return 'irrelevant'
    })
    expect(receivedKey).toBe('status.taken')
  })
})
