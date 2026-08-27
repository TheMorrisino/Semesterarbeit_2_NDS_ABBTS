export function rangesOverlap (aFrom: string, aUntil: string, bFrom: string, bUntil: string): boolean {
  return aFrom <= bUntil && aUntil >= bFrom
}

export function isEmployeeAbsentOn (
  requests: { employeeId: string, from: string, until: string }[],
  employeeId: string,
  iso: string,
): boolean {
  return requests.some(
    request => request.employeeId === employeeId && iso >= request.from && iso <= request.until,
  )
}

// Ist `iso` innerhalb von [request.from, request.until]? Reine String-basierte Prüfung, damit sie
// (anders als ein new Date(request.until)-Vergleich) unabhängig von Zeitzone/Uhrzeit korrekt ist.
export function isRequestActiveOn (request: { from: string, until: string }, iso: string): boolean {
  return rangesOverlap(iso, iso, request.from, request.until)
}
