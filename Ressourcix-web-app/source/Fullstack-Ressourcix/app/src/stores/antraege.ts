import { defineStore } from 'pinia'
import type { Ferienantrag, CreateFerienantrag } from '@/types'
//import { ferienantraegeApi } from '@/api/ferienantraege'

export const useAntraegeStore = defineStore('antraege', {
  state: () => ({ meine: [] as Ferienantrag[], offen: [] as Ferienantrag[], laedt: false }),
  actions: {
    async ladeMeine() { this.laedt = true; this.meine = await ferienantraegeApi.meine(); this.laedt = false },
    async ladeOffen() { this.offen = await ferienantraegeApi.offen() },
    async erfassen(dto: CreateFerienantrag) { this.meine.unshift(await ferienantraegeApi.erfassen(dto)) },
    async entscheiden(id: string, genehmigt: boolean, grund?: string) {
      await ferienantraegeApi.entscheiden(id, genehmigt, grund)
      this.offen = this.offen.filter((a) => a.id !== id)
    }
  }
})
