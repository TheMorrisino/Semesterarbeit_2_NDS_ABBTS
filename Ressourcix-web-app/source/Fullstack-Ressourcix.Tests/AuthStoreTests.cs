using FullstackRessourcix;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using Xunit.Sdk;

namespace Fullstack_Ressourcix.Tests;

public class AuthStoreTests
{
    private const string ConnectionString =
        "Host=localhost;Port=5432;Database=ressourcix;Username=ressourcix;Password=ressourcix_dev_pw;Timeout=3;Command Timeout=3";

    // Aktiver Seed-Mitarbeiter (Lena Brunner, IsActive = true).
    private static readonly Guid ActiveEmployeeId = Guid.Parse("77f37330-cb2b-4a5b-9f6a-6c2d19fde288");

    // Deaktivierter Seed-Mitarbeiter (Tiago de Sousa Sá, IsActive = false).
    private static readonly Guid InactiveEmployeeId = Guid.Parse("144eda86-a7a2-419d-a37d-e16726e3828c");

    private static bool? databaseAvailable;

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

    private static AuthStore CreateStore(AppDbContext db) =>
        new(db, new Microsoft.AspNetCore.Identity.PasswordHasher<Employee>(), NullLogger<AuthStore>.Instance);

    [Fact]
    public async Task IsActiveAsync_AktiverMitarbeiter_GibtTrueZurueck()
    {
        EnsureDatabaseAvailable();

        await using var db = CreateDbContext();
        var store = CreateStore(db);

        var result = await store.IsActiveAsync(ActiveEmployeeId);

        Assert.True(result);
    }

    [Fact]
    public async Task IsActiveAsync_DeaktivierterMitarbeiter_GibtFalseZurueck()
    {
        EnsureDatabaseAvailable();

        await using var db = CreateDbContext();
        var store = CreateStore(db);

        var result = await store.IsActiveAsync(InactiveEmployeeId);

        Assert.False(result);
    }

    [Fact]
    public async Task IsActiveAsync_UnbekannteId_GibtFalseZurueck()
    {
        EnsureDatabaseAvailable();

        await using var db = CreateDbContext();
        var store = CreateStore(db);

        var result = await store.IsActiveAsync(Guid.NewGuid());

        Assert.False(result);
    }
}
