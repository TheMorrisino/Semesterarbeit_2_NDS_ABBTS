// Ein Zeichen pro Namensteil, z.B. "Morris Meier" -> "MM".
export function initialsFor (name: string): string {
  return name
    .split(' ')
    .filter(Boolean)
    .map(part => part[0])
    .join('')
    .toUpperCase()
}

// Kürzel je Mitarbeiter-ID, konsistent mit initialsFor() (z.B. für die Mitarbeiterverwaltung).
// Bei einem Kollisions-Kürzel (z.B. zwei "MM") wird an die zweite und jede weitere Person eine
// fortlaufende Nummer angehängt ("MM", "MM2", "MM3", ...), abhängig von der Reihenfolge in `employees`.
export function uniqueInitials (employees: { id: string, name: string }[]): Record<string, string> {
  const counts: Record<string, number> = {}
  const result: Record<string, string> = {}
  for (const employee of employees) {
    const base = initialsFor(employee.name)
    const count = (counts[base] ?? 0) + 1
    counts[base] = count
    result[employee.id] = count === 1 ? base : `${base}${count}`
  }
  return result
}
