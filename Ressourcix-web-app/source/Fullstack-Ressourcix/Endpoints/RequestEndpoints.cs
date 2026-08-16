namespace FullstackRessourcix;

using System.Security.Claims;

public static class RequestEndpoints
{
  public static void MapRequestEndpoints(this IEndpointRouteBuilder app)
  {
    app.MapGet(
        "/api/requests",
        async (string? status, RequestsStore store) =>
        {
          var requests = status == "open" ? await store.GetOpenAsync() : await store.AllAsync();
          return Results.Ok(requests.Select(RequestResponse.From));
        }
      )
      .RequireAuthorization("ActiveSession");

    app.MapPost(
        "/api/requests",
        async (CreateRequestDto dto, ClaimsPrincipal user, RequestsStore store) =>
        {
          if (dto.employeeId == Guid.Empty)
          {
            return Results.BadRequest(new { message = "employeeId ist erforderlich." });
          }
          if (dto.until < dto.from)
          {
            return Results.BadRequest(new { message = "until darf nicht vor from liegen." });
          }
          if (!AuthHelpers.CanActOnRequest(user, dto.employeeId))
          {
            return Results.Forbid();
          }

          var created = await store.CreateAsync(dto, AuthHelpers.ActorName(user));
          return Results.Created($"/api/requests/{created.id}", RequestResponse.From(created));
        }
      )
      .RequireAuthorization("ActiveSession");

    // Enddatum + Status ändern (z.B. aus einem Bearbeiten-Dialog), unabhängig von approve/reject
    app.MapPut(
        "/api/requests/{id}",
        async (Guid id, RequestUpdate update, ClaimsPrincipal user, RequestsStore store) =>
        {
          var existing = await store.GetAsync(id);
          if (existing is null)
            return Results.NotFound();
          if (!AuthHelpers.CanActOnRequest(user, existing.employeeId))
          {
            return Results.Forbid();
          }

          return await store.UpdateAsync(
            id,
            update.until,
            update.status,
            AuthHelpers.IsAdmin(user),
            AuthHelpers.ActorName(user)
          )
            ? Results.Ok()
            : Results.NotFound();
        }
      )
      .RequireAuthorization("ActiveSession");

    // Keine Frontend-Aufrufer (Approve/Reject läuft über PUT /api/requests/{id}); bewusst beibehalten
    // als Admin-API-Surface für direkte/zukünftige Nutzung.
    app.MapPut(
        "/api/requests/{id}/approve",
        async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
          await store.SetStatusAsync(id, RequestStatus.Approved, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound()
      )
      .RequireAuthorization("Admin"); // Nur Admins dürfen Anträge genehmigen, nicht normale Mitarbeiter

    app.MapPut(
        "/api/requests/{id}/reject",
        async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
          await store.SetStatusAsync(id, RequestStatus.Rejected, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound()
      )
      .RequireAuthorization("Admin"); // Nur Admins dürfen Anträge ablehnen, nicht normale Mitarbeiter

    app.MapDelete(
        "/api/requests/{id}",
        async (Guid id, ClaimsPrincipal user, RequestsStore store) =>
        {
          var existing = await store.GetAsync(id);
          if (existing is null)
            return Results.NotFound();
          if (!AuthHelpers.CanActOnRequest(user, existing.employeeId))
          {
            return Results.Forbid();
          }

          return await store.RemoveAsync(id, AuthHelpers.ActorName(user))
            ? Results.Ok()
            : Results.NotFound();
        }
      )
      .RequireAuthorization("ActiveSession");
  }
}
