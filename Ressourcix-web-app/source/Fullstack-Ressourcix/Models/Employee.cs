namespace FullstackRessourcix;

public class Employee
{
  public Guid Id { get; set; } = Guid.NewGuid();
  public string Name { get; set; } = "";
  public string Role { get; set; } = "";
  public int Workload { get; set; }
  public double VacationDays { get; set; }
  public bool IsActive { get; set; } = true;

  public string Username { get; set; } = "";
  public string PasswordHash { get; set; } = "";
  public bool MustChangePassword { get; set; } = true;
  public int PermissionLevel { get; set; }

  // Berechtigungslevel wird ausschliesslich über die Rolle vergeben, nie direkt vom Client gesetzt
  // (siehe CreateEmployeeRequest/UpdateEmployeeRequest, die kein permissionLevel-Feld mehr haben).
  public static int PermissionLevelForRole(string role) =>
    EmployeeRoles.TryGetPermissionLevel(role, out var level)
      ? level
      : throw new ArgumentException($"Unbekannte Rolle: '{role}'.", nameof(role));
}
