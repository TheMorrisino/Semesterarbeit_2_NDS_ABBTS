namespace FullstackRessourcix;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// Kapselt Login/Passwortänderung analog zu EmployeeStore/RequestsStore, statt AppDbContext und
// PasswordHasher<Employee> direkt in den Auth-Endpoints zu verdrahten.
public class AuthStore
{
  private readonly AppDbContext _db;
  private readonly PasswordHasher<Employee> _hasher;

  public AuthStore(AppDbContext db, PasswordHasher<Employee> hasher)
  {
    _db = db;
    _hasher = hasher;
  }

  public async Task<Employee?> ValidateCredentialsAsync(string username, string password)
  {
    var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Username == username);
    if (
      employee is null
      || !employee.IsActive
      || _hasher.VerifyHashedPassword(employee, employee.PasswordHash, password)
        == PasswordVerificationResult.Failed
    )
    {
      return null;
    }
    return employee;
  }

  public Task<Employee?> FindByIdAsync(Guid id) =>
    _db.Employees.FirstOrDefaultAsync(e => e.Id == id);

  // Rückgabe: (NotFound, Error). Error == null und NotFound == false bedeutet Erfolg.
  public async Task<(bool NotFound, string? Error)> ChangePasswordAsync(
    Guid id,
    string currentPassword,
    string newPassword
  )
  {
    var employee = await _db.Employees.FindAsync(id);
    if (employee is null)
      return (true, null);

    if (
      _hasher.VerifyHashedPassword(employee, employee.PasswordHash, currentPassword)
      == PasswordVerificationResult.Failed
    )
    {
      return (false, "Aktuelles Passwort ist falsch.");
    }

    if (!AuthHelpers.IsStrongPassword(newPassword))
    {
      return (
        false,
        "Neues Passwort muss mindestens 8 Zeichen lang sein und Gross-/Kleinbuchstaben, "
          + "eine Zahl und ein Sonderzeichen enthalten."
      );
    }

    if (newPassword == currentPassword)
    {
      return (false, "Neues Passwort muss sich vom aktuellen unterscheiden.");
    }

    employee.PasswordHash = _hasher.HashPassword(employee, newPassword);
    employee.MustChangePassword = false;
    await _db.SaveChangesAsync();
    return (false, null);
  }
}
