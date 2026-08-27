namespace FullstackRessourcix;

public sealed record AuditLogResponse(
  Guid id,
  AuditLogAction action,
  string summary,
  Guid reference,
  string actor,
  DateTime timestamp
)
{
  public static AuditLogResponse From(AuditLogEntry e) =>
    new(e.Id, e.Action, e.Summary, e.Reference, e.Actor, e.Timestamp);
}
