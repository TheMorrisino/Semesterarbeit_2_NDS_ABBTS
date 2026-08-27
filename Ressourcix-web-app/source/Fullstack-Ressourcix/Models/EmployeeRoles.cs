namespace FullstackRessourcix;

// Einzige Quelle für die gültigen Rollen-Strings und deren Berechtigungslevel. Ein unbekannter
// Rollenwert (Tippfehler, veralteter Client) führt zu einem expliziten Fehlschlag statt --
// wie zuvor -- stillschweigend zum höchsten Berechtigungslevel.
public static class EmployeeRoles
{
  public const string Mitarbeiter = "Mitarbeiter";
  public const string PlanerLeitung = "Planer/Leitung";

  private static readonly IReadOnlyDictionary<string, int> PermissionLevelByRole = new Dictionary<
    string,
    int
  >
  {
    [Mitarbeiter] = 1,
    [PlanerLeitung] = 5,
  };

  public static bool TryGetPermissionLevel(string role, out int permissionLevel) =>
    PermissionLevelByRole.TryGetValue(role, out permissionLevel);
}
