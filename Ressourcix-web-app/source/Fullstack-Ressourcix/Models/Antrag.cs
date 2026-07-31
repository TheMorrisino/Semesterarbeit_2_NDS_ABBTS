namespace FullstackRessourcix;

public enum AntragStatus
{
    Offen,
    Genehmigt,
    Abgelehnt
}

public sealed record Antrag(
    Guid Id,
    Guid MitarbeiterId,
    DateOnly Von,
    DateOnly Bis,
    int Tage,
    bool Ueberschneidung,
    AntragStatus Status,
    DateTime EingereichtAm
);