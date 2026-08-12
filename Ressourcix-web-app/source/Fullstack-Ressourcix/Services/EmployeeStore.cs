namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class EmployeeStore
{
    private readonly AppDbContext _db;

    public EmployeeStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Employee>> AllAsync() => _db.Employees.AsNoTracking().ToListAsync();

    public Task<Employee?> FindByUsernameAsync(string username) =>
        _db.Employees.FirstOrDefaultAsync(e => e.username == username);

    public Task<bool> UsernameExistsAsync(string username) =>
        _db.Employees.AnyAsync(e => e.username == username);

    public async Task<Employee> CreateAsync(Employee employee)
    {
        employee.id = Guid.NewGuid();
        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> ToggleActiveAsync(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;
        employee.isActive = !employee.isActive;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateEmployeeRequest updated)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;
        employee.name = updated.name;
        employee.role = updated.role;
        employee.workload = updated.workload;
        employee.vacationDays = updated.vacationDays;
        employee.permissionLevel = updated.permissionLevel;
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null) return false;
        _db.Employees.Remove(employee);
        await _db.SaveChangesAsync();
        return true;
    }
}
