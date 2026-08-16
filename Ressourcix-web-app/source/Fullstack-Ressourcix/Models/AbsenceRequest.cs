namespace FullstackRessourcix;

public enum RequestStatus
{
  Open,
  Approved,
  Rejected,
  Taken,
  Cancelled,
}

public enum AbsenceType
{
  Vacation,
  Compensation,
  UnpaidLeave,
}

public sealed record AbsenceRequest(
  Guid Id,
  Guid EmployeeId,
  DateOnly From,
  DateOnly Until,
  int Days,
  bool Overlap,
  RequestStatus Status,
  DateTime SubmittedOn,
  AbsenceType Type = AbsenceType.Vacation,
  string? Remark = null
);
