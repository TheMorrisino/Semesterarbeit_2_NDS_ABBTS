// Gemeinsame Datumshilfsfunktionen, verwendet von CalenderView.vue und DashboardView.vue.

export function addDays (date: Date, days: number): Date {
  const result = new Date(date)
  result.setDate(result.getDate() + days)
  return result
}

export function addMonths (date: Date, months: number): Date {
  const result = new Date(date)
  result.setMonth(result.getMonth() + months)
  return result
}

export function addYears (date: Date, years: number): Date {
  const result = new Date(date)
  result.setFullYear(result.getFullYear() + years)
  return result
}

export function buildDayRange (start: Date, end: Date): Date[] {
  const days: Date[] = []
  let cursor = new Date(start)
  while (cursor <= end) {
    days.push(new Date(cursor))
    cursor = addDays(cursor, 1)
  }
  return days
}

export function toISODate (date: Date): string {
  const year = date.getFullYear()
  const month = String(date.getMonth() + 1).padStart(2, '0')
  const day = String(date.getDate()).padStart(2, '0')
  return `${year}-${month}-${day}`
}

export function formatDate (iso: string): string {
  const [year, month, day] = iso.split('-')
  return `${day}.${month}.${year}`
}

export function isWeekend (date: Date): boolean {
  const day = date.getDay()
  return day === 0 || day === 6
}

export function daysBetweenInclusive (startIso: string, endIso: string): number {
  const start = new Date(startIso)
  const end = new Date(endIso)
  return Math.round((end.getTime() - start.getTime()) / 86_400_000) + 1
}

export function startOfWeek (date: Date): Date {
  const result = new Date(date)
  const mondayIndex = (result.getDay() + 6) % 7 // Montag = 0 ... Sonntag = 6
  result.setDate(result.getDate() - mondayIndex)
  return result
}
