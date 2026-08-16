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
    if (before.name != after.name)
      changes.Add($"Name: {before.name} → {after.name}");
    if (before.role != after.role)
      changes.Add($"Rolle: {before.role} → {after.role}");
    if (before.workload != after.workload)
      changes.Add($"Pensum: {before.workload} → {after.workload}");
    if (before.vacationDays != after.vacationDays)
      changes.Add($"Ferientage: {before.vacationDays} → {after.vacationDays}");
    if (before.permissionLevel != newPermissionLevel)
      changes.Add($"Berechtigungslevel: {before.permissionLevel} → {newPermissionLevel}");
    return Join(changes);
  }

  public static string BuildRequestUpdateSummary(
    Request before,
    DateOnly until,
    RequestStatus status
  )
  {
    var changes = new List<string>();
    if (before.until != until)
      changes.Add($"Ende: {FormatDate(before.until)} → {FormatDate(until)}");
    if (before.status != status)
      changes.Add($"Status: {StatusLabels[before.status]} → {StatusLabels[status]}");
    return Join(changes);
  }

  public static string BuildRequestStatusChangeSummary(RequestStatus before, RequestStatus after) =>
    $"Status: {StatusLabels[before]} → {StatusLabels[after]}";

  private static string Join(List<string> changes) =>
    changes.Count > 0 ? string.Join(", ", changes) : "keine Änderung";
}
