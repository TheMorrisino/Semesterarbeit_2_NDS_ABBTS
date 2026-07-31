export type Rolle = 'Mitarbeitende' | 'PlannerLeitung' | 'HR' | 'IT'
export type AntragStatus = 'Ausstehend' | 'Genehmigt' | 'Abgelehnt' | 'Bezogen' | 'Storniert'
export type AbwesenheitTyp = 'Ferien' | 'Kompensation' | 'UnbezahlterUrlaub'

export interface Benutzer {
  id: string
  vorname: string
  nachname: string
  rolle: Rolle
  pensum: number
  ferienwochen: number
  aktiv: boolean
}

export interface Ferienantrag {
  id: string
  mitarbeiterName: string
  typ: AbwesenheitTyp
  von: string
  bis: string
  anzahlArbeitstage: number
  status: AntragStatus
  hatUeberschneidung: boolean
}

export interface CreateFerienantrag {
  typ: AbwesenheitTyp
  von: string
  bis: string
  bemerkung?: string
}
