namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class RequestsStore
{
    private readonly AppDbContext _db;
    private readonly AuditLogStore _auditLog;

    private static readonly Dictionary<RequestStatus, string> StatusLabels = new()
    {
        [RequestStatus.Open] = "Ausstehend",
        [RequestStatus.Approved] = "Genehmigt",
        [RequestStatus.Rejected] = "Abgelehnt",
        [RequestStatus.Taken] = "Bezogen",
        [RequestStatus.Cancelled] = "Storniert",
    };

    public RequestsStore(AppDbContext db, AuditLogStore auditLog)
    {
        _db = db;
        _auditLog = auditLog;
    }

    public Task<List<Request>> AllAsync() => _db.Requests.AsNoTracking().ToListAsync();

    public Task<List<Request>> GetOpenAsync() =>
        _db.Requests.AsNoTracking().Where(r => r.status == RequestStatus.Open).ToListAsync();

    public Task<Request?> GetAsync(Guid id) =>
        _db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.id == id);

    public async Task<Request> CreateAsync(Request request, string actor)
    {
        // id, status und submittedOn kommen nie vom Client - der könnte sonst z.B. ein falsches
        // Einreichdatum vortäuschen.
        var created = request with
        {
            id = Guid.NewGuid(),
            status = RequestStatus.Open,
            submittedOn = DateTime.UtcNow,
        };
        _db.Requests.Add(created);
        await _db.SaveChangesAsync();

        var employeeName = await EmployeeNameAsync(created.employeeId);
        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.RequestCreated,
            Summary = $"Ferienantrag erfasst für {employeeName}: {Format(created.from)} – {Format(created.until)}",
            Reference = created.id,
            Actor = actor,
        });
        return created;
    }

    public async Task<bool> UpdateAsync(Guid id, DateOnly until, RequestStatus status, bool allowStatusChange, string actor)
    {
        var existing = await _db.Requests.FindAsync(id);
        if (existing is null) return false;

        var effectiveStatus = allowStatusChange ? status : existing.status;
        var changes = new List<string>();
        if (existing.until != until) changes.Add($"Ende: {Format(existing.until)} → {Format(until)}");
        if (existing.status != effectiveStatus) changes.Add($"Status: {StatusLabels[existing.status]} → {StatusLabels[effectiveStatus]}");
        var changeText = changes.Count > 0 ? string.Join(", ", changes) : "keine Änderung";

        var employeeName = await EmployeeNameAsync(existing.employeeId);
        _db.Entry(existing).CurrentValues.SetValues(existing with { until = until, status = effectiveStatus });
        await _db.SaveChangesAsync();

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.RequestUpdated,
            Summary = $"Ferienantrag geändert für {employeeName}: {changeText}",
            Reference = id,
            Actor = actor,
        });
        return true;
    }

    public async Task<bool> SetStatusAsync(Guid id, RequestStatus status)
    {
        var existing = await _db.Requests.FindAsync(id);
        if (existing is null) return false;

        _db.Entry(existing).CurrentValues.SetValues(existing with { status = status });
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveAsync(Guid id, string actor)
    {
        var existing = await _db.Requests.FindAsync(id);
        if (existing is null) return false;

        var employeeName = await EmployeeNameAsync(existing.employeeId);
        var summary = $"Ferienantrag gelöscht für {employeeName}: {Format(existing.from)} – {Format(existing.until)}";

        _db.Requests.Remove(existing);
        await _db.SaveChangesAsync();

        await _auditLog.CreateAsync(new AuditLogEntry
        {
            Action = AuditLogAction.RequestDeleted,
            Summary = summary,
            Reference = id,
            Actor = actor,
        });
        return true;
    }

    private async Task<string> EmployeeNameAsync(Guid employeeId) =>
        await _db.Employees.AsNoTracking().Where(e => e.id == employeeId).Select(e => e.name).FirstOrDefaultAsync()
        ?? "Unbekannt";

    private static string Format(DateOnly date) => date.ToString("dd.MM.yyyy");
}
