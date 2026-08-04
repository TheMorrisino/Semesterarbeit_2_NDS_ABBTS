namespace FullstackRessourcix;

public enum RequestStatus
{
    Open,
    Approved,
    Rejected,
    Taken,
    Cancelled
}

public enum AbsenceType
{
    Vacation,
    Compensation,
    UnpaidLeave
}

public sealed record Request(
    Guid id,
    Guid employeeId,
    DateOnly from,
    DateOnly until,
    int days,
    bool overlap,
    RequestStatus status,
    DateTime submittedOn,
    AbsenceType type = AbsenceType.Vacation,
    string? remark = null
);
