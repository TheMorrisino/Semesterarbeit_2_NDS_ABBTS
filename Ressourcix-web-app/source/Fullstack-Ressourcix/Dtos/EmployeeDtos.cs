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
      e.id,
      e.name,
      e.role,
      e.workload,
      e.vacationDays,
      e.isActive,
      e.username,
      e.permissionLevel,
      e.mustChangePassword
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
