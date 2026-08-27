namespace FullstackRessourcix;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;

public static class AuthHelpers
{
  public static ClaimsPrincipal BuildPrincipal(Employee employee)
  {
    var claims = new List<Claim>
    {
      new(ClaimTypes.NameIdentifier, employee.Id.ToString()),
      new(ClaimTypes.Name, employee.Username),
      new("displayName", employee.Name),
      new("permissionLevel", employee.PermissionLevel.ToString()),
      new("mustChangePassword", employee.MustChangePassword.ToString()),
    };
    return new ClaimsPrincipal(
      new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)
    );
  }

  public static bool IsAdmin(ClaimsPrincipal user) =>
    int.TryParse(user.FindFirst("permissionLevel")?.Value, out var level) && level >= 5;

  public static Guid CurrentEmployeeId(ClaimsPrincipal user) =>
    Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value);

  public static bool CanActOnRequest(ClaimsPrincipal user, Guid employeeId) =>
    IsAdmin(user) || employeeId == CurrentEmployeeId(user);

  public static string ActorName(ClaimsPrincipal user) => user.FindFirst("displayName")!.Value;

  public static bool IsStrongPassword(string password) =>
    password.Length >= 8
    && password.Any(char.IsUpper)
    && password.Any(char.IsLower)
    && password.Any(char.IsDigit)
    && password.Any(ch => !char.IsLetterOrDigit(ch));
}
