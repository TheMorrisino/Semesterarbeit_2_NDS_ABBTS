namespace FullstackRessourcix;

public class RequestsStore
{
    private readonly List<Request> _antraege;

    public RequestsStore(MitarbeitendeStore mitarbeitendeStore)
    {
        var ersterMitarbeiter = mitarbeitendeStore.Alle().FirstOrDefault();
        var beispielId = ersterMitarbeiter?.id ?? Guid.NewGuid();

        _antraege = new List<Request>
        {
            new Request(
                Guid.NewGuid(),
                eployeeId: beispielId,
                from: new DateOnly(2026, 7, 13),
                until: new DateOnly(2026, 7, 24),
                days: 10,
                overlap: true,
                status: RequestStatus.Open,
                submittedOn: DateTime.UtcNow.AddDays(-2)
            ),
        };
    }

    public IReadOnlyList<Request> Alle() => _antraege;

    public IReadOnlyList<Request> Offene() =>
        _antraege.Where(a => a.status == RequestStatus.Open).ToList();

    public Request Erstelle(Request neu)
    {
        var mitId = neu.eployeeId == Guid.Empty ? Guid.NewGuid() : neu.eployeeId;
        var erstellt = neu with { id = Guid.NewGuid(), eployeeId = mitId, status = RequestStatus.Open };
        _antraege.Add(erstellt);
        return erstellt;
    }
        public Request Change(Request changed)
    {
        var mitId = neu.eployeeId == Guid.Empty ? Guid.NewGuid() : neu.eployeeId;
        var erstellt = neu with { id = Guid.NewGuid(), eployeeId = mitId, status = RequestStatus.Open };
        _antraege.Add(erstellt);
        return erstellt;
    }

    public bool SetzeStatus(Guid id, RequestStatus status)
    {
        var index = _antraege.FindIndex(a => a.id == id);
        if (index < 0) return false;

        _antraege[index] = _antraege[index] with { status = status };
        return true;
    }
}