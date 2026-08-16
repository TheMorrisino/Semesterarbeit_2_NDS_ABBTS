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
  public static RequestResponse From(AbsenceRequest r) =>
    new(
      r.Id,
      r.EmployeeId,
      r.From,
      r.Until,
      r.Days,
      r.Overlap,
      r.Status,
      r.SubmittedOn,
      r.Type,
      r.Remark
    );
}
