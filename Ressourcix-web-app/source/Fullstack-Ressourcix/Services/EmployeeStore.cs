namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class EmployeeStore
{
  private readonly AppDbContext _db;
  private readonly AuditLogStore _auditLog;
  private readonly ILogger<EmployeeStore> _logger;

  public EmployeeStore(AppDbContext db, AuditLogStore auditLog, ILogger<EmployeeStore> logger)
  {
    _db = db;
    _auditLog = auditLog;
    _logger = logger;
  }

  public Task<List<Employee>> AllAsync() => _db.Employees.AsNoTracking().ToListAsync();

  public Task<Employee?> FindByUsernameAsync(string username) =>
    _db.Employees.FirstOrDefaultAsync(e => e.Username == username);

  public Task<bool> UsernameExistsAsync(string username) =>
    _db.Employees.AnyAsync(e => e.Username == username);

  public async Task<Employee> CreateAsync(Employee employee, string actor)
  {
    employee.Id = Guid.NewGuid();
    _db.Employees.Add(employee);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Mitarbeiter erstellt: {EmployeeId}", employee.Id);

    await _auditLog.RecordAsync(
      AuditLogAction.EmployeeCreated,
      employee.Id,
      actor,
      $"Mitarbeiter erfasst: {employee.Name}"
    );
    return employee;
  }

  public async Task<bool> ToggleActiveAsync(Guid id, string actor)
  {
    var employee = await _db.Employees.FindAsync(id);
    if (employee is null)
      return false;

    var wasActive = employee.IsActive;
    employee.IsActive = !wasActive;
    await _db.SaveChangesAsync();
    _logger.LogInformation(
      "Mitarbeiter-Status umgeschaltet: {EmployeeId} {WasActive} -> {IsActive}",
      id,
      wasActive,
      !wasActive
    );

    await _auditLog.RecordAsync(
      AuditLogAction.EmployeeStatusChanged,
      id,
      actor,
      $"Status geändert für {employee.Name}: {ActiveLabel(wasActive)} → {ActiveLabel(!wasActive)}"
    );
    return true;
  }

  public async Task<bool> UpdateAsync(Guid id, UpdateEmployeeRequest updated, string actor)
  {
    var employee = await _db.Employees.FindAsync(id);
    if (employee is null)
      return false;

    var newPermissionLevel = Employee.PermissionLevelForRole(updated.role);
    var changeText = AuditSummaryBuilder.BuildEmployeeUpdateSummary(
      employee,
      updated,
      newPermissionLevel
    );

    employee.Name = updated.name;
    employee.Role = updated.role;
    employee.Workload = updated.workload;
    employee.VacationDays = updated.vacationDays;
    employee.PermissionLevel = newPermissionLevel;
    await _db.SaveChangesAsync();
    _logger.LogInformation("Mitarbeiter aktualisiert: {EmployeeId}", id);

    await _auditLog.RecordAsync(
      AuditLogAction.EmployeeUpdated,
      id,
      actor,
      $"Mitarbeiter bearbeitet: {updated.name} — {changeText}"
    );
    return true;
  }

  public async Task<bool> DeleteAsync(Guid id, string actor)
  {
    var employee = await _db.Employees.FindAsync(id);
    if (employee is null)
      return false;

    _db.Employees.Remove(employee);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Mitarbeiter gelöscht: {EmployeeId}", id);

    await _auditLog.RecordAsync(
      AuditLogAction.EmployeeDeleted,
      id,
      actor,
      $"Mitarbeiter gelöscht: {employee.Name}"
    );
    return true;
  }

  private static string ActiveLabel(bool isActive) => isActive ? "Aktiv" : "Deaktiviert";
}
