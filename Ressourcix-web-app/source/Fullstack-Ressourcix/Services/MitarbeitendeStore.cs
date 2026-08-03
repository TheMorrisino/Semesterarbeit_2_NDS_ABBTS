namespace FullstackRessourcix;

public class MitarbeitendeStore
{
    private readonly List<Employee> _daten = new()
    {
        new() { name = "Morris Meier", role = "Mitarbeitende", workload = 100, vacationWeeks = 5 },
        new() { name = "Pedro Santos", role = "Planner/Leitung", workload = 100, vacationWeeks = 5 },
        new() { name = "Lena Brunner", role = "Mitarbeitende", workload = 80, vacationWeeks = 4.4 },
        new() { name = "Rafael Koch", role = "Mitarbeitende", workload = 60, vacationWeeks = 3.3, isActive = false },
    };

    public IReadOnlyList<Employee> Alle() => _daten;

    public Employee Erstelle(Employee neu)
    {
        neu.id = Guid.NewGuid();
        _daten.Add(neu);
        return neu;
    }

    public bool ToggleAktiv(Guid id)
    {
        var m = _daten.FirstOrDefault(x => x.id == id);
        if (m is null) return false;
        m.isActive = !m.isActive;
        return true;
    }

    public bool Aktualisiere(Guid id, Employee neu)
    {
        var m = _daten.FirstOrDefault(x => x.id == id);
        if (m is null) return false;
        m.name = neu.name;
        m.role = neu.role;
        m.workload = neu.workload;
        m.vacationWeeks = neu.vacationWeeks;
        return true;
    }
}