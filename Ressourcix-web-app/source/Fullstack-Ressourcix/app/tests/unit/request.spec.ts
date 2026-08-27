import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { requestsApi } from '../../src/api/requests'
import { AbsenceType, RequestStatus, useRequestStore } from '../../src/stores/request'

vi.mock('../../src/api/requests', () => ({
  requestsApi: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    remove: vi.fn(),
  },
}))

describe('Request Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('übernimmt nach dem Bearbeiten den vom Server zurückgegebenen Status, nicht den gesendeten', async () => {
    const store = useRequestStore()
    store.requests = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        employeeId: '22222222-2222-2222-2222-222222222222',
        from: '2026-07-01',
        until: '2026-07-05',
        days: 5,
        overlap: false,
        status: RequestStatus.Approved,
        submittedOn: '2026-06-01T00:00:00Z',
        type: AbsenceType.Vacation,
        remark: null,
      },
    ]

    // Backend hat wegen der Enddatum-Änderung den Status serverseitig auf Open zurückgesetzt,
    // obwohl der Aufrufer (wie bisher üblich) weiterhin "Approved" mitschickt.
    vi.mocked(requestsApi.update).mockResolvedValue({
      id: '11111111-1111-1111-1111-111111111111',
      employeeId: '22222222-2222-2222-2222-222222222222',
      from: '2026-07-01',
      until: '2026-07-10',
      days: 10,
      overlap: false,
      status: RequestStatus.Open,
      submittedOn: '2026-06-01T00:00:00Z',
      type: AbsenceType.Vacation,
      remark: null,
    })

    await store.update('11111111-1111-1111-1111-111111111111', '2026-07-10', RequestStatus.Approved)

    expect(store.requests[0].status).toBe(RequestStatus.Open)
    expect(store.requests[0].until).toBe('2026-07-10')
  })
})
