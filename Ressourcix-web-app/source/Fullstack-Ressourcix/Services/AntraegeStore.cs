namespace FullstackRessourcix;

public class AntraegeStore
{
    private readonly List<Antrag> _antraege;

    public AntraegeStore(MitarbeitendeStore mitarbeitendeStore)
    {
        var ersterMitarbeiter = mitarbeitendeStore.Alle().FirstOrDefault();
        var beispielId = ersterMitarbeiter?.Id ?? Guid.NewGuid();

        _antraege = new List<Antrag>
        {
            new Antrag(
                Guid.NewGuid(),
                MitarbeiterId: beispielId,
                Von: new DateOnly(2026, 7, 13),
                Bis: new DateOnly(2026, 7, 24),
                Tage: 10,
                Ueberschneidung: true,
                Status: AntragStatus.Offen,
                EingereichtAm: DateTime.UtcNow.AddDays(-2)
            ),
        };
    }

    public IReadOnlyList<Antrag> Alle() => _antraege;

    public IReadOnlyList<Antrag> Offene() =>
        _antraege.Where(a => a.Status == AntragStatus.Offen).ToList();

    public Antrag Erstelle(Antrag neu)
    {
        var mitId = neu.MitarbeiterId == Guid.Empty ? Guid.NewGuid() : neu.MitarbeiterId;
        var erstellt = neu with { Id = Guid.NewGuid(), MitarbeiterId = mitId, Status = AntragStatus.Offen };
        _antraege.Add(erstellt);
        return erstellt;
    }

    public bool SetzeStatus(Guid id, AntragStatus status)
    {
        var index = _antraege.FindIndex(a => a.Id == id);
        if (index < 0) return false;

        _antraege[index] = _antraege[index] with { Status = status };
        return true;
    }
}