namespace FullstackRessourcix;

public sealed record EmployeeResponse(
  Guid id,
  string name,
  string role,
  int workload,
  double vacationDays,
  bool isActive,
  string username,
  int permissionLevel,
  bool mustChangePassword
)
{
  public static EmployeeResponse From(Employee e) =>
    new(
      e.Id,
      e.Name,
      e.Role,
      e.Workload,
      e.VacationDays,
      e.IsActive,
      e.Username,
      e.PermissionLevel,
      e.MustChangePassword
    );
}

public sealed record CreateEmployeeRequest(
  string name,
  string role,
  int workload,
  double vacationDays,
  string username
);

public sealed record UpdateEmployeeRequest(
  string name,
  string role,
  int workload,
  double vacationDays
);
