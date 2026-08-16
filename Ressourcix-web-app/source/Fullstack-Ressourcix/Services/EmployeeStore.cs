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
        _db.Employees.FirstOrDefaultAsync(e => e.username == username);

    public Task<bool> UsernameExistsAsync(string username) =>
        _db.Employees.AnyAsync(e => e.username == username);

    public async Task<Employee> CreateAsync(Employee employee, string actor)
    {
        employee.id = Guid.NewGuid();
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Mitarbeiter erstellt: {EmployeeId}", employee.id);

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.EmployeeCreated,
            Summary = $"Mitarbeiter erfasst: {employee.name}",
            Reference = employee.id,
            Actor = actor,
        });
        return employee;
    }

    public async Task<bool> ToggleActiveAsync(Guid id, string actor)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;

        var wasActive = employee.isActive;
        employee.isActive = !wasActive;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Mitarbeiter-Status umgeschaltet: {EmployeeId} {WasActive} -> {IsActive}", id, wasActive, !wasActive);

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.EmployeeStatusChanged,
            Summary = $"Status geändert für {employee.name}: {ActiveLabel(wasActive)} → {ActiveLabel(!wasActive)}",
            Reference = id,
            Actor = actor,
        });
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateEmployeeRequest updated, string actor)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;

        var newPermissionLevel = Employee.PermissionLevelForRole(updated.role);
        var changeText = BuildUpdateSummary(employee, updated, newPermissionLevel);

        employee.name = updated.name;
        employee.role = updated.role;
        employee.workload = updated.workload;
        employee.vacationDays = updated.vacationDays;
        employee.permissionLevel = newPermissionLevel;
        await _db.SaveChangesAsync();
        _logger.LogInformation("Mitarbeiter aktualisiert: {EmployeeId}", id);

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.EmployeeUpdated,
            Summary = $"Mitarbeiter bearbeitet: {updated.name} — {changeText}",
            Reference = id,
            Actor = actor,
        });
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id, string actor)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;

        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
        _logger.LogInformation("Mitarbeiter gelöscht: {EmployeeId}", id);

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.EmployeeDeleted,
            Summary = $"Mitarbeiter gelöscht: {employee.name}",
            Reference = id,
            Actor = actor,
        });
        return true;
    }

    private static string ActiveLabel(bool isActive) => isActive ? "Aktiv" : "Deaktiviert";

    private static string BuildUpdateSummary(Employee before, UpdateEmployeeRequest after, int newPermissionLevel)
    {
        var changes = new List<string>();
        if (before.name != after.name) changes.Add($"Name: {before.name} → {after.name}");
        if (before.role != after.role) changes.Add($"Rolle: {before.role} → {after.role}");
        if (before.workload != after.workload) changes.Add($"Pensum: {before.workload} → {after.workload}");
        if (before.vacationDays != after.vacationDays) changes.Add($"Ferientage: {before.vacationDays} → {after.vacationDays}");
        if (before.permissionLevel != newPermissionLevel) changes.Add($"Berechtigungslevel: {before.permissionLevel} → {newPermissionLevel}");
        return changes.Count > 0 ? string.Join(", ", changes) : "keine Änderung";
    }
}
