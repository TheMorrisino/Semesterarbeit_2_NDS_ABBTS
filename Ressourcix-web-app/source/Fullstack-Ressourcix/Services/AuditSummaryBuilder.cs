namespace FullstackRessourcix;

// Formuliert die "Was hat sich geändert"-Texte für Audit-Log-Einträge in lesbarem Deutsch.
// Zustandslos und unabhängig von EF/Persistenz, damit sie ohne DbContext testbar ist.
public static class AuditSummaryBuilder
{
  private static readonly Dictionary<RequestStatus, string> StatusLabels = new()
  {
    [RequestStatus.Open] = "Ausstehend",
    [RequestStatus.Approved] = "Genehmigt",
    [RequestStatus.Rejected] = "Abgelehnt",
    [RequestStatus.Taken] = "Bezogen",
    [RequestStatus.Cancelled] = "Storniert",
  };

  public static string FormatDate(DateOnly date) => date.ToString("dd.MM.yyyy");

  public static string BuildEmployeeUpdateSummary(
    Employee before,
    UpdateEmployeeRequest after,
    int newPermissionLevel
  )
  {
    var changes = new List<string>();
    if (before.Name != after.name)
      changes.Add($"Name: {before.Name} → {after.name}");
    if (before.Role != after.role)
      changes.Add($"Rolle: {before.Role} → {after.role}");
    if (before.Workload != after.workload)
      changes.Add($"Pensum: {before.Workload} → {after.workload}");
    if (before.VacationDays != after.vacationDays)
      changes.Add($"Ferientage: {before.VacationDays} → {after.vacationDays}");
    if (before.PermissionLevel != newPermissionLevel)
      changes.Add($"Berechtigungslevel: {before.PermissionLevel} → {newPermissionLevel}");
    return Join(changes);
  }

  public static string BuildRequestUpdateSummary(
    AbsenceRequest before,
    DateOnly until,
    RequestStatus status
  )
  {
    var changes = new List<string>();
    if (before.Until != until)
      changes.Add($"Ende: {FormatDate(before.Until)} → {FormatDate(until)}");
    if (before.Status != status)
      changes.Add($"Status: {StatusLabels[before.Status]} → {StatusLabels[status]}");
    return Join(changes);
  }

  public static string BuildRequestStatusChangeSummary(RequestStatus before, RequestStatus after) =>
    $"Status: {StatusLabels[before]} → {StatusLabels[after]}";

  private static string Join(List<string> changes) =>
    changes.Count > 0 ? string.Join(", ", changes) : "keine Änderung";
}
