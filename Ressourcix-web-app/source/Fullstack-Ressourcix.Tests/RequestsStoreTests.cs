using FullstackRessourcix;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Sdk;

namespace Fullstack_Ressourcix.Tests;

public class RequestsStoreTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw;Timeout=3;Command Timeout=3";

    // Seed-Mitarbeitende aus AppDbContext.SeedEmployees(), damit der FK auf employees erfüllt ist.
    private static readonly Guid LenaId = Guid.Parse("77f37330-cb2b-4a5b-9f6a-6c2d19fde288");
    private static readonly Guid PedroId = Guid.Parse("86df2463-1bcd-42de-bb97-2cf112caeabf");

    private static bool? databaseAvailable;

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

    private static RequestsStore CreateStore(AppDbContext db) =>
        new(db, new AuditLogStore(db), NullLogger<RequestsStore>.Instance);

    private static bool CheckDatabaseConnection()
    {
        if (databaseAvailable.HasValue)
        {
            return databaseAvailable.Value;
        }

        try
        {
            using var db = CreateDbContext();
            databaseAvailable = db.Database.CanConnect();
            if (databaseAvailable.Value)
            {
                db.Database.Migrate();
            }

            return databaseAvailable.Value;
        }
        catch
        {
            databaseAvailable = false;
            return false;
        }
    }

    private static void EnsureDatabaseAvailable()
    {
        if (!CheckDatabaseConnection())
        {
            throw SkipException.ForSkip("Datenbank nicht verfügbar. Der Test wird übersprungen.");
        }
    }

    private static async Task<AbsenceRequest> InsertRequest(
        Guid employeeId,
        DateOnly from,
        DateOnly until,
        int days,
        bool overlap,
        RequestStatus status = RequestStatus.Open
    )
    {
        var request = new AbsenceRequest(
            Id: Guid.NewGuid(),
            EmployeeId: employeeId,
            From: from,
            Until: until,
            Days: days,
            Overlap: overlap,
            Status: status,
            SubmittedOn: DateTime.UtcNow,
            Type: AbsenceType.Vacation,
            Remark: "Testantrag (RequestsStoreTests)"
        );

        await using var db = CreateDbContext();
        db.Requests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    private static async Task<AbsenceRequest?> GetRequest(Guid id)
    {
        await using var db = CreateDbContext();
        return await db.Requests.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);
    }

    private static async Task DeleteRequest(Guid id)
    {
        if (!CheckDatabaseConnection())
        {
            return;
        }

        await using var db = CreateDbContext();
        var request = await db.Requests.FirstOrDefaultAsync(r => r.Id == id);
        if (request != null)
        {
            db.Requests.Remove(request);
            await db.SaveChangesAsync();
        }
    }

    [Fact]
    public async Task UpdateAsync_UnbekannteId_GibtNotFoundZurueck()
    {
        EnsureDatabaseAvailable();

        await using var db = CreateDbContext();
        var store = CreateStore(db);

        var result = await store.UpdateAsync(
            Guid.NewGuid(),
            new DateOnly(2026, 10, 10),
            RequestStatus.Open,
            allowStatusChange: true,
            actor: "test"
        );

        Assert.Equal(RequestUpdateResult.NotFound, result);
    }

    [Fact]
    public async Task UpdateAsync_UntilVorFrom_WirdAbgelehntUndDatensatzBleibtUnveraendert()
    {
        EnsureDatabaseAvailable();

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 12, 1),
            until: new DateOnly(2026, 12, 5),
            days: 5,
            overlap: false
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 11, 1), // vor From
                RequestStatus.Open,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.InvalidDateRange, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.Equal(new DateOnly(2026, 12, 5), stored.Until);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_AenderungDesEnddatums_BerechnetDaysNeu()
    {
        EnsureDatabaseAvailable();

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 10, 5),
            until: new DateOnly(2026, 10, 9),
            days: 5,
            overlap: false
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 10, 30), // 26 Tage ab 5.10. statt ursprünglich 5
                RequestStatus.Open,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.Equal(26, stored.Days);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_AenderungDesEnddatumsInUeberschneidung_SetztOverlapAufTrue()
    {
        EnsureDatabaseAvailable();

        var otherEmployeesRequest = await InsertRequest(
            PedroId,
            from: new DateOnly(2026, 11, 10),
            until: new DateOnly(2026, 11, 15),
            days: 6,
            overlap: false,
            status: RequestStatus.Approved
        );

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 11, 1),
            until: new DateOnly(2026, 11, 5),
            days: 5,
            overlap: false
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 11, 12), // überschneidet jetzt Pedros 10.-15.11.
                RequestStatus.Open,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.True(stored.Overlap);
        }
        finally
        {
            await DeleteRequest(request.Id);
            await DeleteRequest(otherEmployeesRequest.Id);
        }
    }

    [Fact]
    public async Task CreateAsync_UeberschneidungMitAbgelehntemAntragAndererPerson_SetztOverlapNichtAufTrue()
    {
        EnsureDatabaseAvailable();

        var pedrosRejectedRequest = await InsertRequest(
            PedroId,
            from: new DateOnly(2026, 9, 10),
            until: new DateOnly(2026, 9, 15),
            days: 6,
            overlap: false,
            status: RequestStatus.Rejected
        );

        AbsenceRequest? created = null;
        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            created = await store.CreateAsync(
                new CreateRequestDto(LenaId, new DateOnly(2026, 9, 12), new DateOnly(2026, 9, 13), AbsenceType.Vacation, null),
                actor: "test"
            );

            Assert.NotNull(created);
            Assert.False(created.Overlap);
        }
        finally
        {
            await DeleteRequest(pedrosRejectedRequest.Id);
            if (created != null)
            {
                await DeleteRequest(created.Id);
            }
        }
    }

    [Fact]
    public async Task CreateAsync_UeberschneidungMitEigenemAktivemAntrag_WirdAbgelehnt()
    {
        EnsureDatabaseAvailable();

        var lenasExistingRequest = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 9, 20),
            until: new DateOnly(2026, 9, 25),
            days: 6,
            overlap: false
        );

        AbsenceRequest? created = null;
        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            created = await store.CreateAsync(
                new CreateRequestDto(LenaId, new DateOnly(2026, 9, 22), new DateOnly(2026, 9, 23), AbsenceType.Vacation, null),
                actor: "test"
            );

            Assert.Null(created);
        }
        finally
        {
            await DeleteRequest(lenasExistingRequest.Id);
            if (created != null)
            {
                await DeleteRequest(created.Id);
            }
        }
    }

    [Fact]
    public async Task UpdateAsync_UeberschneidungMitAbgelehntemAntragAndererPerson_SetztOverlapNichtAufTrue()
    {
        EnsureDatabaseAvailable();

        var pedrosRejectedRequest = await InsertRequest(
            PedroId,
            from: new DateOnly(2026, 9, 10),
            until: new DateOnly(2026, 9, 15),
            days: 6,
            overlap: false,
            status: RequestStatus.Rejected
        );

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 9, 1),
            until: new DateOnly(2026, 9, 5),
            days: 5,
            overlap: false
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 9, 12), // überschneidet geografisch Pedros abgelehnten Antrag
                RequestStatus.Open,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.False(stored.Overlap);
        }
        finally
        {
            await DeleteRequest(request.Id);
            await DeleteRequest(pedrosRejectedRequest.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_AenderungInUeberschneidungMitEigenemAktivemAntrag_WirdAlsSelfOverlapAbgelehnt()
    {
        EnsureDatabaseAvailable();

        var requestA = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 10, 15),
            until: new DateOnly(2026, 10, 20),
            days: 6,
            overlap: false
        );

        var requestB = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 10, 1),
            until: new DateOnly(2026, 10, 5),
            days: 5,
            overlap: false
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            var result = await store.UpdateAsync(
                requestB.Id,
                new DateOnly(2026, 10, 16), // überschneidet jetzt requestA derselben Person
                RequestStatus.Open,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.SelfOverlap, result);

            var stored = await GetRequest(requestB.Id);
            Assert.NotNull(stored);
            Assert.Equal(new DateOnly(2026, 10, 5), stored.Until); // unverändert, Update wurde nicht persistiert
        }
        finally
        {
            await DeleteRequest(requestA.Id);
            await DeleteRequest(requestB.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_AenderungDesEnddatums_SetztStatusAufOpenZurueckAuchWennApprovedAngefordertWird()
    {
        EnsureDatabaseAvailable();

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 7, 1),
            until: new DateOnly(2026, 7, 5),
            days: 5,
            overlap: false,
            status: RequestStatus.Approved
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            // Admin ändert das Enddatum und schickt (z.B. weil das Formular den bisherigen Status
            // vorausfüllt) weiterhin "Approved" mit - die Verlängerung muss trotzdem neu freigegeben werden.
            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 7, 10),
                RequestStatus.Approved,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.Equal(RequestStatus.Open, stored.Status);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_AenderungDesEnddatumsDurchNichtAdmin_SetztStatusEbenfallsAufOpenZurueck()
    {
        EnsureDatabaseAvailable();

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 7, 15),
            until: new DateOnly(2026, 7, 20),
            days: 6,
            overlap: false,
            status: RequestStatus.Approved
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            // Mitarbeiter (nicht Admin) ändert nur das Enddatum des eigenen, bereits genehmigten
            // Antrags - allowStatusChange ist false, trotzdem darf der Status nicht "Approved" bleiben.
            var result = await store.UpdateAsync(
                request.Id,
                new DateOnly(2026, 7, 25),
                RequestStatus.Approved,
                allowStatusChange: false,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.Equal(RequestStatus.Open, stored.Status);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task UpdateAsync_OhneAenderungDesEnddatums_BehaeltDenAngeforderetenStatus()
    {
        EnsureDatabaseAvailable();

        var request = await InsertRequest(
            LenaId,
            from: new DateOnly(2026, 7, 27),
            until: new DateOnly(2026, 7, 29),
            days: 3,
            overlap: false,
            status: RequestStatus.Open
        );

        try
        {
            await using var db = CreateDbContext();
            var store = CreateStore(db);

            // Admin genehmigt einen Antrag (Approve läuft über PUT), Enddatum bleibt unverändert.
            var result = await store.UpdateAsync(
                request.Id,
                request.Until,
                RequestStatus.Approved,
                allowStatusChange: true,
                actor: "test"
            );

            Assert.Equal(RequestUpdateResult.Success, result);

            var stored = await GetRequest(request.Id);
            Assert.NotNull(stored);
            Assert.Equal(RequestStatus.Approved, stored.Status);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }
}
