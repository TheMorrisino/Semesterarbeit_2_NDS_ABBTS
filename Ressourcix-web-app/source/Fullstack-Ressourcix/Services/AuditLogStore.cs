namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class AuditLogStore
{
  private readonly AppDbContext _db;

  public AuditLogStore(AppDbContext db)
  {
    _db = db;
  }

  public Task<List<AuditLogEntry>> AllAsync() => _db.AuditLogEntries.AsNoTracking().ToListAsync();

  public async Task<AuditLogEntry> CreateAsync(AuditLogEntry entry)
  {
    entry.Id = Guid.NewGuid();
    _db.AuditLogEntries.Add(entry);
    await _db.SaveChangesAsync();
    return entry;
  }

  // Kurzform für den in EmployeeStore/RequestsStore wiederkehrenden Aufbau eines AuditLogEntry
  // aus den vier Pflichtfeldern, ohne dass jede Aufrufstelle den Objekt-Initializer wiederholt.
  public Task<AuditLogEntry> RecordAsync(
    AuditLogAction action,
    Guid reference,
    string actor,
    string summary
  ) =>
    CreateAsync(
      new AuditLogEntry
      {
        Action = action,
        Summary = summary,
        Reference = reference,
        Actor = actor,
      }
    );
}
