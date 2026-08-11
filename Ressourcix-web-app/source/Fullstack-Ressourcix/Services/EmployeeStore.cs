namespace FullstackRessourcix;

public class EmployeeStore
{
    private readonly List<Employee> _employees = new()
    {
        new() { name = "Morris Meier", role = "Administrator", workload = 100, vacationDays = 5 },
        new() { name = "Pedro Santos", role = "Administrator", workload = 100, vacationDays = 5 },
        new() { name = "Lena Brunner", role = "Mitarbeitende", workload = 80, vacationDays = 4 },
        new() { name = "Tiago de Sousa Sá", role = "Administrator", workload = 60, vacationDays = 3, isActive = false },
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
        employee.vacationDays = updated.vacationDays;
        return true;
    }
}
