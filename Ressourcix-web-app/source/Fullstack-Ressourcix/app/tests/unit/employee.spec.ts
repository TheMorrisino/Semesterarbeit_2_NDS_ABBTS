import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'

import { useEmployeeStore } from '../../src/stores/employee'
import { employeesApi } from '../../src/api/employees'

vi.mock('../../src/api/employees', () => ({
  employeesApi: {
    list: vi.fn(),
    create: vi.fn(),
    update: vi.fn(),
    toggleActive: vi.fn(),
    remove: vi.fn(),
  },
}))

describe('Employee Store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('legt einen neuen Mitarbeiter im Store an', async () => {
    const store = useEmployeeStore()

    const employee = {
      name: 'Max Muster',
      username: 'max.muster',
      role: 'Mitarbeiter',
      workload: 100,
      vacationDays: 25,
      isActive: true,
      permissionLevel: 1,
    }

    const createdEmployee = {
      id: '11111111-1111-1111-1111-111111111111',
      ...employee,
      mustChangePassword: true,
    }

    vi.mocked(employeesApi.create).mockResolvedValue(createdEmployee)

    const result = await store.create(employee)

    expect(employeesApi.create).toHaveBeenCalledTimes(1)
    expect(employeesApi.create).toHaveBeenCalledWith(employee)

    expect(result).toEqual(createdEmployee)
    expect(store.employees).toHaveLength(1)
    expect(store.employees[0]).toEqual(createdEmployee)
  })
})