import { defineStore } from 'pinia'
import { requestsApi } from '@/api/requests'

export enum RequestStatus {
  Open = 'Open',
  Approved = 'Approved',
  Rejected = 'Rejected',
  Taken = 'Taken',
  Cancelled = 'Cancelled',
}

export enum AbsenceType {
  Vacation = 'Vacation',
  Compensation = 'Compensation',
  UnpaidLeave = 'UnpaidLeave',
}

export interface Request {
  id: string // Guid
  employeeId: string // Guid
  from: string // ISO date
  until: string // ISO date
  days: number
  overlap: boolean
  status: RequestStatus
  submittedOn: string // ISO datetime
  type: AbsenceType
  remark: string | null
}

type CreateRequestPayload = Pick<Request, 'employeeId' | 'from' | 'until' | 'days' | 'overlap' | 'type' | 'remark'>

export const useRequestStore = defineStore('requests', {
  state: () => ({
    requests: [] as Request[],
    loading: false,
  }),
  actions: {
    async load (status?: 'open') {
      this.loading = true
      try {
        this.requests = await requestsApi.list(status)
      } finally {
        this.loading = false
      }
    },
    async create (request: CreateRequestPayload) {
      const created = await requestsApi.create(request)
      this.requests.unshift(created)
      return created
    },
    async update (id: string, until: string, status: RequestStatus) {
      await requestsApi.update(id, until, status)
      const request = this.requests.find(r => r.id === id)
      if (request) {
        request.until = until
        request.status = status
      }
    },
    async approve (id: string) {
      await requestsApi.approve(id)
      const request = this.requests.find(r => r.id === id)
      if (request) {
        request.status = RequestStatus.Approved
      }
    },
    async reject (id: string) {
      await requestsApi.reject(id)
      const request = this.requests.find(r => r.id === id)
      if (request) {
        request.status = RequestStatus.Rejected
      }
    },
    async remove (id: string) {
      await requestsApi.remove(id)
      this.requests = this.requests.filter(r => r.id !== id)
    },
  },
})
