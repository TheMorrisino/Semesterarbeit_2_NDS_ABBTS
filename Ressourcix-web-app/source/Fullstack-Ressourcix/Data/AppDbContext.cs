namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Request> Requests => Set<Request>();
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
            entity.HasKey(e => e.id);
            entity.HasIndex(e => e.username).IsUnique();

            entity.HasData(
                new Employee
                {
                    id = Guid.Parse("4c3469de-428f-437e-b752-46f56714f063"),
                    name = "Morris Meier",
                    role = "Mitarbeitende",
                    workload = 100,
                    vacationDays = 5,
                    isActive = true,
                    username = "morris.meier",
                    passwordHash = SeedPasswordHash,
                    mustChangePassword = true,
                    permissionLevel = 1,
                },
                new Employee
                {
                    id = Guid.Parse("86df2463-1bcd-42de-bb97-2cf112caeabf"),
                    name = "Pedro Santos",
                    role = "Planner/Leitung",
                    workload = 100,
                    vacationDays = 5,
                    isActive = true,
                    username = "pedro.santos",
                    passwordHash = SeedPasswordHash,
                    mustChangePassword = true,
                    permissionLevel = 5,
                },
                new Employee
                {
                    id = Guid.Parse("77f37330-cb2b-4a5b-9f6a-6c2d19fde288"),
                    name = "Lena Brunner",
                    role = "Mitarbeitende",
                    workload = 80,
                    vacationDays = 4.4,
                    isActive = true,
                    username = "lena.brunner",
                    passwordHash = SeedPasswordHash,
                    mustChangePassword = true,
                    permissionLevel = 1,
                },
                new Employee
                {
                    id = Guid.Parse("144eda86-a7a2-419d-a37d-e16726e3828c"),
                    name = "Tiago de Sousa Sá",
                    role = "Mitarbeitende",
                    workload = 60,
                    vacationDays = 3.3,
                    isActive = false,
                    username = "tiago.desousa",
                    passwordHash = SeedPasswordHash,
                    mustChangePassword = true,
                    permissionLevel = 1,
                }
            );
        });

        modelBuilder.Entity<Request>(entity =>
        {
            entity.ToTable("requests");
            entity.HasKey(r => r.id);
            entity.HasOne<Employee>().WithMany().HasForeignKey(r => r.employeeId);

            entity.HasData(new Request(
                id: Guid.Parse("7e978e11-0a00-4e05-b61a-007763f529cd"),
                employeeId: Guid.Parse("4c3469de-428f-437e-b752-46f56714f063"),
                from: new DateOnly(2026, 7, 13),
                until: new DateOnly(2026, 7, 24),
                days: 10,
                overlap: true,
                status: RequestStatus.Open,
                submittedOn: new DateTime(2026, 7, 22, 0, 0, 0, DateTimeKind.Utc)
            ));
        });

        modelBuilder.Entity<AuditLogEntry>(entity =>
        {
            entity.ToTable("audit_log_entries");
            entity.HasKey(e => e.Id);
        });
    }
}
