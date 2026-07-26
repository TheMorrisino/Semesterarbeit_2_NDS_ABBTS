namespace FullstackRessourcix;

public class Mitarbeitender
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = "";
    public string Rolle { get; set; } = "";
    public int PensumProzent { get; set; }
    public double Ferienwochen { get; set; }
    public bool IstAktiv { get; set; } = true;
}