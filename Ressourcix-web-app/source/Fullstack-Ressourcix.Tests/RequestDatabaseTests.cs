using FullstackRessourcix;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Xunit.Abstractions;

namespace Fullstack_Ressourcix.Tests;

public class RequestDatabaseTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw";

    private static readonly Guid EmployeeId =
        Guid.Parse("4c3469de-428f-437e-b752-46f56714f063");

    [Fact]
    public async Task Ferienantrag_WirdGespeichert()
    {
        var request = CreateTestRequest();

        try
        {
            await using var db = CreateDbContext();

            db.Requests.Add(request);

            await db.SaveChangesAsync();

            var savedRequest = await db.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            Assert.NotNull(savedRequest);

            Console.WriteLine("==========================================");
            Console.WriteLine("FERIENANTRAG AUS DER DATENBANK");
            Console.WriteLine("==========================================");
            Console.WriteLine($"ID:          {savedRequest.Id}");
            Console.WriteLine($"EmployeeId:  {savedRequest.EmployeeId}");
            Console.WriteLine($"From:        {savedRequest.From}");
            Console.WriteLine($"Until:       {savedRequest.Until}");
            Console.WriteLine($"Days:        {savedRequest.Days}");
            Console.WriteLine($"Overlap:     {savedRequest.Overlap}");
            Console.WriteLine($"Status:      {savedRequest.Status}");
            Console.WriteLine($"SubmittedOn: {savedRequest.SubmittedOn}");
            Console.WriteLine($"Type:        {savedRequest.Type}");
            Console.WriteLine($"Remark:      {savedRequest.Remark}");
            Console.WriteLine("==========================================");
        }
        finally
        {
            await DeleteRequest(request.Id);
        }
    }

    [Fact]
    public async Task Ferienantrag_EmployeeId_WirdKorrektGespeichert()
    {
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