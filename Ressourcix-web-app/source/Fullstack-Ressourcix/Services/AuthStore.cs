namespace FullstackRessourcix;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Kapselt Login/Passwortänderung analog zu EmployeeStore/RequestsStore, statt AppDbContext und
// PasswordHasher<Employee> direkt in den Auth-Endpoints zu verdrahten.
public class AuthStore
{
  private readonly AppDbContext _db;
  private readonly PasswordHasher<Employee> _hasher;
  private readonly ILogger<AuthStore> _logger;

  public AuthStore(AppDbContext db, PasswordHasher<Employee> hasher, ILogger<AuthStore> logger)
  {
    _db = db;
    _hasher = hasher;
    _logger = logger;
  }

  public async Task<Employee?> ValidateCredentialsAsync(string username, string password)
  {
    var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Username == username);
    if (employee is null)
    {
      // Bewusst ohne Unterscheidung "Benutzer existiert nicht" vs. "falsches Passwort" in der
      // Log-Meldung (User-Enumeration), aber mit Benutzernamen, damit Brute-Force-Muster im Log
      // erkennbar bleiben (siehe auch das Rate-Limiting auf /api/auth/login).
      _logger.LogWarning("Login fehlgeschlagen (unbekannter Benutzername): {Username}", username);
      return null;
    }

    if (!employee.IsActive)
    {
      _logger.LogWarning("Login verweigert (Konto deaktiviert): {Username}", username);
      return null;
    }

    if (
      _hasher.VerifyHashedPassword(employee, employee.PasswordHash, password)
      == PasswordVerificationResult.Failed
    )
    {
      _logger.LogWarning("Login fehlgeschlagen (falsches Passwort): {Username}", username);
      return null;
    }

    _logger.LogInformation("Login erfolgreich: {Username}", username);
    return employee;
  }

  public Task<Employee?> FindByIdAsync(Guid id) =>
    _db.Employees.FirstOrDefaultAsync(e => e.Id == id);

  // Erneute Prüfung pro Request (siehe Program.cs OnValidatePrincipal): Login prüft IsActive nur
  // einmal beim Einloggen - ohne diese Prüfung bliebe eine bereits ausgestellte Session bis zu 8h
  // gültig, selbst wenn das Konto zwischenzeitlich deaktiviert wurde.
  public async Task<bool> IsActiveAsync(Guid employeeId) =>
    await _db
      .Employees.AsNoTracking()
      .Where(e => e.Id == employeeId)
      .Select(e => (bool?)e.IsActive)
      .FirstOrDefaultAsync() == true;

  // Rückgabe: (NotFound, Error). Error == null und NotFound == false bedeutet Erfolg.
  public async Task<(bool NotFound, string? Error)> ChangePasswordAsync(
    Guid id,
    string currentPassword,
    string newPassword
  )
  {
    var employee = await _db.Employees.FindAsync(id);
    if (employee is null)
    {
      _logger.LogWarning(
        "Passwortänderung fehlgeschlagen (Mitarbeiter nicht gefunden): {EmployeeId}",
        id
      );
      return (true, null);
    }

    if (
      _hasher.VerifyHashedPassword(employee, employee.PasswordHash, currentPassword)
      == PasswordVerificationResult.Failed
    )
    {
      _logger.LogWarning(
        "Passwortänderung fehlgeschlagen (aktuelles Passwort falsch): {EmployeeId}",
        id
      );
      return (false, "Aktuelles Passwort ist falsch.");
    }

    if (!AuthHelpers.IsStrongPassword(newPassword))
    {
      _logger.LogInformation(
        "Passwortänderung abgelehnt (Passwortrichtlinie nicht erfüllt): {EmployeeId}",
        id
      );
      return (
        false,
        "Neues Passwort muss mindestens 8 Zeichen lang sein und Gross-/Kleinbuchstaben, "
          + "eine Zahl und ein Sonderzeichen enthalten."
      );
    }

    if (newPassword == currentPassword)
    {
      _logger.LogInformation(
        "Passwortänderung abgelehnt (neues Passwort identisch mit altem): {EmployeeId}",
        id
      );
      return (false, "Neues Passwort muss sich vom aktuellen unterscheiden.");
    }

    employee.PasswordHash = _hasher.HashPassword(employee, newPassword);
    employee.MustChangePassword = false;
    await _db.SaveChangesAsync();
    _logger.LogInformation("Passwort geändert: {EmployeeId}", id);
    return (false, null);
  }
}
