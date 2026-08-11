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
}