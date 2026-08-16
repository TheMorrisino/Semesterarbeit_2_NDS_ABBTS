namespace FullstackRessourcix;

using Microsoft.EntityFrameworkCore;

public class RequestsStore
{
    private readonly AppDbContext _db;

    public RequestsStore(AppDbContext db)
    {
        _db = db;
    }

    public Task<List<Request>> AllAsync() => _db.Requests.AsNoTracking().ToListAsync();

    public Task<List<Request>> GetOpenAsync() =>
        _db.Requests.AsNoTracking().Where(r => r.status == RequestStatus.Open).ToListAsync();

    public Task<Request?> GetAsync(Guid id) =>
        _db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.id == id);

    public async Task<Request> CreateAsync(Request request)
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
        return created;
    }

    public async Task<bool> UpdateAsync(Guid id, DateOnly until, RequestStatus status, bool allowStatusChange)
    {
        var existing = await _db.Requests.FindAsync(id);
        if (existing is null) return false;

        var effectiveStatus = allowStatusChange ? status : existing.status;
        _db.Entry(existing).CurrentValues.SetValues(existing with { until = until, status = effectiveStatus });
        await _db.SaveChangesAsync();
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

    public async Task<bool> RemoveAsync(Guid id)
    {
        var existing = await _db.Requests.FindAsync(id);
        if (existing is null) return false;

        _db.Requests.Remove(existing);
        await _db.SaveChangesAsync();
        return true;
    }
}
