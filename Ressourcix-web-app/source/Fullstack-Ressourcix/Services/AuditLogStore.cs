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
}
