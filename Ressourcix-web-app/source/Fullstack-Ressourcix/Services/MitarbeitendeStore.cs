namespace FullstackRessourcix;

public class MitarbeitendeStore
{
    private readonly List<Mitarbeitender> _daten = new()
    {
        new() { Name = "Morris Meier", Rolle = "Mitarbeitende", PensumProzent = 100, Ferienwochen = 5 },
        new() { Name = "Pedro Santos", Rolle = "Planner/Leitung", PensumProzent = 100, Ferienwochen = 5 },
        new() { Name = "Lena Brunner", Rolle = "Mitarbeitende", PensumProzent = 80, Ferienwochen = 4.4 },
        new() { Name = "Rafael Koch", Rolle = "Mitarbeitende", PensumProzent = 60, Ferienwochen = 3.3, IstAktiv = false },
    };

    public IReadOnlyList<Mitarbeitender> Alle() => _daten;

    public Mitarbeitender Erstelle(Mitarbeitender neu)
    {
        neu.Id = Guid.NewGuid();
        _daten.Add(neu);
        return neu;
    }

    public bool ToggleAktiv(Guid id)
    {
        var m = _daten.FirstOrDefault(x => x.Id == id);
        if (m is null) return false;
        m.IstAktiv = !m.IstAktiv;
        return true;
    }

    public bool Aktualisiere(Guid id, Mitarbeitender neu)
    {
        var m = _daten.FirstOrDefault(x => x.Id == id);
        if (m is null) return false;
        m.Name = neu.Name;
        m.Rolle = neu.Rolle;
        m.PensumProzent = neu.PensumProzent;
        m.Ferienwochen = neu.Ferienwochen;
        return true;
    }
}