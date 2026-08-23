using FullstackRessourcix;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Fullstack_Ressourcix.Tests;

public class RequestDatabaseTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw;Timeout=3;Command Timeout=3";

    private static readonly Guid EmployeeId =
        Guid.Parse("4c3469de-428f-437e-b752-46f56714f063");

    private readonly ITestOutputHelper output;

    private static bool? databaseAvailable;

    public RequestDatabaseTests(ITestOutputHelper output)
    {
        this.output = output;
    }

    [Fact]
    public void Datenbank_VerbindungIstVerfuegbar()
    {
        var connected = CheckDatabaseConnection();

        Assert.True(
            connected,
            "Keine Verbindung zur Ressourcix-Datenbank möglich. Bitte prüfen, ob PostgreSQL läuft."
        );
    }

    [Fact]
    public async Task Ferienantrag_WirdGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            output.WriteLine("==========================================");
            output.WriteLine("FERIENANTRAG AUS DER DATENBANK");
            output.WriteLine("==========================================");
            output.WriteLine($"ID:          {savedRequest.Id}");
            output.WriteLine($"EmployeeId:  {savedRequest.EmployeeId}");
            output.WriteLine($"From:        {savedRequest.From}");
            output.WriteLine($"Until:       {savedRequest.Until}");
            output.WriteLine($"Days:        {savedRequest.Days}");
            output.WriteLine($"Overlap:     {savedRequest.Overlap}");
            output.WriteLine($"Status:      {savedRequest.Status}");
            output.WriteLine($"SubmittedOn: {savedRequest.SubmittedOn}");
            output.WriteLine($"Type:        {savedRequest.Type}");
            output.WriteLine($"Remark:      {savedRequest.Remark}");
            output.WriteLine("==========================================");
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_EmployeeId_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                EmployeeId,
                savedRequest.EmployeeId
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_From_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                new DateOnly(2026, 9, 7),
                savedRequest.From
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Until_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                new DateOnly(2026, 9, 11),
                savedRequest.Until
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Days_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                5,
                savedRequest.Days
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Overlap_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.False(
                savedRequest.Overlap
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Status_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                RequestStatus.Open,
                savedRequest.Status
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Type_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                AbsenceType.Vacation,
                savedRequest.Type
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_Remark_WirdKorrektGespeichert()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var savedRequest = await GetRequest(request.Id);

            Assert.NotNull(savedRequest);

            Assert.Equal(
                "Test Ferienantrag",
                savedRequest.Remark
            );
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_WirdGeloescht()
    {
        EnsureDatabaseAvailable();

        var request = CreateTestRequest();

        try
        {
            await SaveRequest(request);

            var requestToDelete = await GetRequest(request.Id);

            Assert.NotNull(requestToDelete);

            await using var db = CreateDbContext();

            db.Requests.Remove(requestToDelete);

            await db.SaveChangesAsync();

            var deletedRequest = await GetRequest(request.Id);

            Assert.Null(deletedRequest);
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    private static AbsenceRequest CreateTestRequest()
    {
        return new AbsenceRequest(
            Id: Guid.NewGuid(),
            EmployeeId: EmployeeId,
            From: new DateOnly(2026, 9, 7),
            Until: new DateOnly(2026, 9, 11),
            Days: 5,
            Overlap: false,
            Status: RequestStatus.Open,
            SubmittedOn: DateTime.UtcNow,
            Type: AbsenceType.Vacation,
            Remark: "Test Ferienantrag"
        );
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        return new AppDbContext(options);
    }

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
            throw SkipException.ForSkip(
                "Datenbank nicht verfügbar. Der Test wird übersprungen."
            );
        }
    }

    private static async Task SaveRequest(AbsenceRequest request)
    {
        await using var db = CreateDbContext();

        db.Requests.Add(request);

        await db.SaveChangesAsync();
    }

    private static async Task<AbsenceRequest?> GetRequest(Guid requestId)
    {
        await using var db = CreateDbContext();

        return await db.Requests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId);
    }

    private static async Task DeleteRequest(Guid requestId)
    {
        if (!CheckDatabaseConnection())
        {
            return;
        }

        await using var db = CreateDbContext();

        var request = await db.Requests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request != null)
        {
            db.Requests.Remove(request);

            await db.SaveChangesAsync();
        }
    }
}