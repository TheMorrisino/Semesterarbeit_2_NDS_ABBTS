namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class RequestsStore
{
  private readonly AppDbContext _db;
  private readonly AuditLogStore _auditLog;
  private readonly ILogger<RequestsStore> _logger;

  public RequestsStore(AppDbContext db, AuditLogStore auditLog, ILogger<RequestsStore> logger)
  {
    _db = db;
    _auditLog = auditLog;
    _logger = logger;
  }

  public Task<List<Request>> AllAsync() => _db.Requests.AsNoTracking().ToListAsync();

  public Task<List<Request>> GetOpenAsync() =>
    _db.Requests.AsNoTracking().Where(r => r.status == RequestStatus.Open).ToListAsync();

  public Task<Request?> GetAsync(Guid id) =>
    _db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.id == id);

  public async Task<Request> CreateAsync(CreateRequestDto dto, string actor)
  {
    // days und overlap kommen nie vom Client - der könnte sonst z.B. eine falsche Ferientageanzahl
    // oder ein falsches Überschneidungsflag vortäuschen; beides wird serverseitig berechnet.
    var days = dto.until.DayNumber - dto.from.DayNumber + 1;
    var overlap = await _db
      .Requests.AsNoTracking()
      .AnyAsync(r => r.employeeId != dto.employeeId && r.from <= dto.until && dto.from <= r.until);

    var created = new Request(
      id: Guid.NewGuid(),
      employeeId: dto.employeeId,
      from: dto.from,
      until: dto.until,
      days: days,
      overlap: overlap,
      status: RequestStatus.Open,
      submittedOn: DateTime.UtcNow,
      type: dto.type,
      remark: dto.remark
    );
    _db.Requests.Add(created);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag erstellt: {RequestId}", created.id);

    var employeeName = await EmployeeNameAsync(created.employeeId);
    await _auditLog.CreateAsync(
      new AuditLogEntry
      {
        Action = AuditLogAction.RequestCreated,
        Summary =
          $"Ferienantrag erfasst für {employeeName}: {AuditSummaryBuilder.FormatDate(created.from)} – {AuditSummaryBuilder.FormatDate(created.until)}",
        Reference = created.id,
        Actor = actor,
      }
    );
    return created;
  }

  public async Task<bool> UpdateAsync(
    Guid id,
    DateOnly until,
    RequestStatus status,
    bool allowStatusChange,
    string actor
  )
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return false;

    var effectiveStatus = allowStatusChange ? status : existing.status;
    var changeText = AuditSummaryBuilder.BuildRequestUpdateSummary(
      existing,
      until,
      effectiveStatus
    );

    var employeeName = await EmployeeNameAsync(existing.employeeId);
    _db.Entry(existing)
      .CurrentValues.SetValues(existing with { until = until, status = effectiveStatus });
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag aktualisiert: {RequestId}", id);

    await _auditLog.CreateAsync(
      new AuditLogEntry
      {
        Action = AuditLogAction.RequestUpdated,
        Summary = $"Ferienantrag geändert für {employeeName}: {changeText}",
        Reference = id,
        Actor = actor,
      }
    );
    return true;
  }

  public async Task<bool> SetStatusAsync(Guid id, RequestStatus status, string actor)
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return false;

    var employeeName = await EmployeeNameAsync(existing.employeeId);
    var changeText = AuditSummaryBuilder.BuildRequestStatusChangeSummary(existing.status, status);

    _db.Entry(existing).CurrentValues.SetValues(existing with { status = status });
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag-Status gesetzt: {RequestId} -> {Status}", id, status);

    await _auditLog.CreateAsync(
      new AuditLogEntry
      {
        Action = AuditLogAction.RequestUpdated,
        Summary = $"Ferienantrag geändert für {employeeName}: {changeText}",
        Reference = id,
        Actor = actor,
      }
    );
    return true;
  }

  public async Task<bool> RemoveAsync(Guid id, string actor)
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return false;

    var employeeName = await EmployeeNameAsync(existing.employeeId);
    var summary =
      $"Ferienantrag gelöscht für {employeeName}: {AuditSummaryBuilder.FormatDate(existing.from)} – {AuditSummaryBuilder.FormatDate(existing.until)}";

    _db.Requests.Remove(existing);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag gelöscht: {RequestId}", id);

    await _auditLog.CreateAsync(
      new AuditLogEntry
      {
        Action = AuditLogAction.RequestDeleted,
        Summary = summary,
        Reference = id,
        Actor = actor,
      }
    );
    return true;
  }

  private async Task<string> EmployeeNameAsync(Guid employeeId) =>
    await _db
      .Employees.AsNoTracking()
      .Where(e => e.id == employeeId)
      .Select(e => e.name)
      .FirstOrDefaultAsync()
    ?? "Unbekannt";
}
