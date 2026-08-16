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

  public Task<List<AbsenceRequest>> AllAsync() => _db.Requests.AsNoTracking().ToListAsync();

  public Task<List<AbsenceRequest>> GetOpenAsync() =>
    _db.Requests.AsNoTracking().Where(r => r.Status == RequestStatus.Open).ToListAsync();

  public Task<AbsenceRequest?> GetAsync(Guid id) =>
    _db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

  public async Task<AbsenceRequest> CreateAsync(CreateRequestDto dto, string actor)
  {
    // days und overlap kommen nie vom Client - der könnte sonst z.B. eine falsche Ferientageanzahl
    // oder ein falsches Überschneidungsflag vortäuschen; beides wird serverseitig berechnet.
    var days = dto.until.DayNumber - dto.from.DayNumber + 1;
    var overlap = await _db
      .Requests.AsNoTracking()
      .AnyAsync(r => r.EmployeeId != dto.employeeId && r.From <= dto.until && dto.from <= r.Until);

    var created = new AbsenceRequest(
      Id: Guid.NewGuid(),
      EmployeeId: dto.employeeId,
      From: dto.from,
      Until: dto.until,
      Days: days,
      Overlap: overlap,
      Status: RequestStatus.Open,
      SubmittedOn: DateTime.UtcNow,
      Type: dto.type,
      Remark: dto.remark
    );
    _db.Requests.Add(created);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag erstellt: {RequestId}", created.Id);

    var employeeName = await EmployeeNameAsync(created.EmployeeId);
    await _auditLog.RecordAsync(
      AuditLogAction.RequestCreated,
      created.Id,
      actor,
      $"Ferienantrag erfasst für {employeeName}: {AuditSummaryBuilder.FormatDate(created.From)} – {AuditSummaryBuilder.FormatDate(created.Until)}"
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

    var effectiveStatus = allowStatusChange ? status : existing.Status;
    var changeText = AuditSummaryBuilder.BuildRequestUpdateSummary(
      existing,
      until,
      effectiveStatus
    );

    var employeeName = await EmployeeNameAsync(existing.EmployeeId);
    _db.Entry(existing)
      .CurrentValues.SetValues(existing with { Until = until, Status = effectiveStatus });
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag aktualisiert: {RequestId}", id);

    await _auditLog.RecordAsync(
      AuditLogAction.RequestUpdated,
      id,
      actor,
      $"Ferienantrag geändert für {employeeName}: {changeText}"
    );
    return true;
  }

  public async Task<bool> SetStatusAsync(Guid id, RequestStatus status, string actor)
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return false;

    var employeeName = await EmployeeNameAsync(existing.EmployeeId);
    var changeText = AuditSummaryBuilder.BuildRequestStatusChangeSummary(existing.Status, status);

    _db.Entry(existing).CurrentValues.SetValues(existing with { Status = status });
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag-Status gesetzt: {RequestId} -> {Status}", id, status);

    await _auditLog.RecordAsync(
      AuditLogAction.RequestUpdated,
      id,
      actor,
      $"Ferienantrag geändert für {employeeName}: {changeText}"
    );
    return true;
  }

  public async Task<bool> RemoveAsync(Guid id, string actor)
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return false;

    var employeeName = await EmployeeNameAsync(existing.EmployeeId);
    var summary =
      $"Ferienantrag gelöscht für {employeeName}: {AuditSummaryBuilder.FormatDate(existing.From)} – {AuditSummaryBuilder.FormatDate(existing.Until)}";

    _db.Requests.Remove(existing);
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag gelöscht: {RequestId}", id);

    await _auditLog.RecordAsync(AuditLogAction.RequestDeleted, id, actor, summary);
    return true;
  }

  private async Task<string> EmployeeNameAsync(Guid employeeId) =>
    await _db
      .Employees.AsNoTracking()
      .Where(e => e.Id == employeeId)
      .Select(e => e.Name)
      .FirstOrDefaultAsync()
    ?? "Unbekannt";
}
