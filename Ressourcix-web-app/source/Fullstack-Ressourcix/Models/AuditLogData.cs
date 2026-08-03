namespace FullstackRessourcix;


public enum AuditLogAction {
    ApplicationCreated,
    ApplicationUpdated,
    ApplicationDeleted,
    EmployeeCreated,
    EmployeeUpdated
}
public class AuditLogEntry
{
  public Guid id { get; set; } = Guid.NewGuid();
  public AuditLogAction action { get; set; }
  public string summary { get; set; } = "";
  public int reference { get; set; }
  public double actor { get; set; }
  public bool time { get; set; } = true;

}
