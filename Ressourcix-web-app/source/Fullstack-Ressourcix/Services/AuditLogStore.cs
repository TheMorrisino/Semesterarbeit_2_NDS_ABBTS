namespace FullstackRessourcix;

public class AuditLogStore
{
    // bewusst leer: erfundene Reference-Guids würden auf keine echten Employees/Requests zeigen
    private readonly List<AuditLogEntry> _entries = new();

    public IReadOnlyList<AuditLogEntry> All() => _entries;

    public AuditLogEntry Create(AuditLogEntry entry)
    {
        entry.Id = Guid.NewGuid();
        _entries.Add(entry);
        return entry;
    }

    public bool Remove(Guid id)
    {
        var index = _entries.FindIndex(e => e.Id == id);
        if (index < 0) return false;

        _entries.RemoveAt(index);
        return true;
    }
}
