namespace FullstackRessourcix;

public class Employee
{
  public Guid id { get; set; } = Guid.NewGuid();
  public string name { get; set; } = "";
  public string role { get; set; } = "";
  public int workload { get; set; }
  public double vacationDays { get; set; }
  public bool isActive { get; set; } = true;

  public string username { get; set; } = "";
  public string passwordHash { get; set; } = "";
  public bool mustChangePassword { get; set; } = true;
  public int permissionLevel { get; set; }

  // Berechtigungslevel wird ausschliesslich über die Rolle vergeben, nie direkt vom Client gesetzt
  // (siehe CreateEmployeeRequest/UpdateEmployeeRequest, die kein permissionLevel-Feld mehr haben).
  public static int PermissionLevelForRole(string role) =>
    EmployeeRoles.TryGetPermissionLevel(role, out var level)
      ? level
      : throw new ArgumentException($"Unbekannte Rolle: '{role}'.", nameof(role));
}
