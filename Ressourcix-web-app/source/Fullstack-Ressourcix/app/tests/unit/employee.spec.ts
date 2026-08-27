import { createPinia, setActivePinia } from 'pinia'
import { beforeEach, describe, expect, it, vi } from 'vitest'

import { employeesApi } from '../../src/api/employees'
import { useEmployeeStore } from '../../src/stores/employee'

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

  it('findet einen Mitarbeiter anhand der ID, nicht anhand des Namens', () => {
    const store = useEmployeeStore()
    store.employees = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        name: 'Max Muster',
        username: 'max.muster',
        role: 'Mitarbeiter',
        workload: 100,
        vacationDays: 25,
        isActive: true,
        permissionLevel: 1,
      },
      {
        id: '22222222-2222-2222-2222-222222222222',
        name: 'Max Muster',
        username: 'max.muster2',
        role: 'Mitarbeiter',
        workload: 100,
        vacationDays: 20,
        isActive: true,
        permissionLevel: 1,
      },
    ]

    const found = store.employeeById('22222222-2222-2222-2222-222222222222')

    expect(found?.id).toBe('22222222-2222-2222-2222-222222222222')
    expect(found?.vacationDays).toBe(20)
  })

  it('behält beim Bearbeiten Felder wie mustChangePassword, die im Update-Payload fehlen', async () => {
    const store = useEmployeeStore()
    store.employees = [
      {
        id: '11111111-1111-1111-1111-111111111111',
        name: 'Max Muster',
        username: 'max.muster',
        role: 'Mitarbeiter',
        workload: 100,
        vacationDays: 25,
        isActive: true,
        permissionLevel: 1,
        mustChangePassword: true,
      },
    ]

    vi.mocked(employeesApi.update).mockResolvedValue(undefined)

    await store.update('11111111-1111-1111-1111-111111111111', {
      name: 'Max Muster',
      username: 'max.muster',
      role: 'Mitarbeiter',
      workload: 100,
      vacationDays: 25,
      isActive: true,
      permissionLevel: 1,
    })

    expect(store.employees[0].mustChangePassword).toBe(true)
  })
})
