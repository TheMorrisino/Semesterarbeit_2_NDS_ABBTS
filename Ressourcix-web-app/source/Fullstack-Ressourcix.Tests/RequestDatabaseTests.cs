using FullstackRessourcix;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fullstack_Ressourcix.Tests;

public class RequestDatabaseTests
{
    [Fact]
    public async Task Ferienantrag_WirdGespeichert_Ueberprueft_UndGeloescht()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(
                "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw"
            )
            .Options;

        var requestId = Guid.NewGuid();

        var employeeId = Guid.Parse(
            "4c3469de-428f-437e-b752-46f56714f063"
        );

        var request = new AbsenceRequest(
            Id: requestId,
            EmployeeId: employeeId,
            From: new DateOnly(2026, 9, 7),
            Until: new DateOnly(2026, 9, 11),
            Days: 5,
            Overlap: false,
            Status: RequestStatus.Open,
            SubmittedOn: DateTime.UtcNow,
            Type: AbsenceType.Vacation,
            Remark: "Test Ferienantrag"
        );

        await using var db = new AppDbContext(options);

        try
        {
            // ==========================================
            // 1. Ferienantrag speichern
            // ==========================================

            db.Requests.Add(request);

            await db.SaveChangesAsync();

            // ==========================================
            // 2. Antrag aus Datenbank laden
            // ==========================================

            var savedRequest = await db.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            // ==========================================
            // 3. Prüfen, ob Antrag gespeichert wurde
            // ==========================================

            Assert.NotNull(savedRequest);

            // ==========================================
            // 4. Gespeicherte Werte überprüfen
            // ==========================================

            Assert.Equal(
                requestId,
                savedRequest!.Id
            );

            Assert.Equal(
                employeeId,
                savedRequest.EmployeeId
            );

            Assert.Equal(
                new DateOnly(2026, 9, 7),
                savedRequest.From
            );

            Assert.Equal(
                new DateOnly(2026, 9, 11),
                savedRequest.Until
            );

            Assert.Equal(
                5,
                savedRequest.Days
            );

            Assert.False(
                savedRequest.Overlap
            );

            Assert.Equal(
                RequestStatus.Open,
                savedRequest.Status
            );

            Assert.Equal(
                AbsenceType.Vacation,
                savedRequest.Type
            );

            Assert.Equal(
                "Test Ferienantrag",
                savedRequest.Remark
            );

            // ==========================================
            // 5. Ferienantrag löschen
            // ==========================================

            var requestToDelete = await db.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            Assert.NotNull(requestToDelete);

            db.Requests.Remove(requestToDelete!);

            await db.SaveChangesAsync();

            // ==========================================
            // 6. Prüfen, ob Antrag gelöscht wurde
            // ==========================================

            var deletedRequest = await db.Requests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            Assert.Null(deletedRequest);
        }
        finally
        {
            // ==========================================
            // Cleanup
            // Wird auch bei einem Fehler ausgeführt
            // ==========================================

            var cleanupRequest = await db.Requests
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (cleanupRequest != null)
            {
                db.Requests.Remove(cleanupRequest);

                await db.SaveChangesAsync();
            }
        }
    }
}