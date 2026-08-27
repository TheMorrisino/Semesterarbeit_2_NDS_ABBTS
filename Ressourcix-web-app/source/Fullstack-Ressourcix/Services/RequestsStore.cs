namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public enum RequestUpdateResult
{
  NotFound,
  InvalidDateRange,
  SelfOverlap,
  Success,
}

public class RequestsStore
{
  private readonly AppDbContext _db;
  private readonly AuditLogStore _auditLog;
  private readonly ILogger<RequestsStore> _logger;

  // Status, die eine reale oder wahrscheinliche Abwesenheit bedeuten und daher für die
  // Überschneidungsprüfung zählen. Rejected/Cancelled-Anträge finden nie statt und werden
  // bewusst ausgeschlossen, damit sie nicht fälschlich als Konflikt markiert werden.
  private static readonly RequestStatus[] BlockingStatuses =
    [RequestStatus.Open, RequestStatus.Approved, RequestStatus.Taken];

  public RequestsStore(AppDbContext db, AuditLogStore auditLog, ILogger<RequestsStore> logger)
  {
    _db = db;
    _auditLog = auditLog;
    _logger = logger;
  }

  // Für das Overlap-Flag: überschneidet sich der Zeitraum mit einer aktiven Abwesenheit einer
  // ANDEREN Person?
  private Task<bool> HasCrossEmployeeOverlapAsync(Guid employeeId, DateOnly from, DateOnly until) =>
    _db
      .Requests.AsNoTracking()
      .AnyAsync(r =>
        r.EmployeeId != employeeId
        && BlockingStatuses.Contains(r.Status)
        && r.From <= until
        && from <= r.Until
      );

  // Verhindert, dass dieselbe Person zwei sich überschneidende aktive Anträge hat.
  // excludeRequestId schliesst beim Bearbeiten den Antrag selbst von der Prüfung aus.
  private Task<bool> HasSelfOverlapAsync(
    Guid employeeId,
    DateOnly from,
    DateOnly until,
    Guid? excludeRequestId = null
  ) =>
    _db
      .Requests.AsNoTracking()
      .AnyAsync(r =>
        r.EmployeeId == employeeId
        && (excludeRequestId == null || r.Id != excludeRequestId.Value)
        && BlockingStatuses.Contains(r.Status)
        && r.From <= until
        && from <= r.Until
      );

  public Task<List<AbsenceRequest>> AllAsync() => _db.Requests.AsNoTracking().ToListAsync();

  public Task<List<AbsenceRequest>> GetOpenAsync() =>
    _db.Requests.AsNoTracking().Where(r => r.Status == RequestStatus.Open).ToListAsync();

  public Task<AbsenceRequest?> GetAsync(Guid id) =>
    _db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

  // Gibt null zurück, wenn der neue Antrag einen bestehenden aktiven Antrag DERSELBEN Person
  // überschneidet (Selbst-Überschneidung) - der Aufrufer meldet das als 400 Bad Request.
  public async Task<AbsenceRequest?> CreateAsync(CreateRequestDto dto, string actor)
  {
    if (await HasSelfOverlapAsync(dto.employeeId, dto.from, dto.until))
      return null;

    // days und overlap kommen nie vom Client - der könnte sonst z.B. eine falsche Ferientageanzahl
    // oder ein falsches Überschneidungsflag vortäuschen; beides wird serverseitig berechnet.
    var days = dto.until.DayNumber - dto.from.DayNumber + 1;
    var overlap = await HasCrossEmployeeOverlapAsync(dto.employeeId, dto.from, dto.until);

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

  public async Task<RequestUpdateResult> UpdateAsync(
    Guid id,
    DateOnly until,
    RequestStatus status,
    bool allowStatusChange,
    string actor
  )
  {
    var existing = await _db.Requests.FindAsync(id);
    if (existing is null)
      return RequestUpdateResult.NotFound;

    if (until < existing.From)
      return RequestUpdateResult.InvalidDateRange;

    if (await HasSelfOverlapAsync(existing.EmployeeId, existing.From, until, excludeRequestId: id))
      return RequestUpdateResult.SelfOverlap;

    // Days und Overlap wie bei CreateAsync serverseitig aus dem (ggf. geänderten) Datumsbereich neu
    // berechnen - sonst blieben beide nach einer Enddatum-Änderung auf dem alten, jetzt falschen Stand.
    var days = until.DayNumber - existing.From.DayNumber + 1;
    var overlap = await HasCrossEmployeeOverlapAsync(existing.EmployeeId, existing.From, until);

    // Eine Datumsänderung entwertet eine bestehende Genehmigung/Ablehnung - der Antrag muss
    // erneut geprüft werden, unabhängig davon, wer die Änderung vornimmt oder welcher Status
    // mitgeschickt wurde.
    var effectiveStatus =
      until != existing.Until ? RequestStatus.Open
      : allowStatusChange ? status
      : existing.Status;
    var changeText = AuditSummaryBuilder.BuildRequestUpdateSummary(
      existing,
      until,
      effectiveStatus
    );

    var employeeName = await EmployeeNameAsync(existing.EmployeeId);
    _db.Entry(existing)
      .CurrentValues.SetValues(
        existing with
        {
          Until = until,
          Days = days,
          Overlap = overlap,
          Status = effectiveStatus,
        }
      );
    await _db.SaveChangesAsync();
    _logger.LogInformation("Ferienantrag aktualisiert: {RequestId}", id);

    await _auditLog.RecordAsync(
      AuditLogAction.RequestUpdated,
      id,
      actor,
      $"Ferienantrag geändert für {employeeName}: {changeText}"
    );
    return RequestUpdateResult.Success;
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
