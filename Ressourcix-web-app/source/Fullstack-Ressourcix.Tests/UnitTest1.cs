using FullstackRessourcix;

namespace Fullstack_Ressourcix.Tests;

public class UnitTest1
{
    [Fact]
    public void Mitarbeiter_RolleHatPermissionLevel1()
    {
        const string role = "Mitarbeiter";

        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out var permissionLevel);

        Assert.True(result);
        Assert.Equal(1, permissionLevel);
    }

    [Fact]
    public void PlanerLeitung_RolleHatPermissionLevel5()
    {
        const string role = "Planer/Leitung";

        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out var permissionLevel);

        Assert.True(result);
        Assert.Equal(5, permissionLevel);
    }

    [Fact]
    public void UnbekannteRolle_WirdAbgelehnt()
    {
        const string role = "Unbekannte Rolle";

        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out _);

        Assert.False(result);
    }
}