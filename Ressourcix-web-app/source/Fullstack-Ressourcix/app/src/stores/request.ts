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

type CreateRequestPayload = Pick<Request, 'employeeId' | 'from' | 'until' | 'type' | 'remark'>

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
      // Der Server kann Felder abweichend vom Payload setzen (z.B. Status serverseitig auf Open
      // zurücksetzen, wenn sich das Enddatum ändert) - deshalb die Response übernehmen, nicht die
      // gesendeten Werte annehmen.
      const updated = await requestsApi.update(id, until, status)
      const index = this.requests.findIndex(r => r.id === id)
      if (index !== -1) {
        this.requests[index] = updated
      }
    },
    async remove (id: string) {
      await requestsApi.remove(id)
      this.requests = this.requests.filter(r => r.id !== id)
    },
  },
})
