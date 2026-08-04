namespace FullstackRessourcix;

public class RequestsStore
{
    private readonly List<Request> _requests;

    public RequestsStore(EmployeeStore employeeStore)
    {
        var firstEmployee = employeeStore.All().FirstOrDefault();
        var sampleEmployeeId = firstEmployee?.id ?? Guid.NewGuid();

        _requests = new List<Request>
        {
            new Request(
                Guid.NewGuid(),
                employeeId: sampleEmployeeId,
                from: new DateOnly(2026, 7, 13),
                until: new DateOnly(2026, 7, 24),
                days: 10,
                overlap: true,
                status: RequestStatus.Open,
                submittedOn: DateTime.UtcNow.AddDays(-2)
            ),
        };
    }

    public IReadOnlyList<Request> All() => _requests;

    public IReadOnlyList<Request> GetOpen() =>
        _requests.Where(r => r.status == RequestStatus.Open).ToList();

    public Request Create(Request request)
    {
        var employeeId = request.employeeId == Guid.Empty ? Guid.NewGuid() : request.employeeId;
        // id, status und submittedOn kommen nie vom Client - der könnte sonst z.B. ein falsches
        // Einreichdatum vortäuschen.
        var created = request with
        {
            id = Guid.NewGuid(),
            employeeId = employeeId,
            status = RequestStatus.Open,
            submittedOn = DateTime.UtcNow,
        };
        _requests.Add(created);
        return created;
    }

    public bool Update(Guid id, DateOnly until, RequestStatus status)
    {
        var index = _requests.FindIndex(r => r.id == id);
        if (index < 0) return false;

        _requests[index] = _requests[index] with { until = until, status = status };
        return true;
    }

    public bool SetStatus(Guid id, RequestStatus status)
    {
        var index = _requests.FindIndex(r => r.id == id);
        if (index < 0) return false;

        _requests[index] = _requests[index] with { status = status };
        return true;
    }

    public bool Remove(Guid id)
    {
        var index = _requests.FindIndex(r => r.id == id);
        if (index < 0) return false;

        _requests.RemoveAt(index);
        return true;
    }
}
