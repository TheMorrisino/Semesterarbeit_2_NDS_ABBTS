namespace FullstackRessourcix;

public sealed record CreateRequestDto(
  Guid employeeId,
  DateOnly from,
  DateOnly until,
  AbsenceType type,
  string? remark
);

public sealed record RequestUpdate(DateOnly until, RequestStatus status);

public sealed record RequestResponse(
  Guid id,
  Guid employeeId,
  DateOnly from,
  DateOnly until,
  int days,
  bool overlap,
  RequestStatus status,
  DateTime submittedOn,
  AbsenceType type,
  string? remark
)
{
  public static RequestResponse From(Request r) =>
    new(
      r.id,
      r.employeeId,
      r.from,
      r.until,
      r.days,
      r.overlap,
      r.status,
      r.submittedOn,
      r.type,
      r.remark
    );
}
