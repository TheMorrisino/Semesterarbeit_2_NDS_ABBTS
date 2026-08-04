namespace FullstackRessourcix;

public class EmployeeStore
{
    private readonly List<Employee> _employees = new()
    {
        new() { name = "Morris Meier", role = "Mitarbeitende", workload = 100, vacationWeeks = 5 },
        new() { name = "Pedro Santos", role = "Planner/Leitung", workload = 100, vacationWeeks = 5 },
        new() { name = "Lena Brunner", role = "Mitarbeitende", workload = 80, vacationWeeks = 4.4 },
        new() { name = "Rafael Koch", role = "Mitarbeitende", workload = 60, vacationWeeks = 3.3, isActive = false },
    };

    public IReadOnlyList<Employee> All() => _employees;

    public Employee Create(Employee employee)
    {
        employee.id = Guid.NewGuid();
        _employees.Add(employee);
        return employee;
    }

    public bool ToggleActive(Guid id)
    {
        var employee = _employees.FirstOrDefault(x => x.id == id);
        if (employee is null) return false;
        employee.isActive = !employee.isActive;
        return true;
    }

    public bool Update(Guid id, Employee updated)
    {
        var employee = _employees.FirstOrDefault(x => x.id == id);
        if (employee is null) return false;
        employee.name = updated.name;
        employee.role = updated.role;
        employee.workload = updated.workload;
        employee.vacationWeeks = updated.vacationWeeks;
        employee.department = updated.department;
        employee.education = updated.education;
        return true;
    }
}
