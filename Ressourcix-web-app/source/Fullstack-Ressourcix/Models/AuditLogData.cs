namespace FullstackRessourcix;

public enum AuditLogAction
{
    RequestCreated,
    RequestUpdated,
    RequestDeleted,
    EmployeeCreated,
    EmployeeUpdated,
    EmployeeStatusChanged,
    EmployeeDeleted
}

public class AuditLogEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public AuditLogAction Action { get; set; }
    public string Summary { get; set; } = "";

    // Guid der betroffenen Entität (Request oder Employee), damit ein Eintrag wirklich nachvollziehbar bleibt
    public Guid Reference { get; set; }

    // Anzeigename statt Guid, da es noch kein echtes Login/Session gibt, über das man das auflösen könnte
    public string Actor { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
