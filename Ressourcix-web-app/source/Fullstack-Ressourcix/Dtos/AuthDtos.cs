namespace FullstackRessourcix;

using System.Security.Claims;

public sealed record LoginRequest(string username, string password);

public sealed record ChangePasswordRequest(string currentPassword, string newPassword);

public sealed record AuthUserResponse(
  Guid id,
  string username,
  string name,
  int permissionLevel,
  bool mustChangePassword
)
{
  public static AuthUserResponse From(Employee e) =>
    new(e.id, e.username, e.name, e.permissionLevel, e.mustChangePassword);

  public static AuthUserResponse FromClaims(ClaimsPrincipal user) =>
    new(
      Guid.Parse(user.FindFirst(ClaimTypes.NameIdentifier)!.Value),
      user.FindFirst(ClaimTypes.Name)!.Value,
      user.FindFirst("displayName")!.Value,
      int.Parse(user.FindFirst("permissionLevel")!.Value),
      bool.Parse(user.FindFirst("mustChangePassword")!.Value)
    );
}
