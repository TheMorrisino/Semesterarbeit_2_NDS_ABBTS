import { describe, expect, it, vi, beforeEach, afterEach } from 'vitest'

import { employeesApi } from '../../src/api/employees'

describe('Employee API', () => {
  beforeEach(() => {
    vi.stubGlobal('fetch', vi.fn())
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('erstellt einen neuen Mitarbeiter über die API', async () => {
    const createdEmployee = {
      id: '11111111-1111-1111-1111-111111111111',
      name: 'Max Muster',
      username: 'max.muster',
      role: 'Mitarbeiter',
      workload: 100,
      vacationDays: 25,
      isActive: true,
      permissionLevel: 1,
      mustChangePassword: true,
    }

    vi.mocked(fetch).mockResolvedValue(
      new Response(JSON.stringify(createdEmployee), {
        status: 201,
        headers: {
          'Content-Type': 'application/json',
        },
      }),
    )

    const employee = {
      name: 'Max Muster',
      username: 'max.muster',
      role: 'Mitarbeiter',
      workload: 100,
      vacationDays: 25,
      isActive: true,
      permissionLevel: 1,
    }

    const result = await employeesApi.create(employee)

    expect(fetch).toHaveBeenCalledTimes(1)

    const [url, options] = vi.mocked(fetch).mock.calls[0]

    expect(url).toContain('/api/employees')
    expect(options?.method).toBe('POST')

    expect(JSON.parse(options?.body as string)).toEqual(employee)

    expect(result).toEqual(createdEmployee)
  })
})