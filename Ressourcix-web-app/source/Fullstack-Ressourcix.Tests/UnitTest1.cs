using FullstackRessourcix;

namespace Fullstack_Ressourcix.Tests;

public class UnitTest1
{
    [Fact]
    public void Mitarbeiter_RolleHatPermissionLevel1()
    {
        // Arrange
        const string role = "Mitarbeiter";

        // Act
        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out var permissionLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(1, permissionLevel);
    }

    [Fact]
    public void PlanerLeitung_RolleHatPermissionLevel5()
    {
        // Arrange
        const string role = "Planer/Leitung";

        // Act
        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out var permissionLevel);

        // Assert
        Assert.True(result);
        Assert.Equal(5, permissionLevel);
    }

    [Fact]
    public void UnbekannteRolle_WirdAbgelehnt()
    {
        // Arrange
        const string role = "Unbekannte Rolle";

        // Act
        var result = EmployeeRoles.TryGetPermissionLevel(
            role,
            out _);

        // Assert
        Assert.False(result);
    }
}