namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
  public AppDbContext(DbContextOptions<AppDbContext> options)
    : base(options) { }

  public DbSet<Employee> Employees => Set<Employee>();
  public DbSet<AbsenceRequest> Requests => Set<AbsenceRequest>();
  public DbSet<AuditLogEntry> AuditLogEntries => Set<AuditLogEntry>();

  // Gehashtes Standardpasswort "Ressourcix#2026" (siehe appsettings.Development.json Auth:DefaultPassword).
  // PasswordHasher<T>.HashPassword nutzt den generischen Typ nicht für den Hash selbst, daher hier
  // unabhängig von der Employee-Instanz vorab erzeugt und als Literal eingebettet (Migrationen müssen
  // deterministisch sein, ein Aufruf von PasswordHasher zur Migrationszeit ist nicht möglich).
  private const string SeedPasswordHash =
    "AQAAAAIAAYagAAAAEB9r8iV+OfZIu2V5H1x/Hh3SM3oPW8VHpRdMSxKV+bTtHl2pQleQPQzWZRZstzlw2w==";

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    modelBuilder.Entity<Employee>(entity =>
    {
      entity.ToTable("employees");
      entity.HasKey(e => e.Id);
      entity.HasIndex(e => e.Username).IsUnique();
      entity.HasData(SeedEmployees());
    });

    modelBuilder.Entity<AbsenceRequest>(entity =>
    {
      entity.ToTable("requests");
      entity.HasKey(r => r.Id);
      entity.HasOne<Employee>().WithMany().HasForeignKey(r => r.EmployeeId);
      entity.HasData(SeedRequests());
    });

    modelBuilder.Entity<AuditLogEntry>(entity =>
    {
      entity.ToTable("audit_log_entries");
      entity.HasKey(e => e.Id);
    });
  }

  private static Employee[] SeedEmployees() =>
    [
      new Employee
      {
        Id = Guid.Parse("4c3469de-428f-437e-b752-46f56714f063"),
        Name = "Morris Meier",
        Role = EmployeeRoles.Mitarbeiter,
        Workload = 100,
        VacationDays = 25,
        IsActive = true,
        Username = "morris.meier",
        PasswordHash = SeedPasswordHash,
        MustChangePassword = true,
        PermissionLevel = 1,
      },
      new Employee
      {
        Id = Guid.Parse("86df2463-1bcd-42de-bb97-2cf112caeabf"),
        Name = "Pedro Santos",
        Role = EmployeeRoles.PlanerLeitung,
        Workload = 100,
        VacationDays = 25,
        IsActive = true,
        Username = "pedro.santos",
        PasswordHash = SeedPasswordHash,
        MustChangePassword = true,
        PermissionLevel = 5,
      },
      new Employee
      {
        Id = Guid.Parse("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"),
        Name = "Lena Brunner",
        Role = EmployeeRoles.Mitarbeiter,
        Workload = 80,
        VacationDays = 22,
        IsActive = true,
        Username = "lena.brunner",
        PasswordHash = SeedPasswordHash,
        MustChangePassword = true,
        PermissionLevel = 1,
      },
      new Employee
      {
        Id = Guid.Parse("144eda86-a7a2-419d-a37d-e16726e3828c"),
        Name = "Tiago de Sousa Sá",
        Role = EmployeeRoles.Mitarbeiter,
        Workload = 60,
        VacationDays = 16.5,
        IsActive = false,
        Username = "tiago.desousa",
        PasswordHash = SeedPasswordHash,
        MustChangePassword = true,
        PermissionLevel = 1,
      },
    ];

  private static AbsenceRequest[] SeedRequests() =>
    [
      new AbsenceRequest(
        Id: Guid.Parse("7e978e11-0a00-4e05-b61a-007763f529cd"),
        EmployeeId: Guid.Parse("4c3469de-428f-437e-b752-46f56714f063"),
        From: new DateOnly(2026, 7, 13),
        Until: new DateOnly(2026, 7, 24),
        Days: 10,
        Overlap: true,
        Status: RequestStatus.Open,
        SubmittedOn: new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc)
      ),
    ];
}
