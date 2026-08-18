// Gemeinsame Überschneidungslogik für Ferienanträge, verwendet von CalenderView.vue und DashboardView.vue.

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
