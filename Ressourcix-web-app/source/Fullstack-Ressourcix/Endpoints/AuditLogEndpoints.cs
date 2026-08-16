namespace FullstackRessourcix;

public static class AuditLogEndpoints
{
  public static void MapAuditLogEndpoints(this IEndpointRouteBuilder app)
  {
    // Kein POST/PUT/DELETE für Audit-Log-Einträge über die API: Einträge entstehen ausschliesslich
    // serverseitig als Nebeneffekt der jeweiligen Mutation in EmployeeStore/RequestsStore, damit die
    // geforderte Revisionssicherheit (BR-01.07) nicht durch frei wählbare Client-Werte untergraben wird.
    app.MapGet(
        "/api/auditlog",
        async (AuditLogStore store) =>
          Results.Ok((await store.AllAsync()).Select(AuditLogResponse.From))
      )
      .RequireAuthorization("ActiveSession");
  }
}
