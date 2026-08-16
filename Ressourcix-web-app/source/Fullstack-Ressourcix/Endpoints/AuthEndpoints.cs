namespace FullstackRessourcix;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.RateLimiting;

public static class AuthEndpoints
{
  public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapPost(
        "/api/auth/login",
        async (LoginRequest request, AuthStore store, HttpContext http) =>
        {
          var employee = await store.ValidateCredentialsAsync(request.username, request.password);
          if (employee is null)
            return Results.Unauthorized();

          await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthHelpers.BuildPrincipal(employee)
          );
          return Results.Ok(AuthUserResponse.From(employee));
        }
      )
      .RequireRateLimiting("login");

    app.MapGet(
      "/api/auth/me",
      (ClaimsPrincipal user) =>
        user.Identity?.IsAuthenticated == true
          ? Results.Ok(AuthUserResponse.FromClaims(user))
          : Results.Unauthorized()
    );

    app.MapPost(
        "/api/auth/logout",
        async (HttpContext http) =>
        {
          await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
          return Results.Ok();
        }
      )
      .RequireAuthorization();

    app.MapPost(
        "/api/auth/change-password",
        async (
          ChangePasswordRequest request,
          ClaimsPrincipal user,
          AuthStore store,
          HttpContext http
        ) =>
        {
          var id = AuthHelpers.CurrentEmployeeId(user);
          var (notFound, error) = await store.ChangePasswordAsync(
            id,
            request.currentPassword,
            request.newPassword
          );
          if (notFound)
            return Results.NotFound();
          if (error is not null)
            return Results.BadRequest(new { message = error });

          // Cookie neu ausstellen, damit mustChangePassword=false sofort im Claim steht
          // (sonst würde der Router-Guard im Frontend weiter auf /change-password umleiten).
          var employee = await store.FindByIdAsync(id);
          await http.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            AuthHelpers.BuildPrincipal(employee!)
          );

          return Results.Ok();
        }
      )
      .RequireAuthorization();
  }
}
