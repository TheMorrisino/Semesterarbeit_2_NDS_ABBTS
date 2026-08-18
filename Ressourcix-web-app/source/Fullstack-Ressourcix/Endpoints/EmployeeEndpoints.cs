namespace FullstackRessourcix;

using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

public static class EmployeeEndpoints
{
  public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet(
        "/api/employees",
        async (EmployeeStore store) =>
          Results.Ok((await store.AllAsync()).Select(EmployeeResponse.From))
      )
      .RequireAuthorization("ActiveSession");

    app.MapPost(
        "/api/employees",
        async (
          CreateEmployeeRequest request,
          ClaimsPrincipal user,
          EmployeeStore store,
          PasswordHasher<Employee> hasher,
          IConfiguration config
        ) =>
        {
          if (await store.UsernameExistsAsync(request.username))
          {
            return Results.Conflict(new { message = "Benutzername bereits vergeben." });
          }

          if (!EmployeeRoles.TryGetPermissionLevel(request.role, out var permissionLevel))
          {
            return Results.BadRequest(new { message = $"Unbekannte Rolle: '{request.role}'." });
          }

          var defaultPassword =
            config["Auth:DefaultPassword"]
            ?? throw new InvalidOperationException("Auth:DefaultPassword ist nicht konfiguriert.");

          var employee = new Employee
          {
            Name = request.name,
            Role = request.role,
            Workload = request.workload,
            VacationDays = request.vacationDays,
            IsActive = true,
            Username = request.username,
            PermissionLevel = permissionLevel,
            MustChangePassword = true,
          };
          employee.PasswordHash = hasher.HashPassword(employee, defaultPassword);

          var created = await store.CreateAsync(employee, AuthHelpers.ActorName(user));
          return Results.Created($"/api/employees/{created.Id}", EmployeeResponse.From(created));
        }
      )
      .RequireAuthorization("Admin");

    app.MapPut(
        "/api/employees/{id}/toggle-active",
        async (Guid id, ClaimsPrincipal user, EmployeeStore store) =>
          await store.ToggleActiveAsync(id, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound()
      )
      .RequireAuthorization("Admin");

    app.MapPut(
        "/api/employees/{id}",
        async (Guid id, UpdateEmployeeRequest request, ClaimsPrincipal user, EmployeeStore store) =>
        {
          if (!EmployeeRoles.TryGetPermissionLevel(request.role, out _))
          {
            return Results.BadRequest(new { message = $"Unbekannte Rolle: '{request.role}'." });
          }

          return await store.UpdateAsync(id, request, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound();
        }
      )
      .RequireAuthorization("Admin");

    app.MapDelete(
        "/api/employees/{id}",
        async (Guid id, ClaimsPrincipal user, EmployeeStore store) =>
          await store.DeleteAsync(id, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound()
      )
      .RequireAuthorization("Admin");
  }
}
