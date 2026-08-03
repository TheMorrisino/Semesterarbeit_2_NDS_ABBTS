namespace FullstackRessourcix;

public enum RequestStatus
{
    Open,
    Approved,
    Rejected
}

public sealed record Request(
    Guid id,
    Guid eployeeId,
    DateOnly from,
    DateOnly until,
    int days,
    bool overlap,
    RequestStatus status,
    DateTime submittedOn
);