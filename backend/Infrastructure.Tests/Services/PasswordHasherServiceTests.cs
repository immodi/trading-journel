using FluentAssertions;
using Infrastructure.Services;

namespace Infrastructure.Tests.Services;

public class PasswordHasherServiceTests
{
    [Fact]
    public void HashPassword_Should_Return_Hash()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        const string password = "MySecurePassword123!";

        // Act
        var hash = passwordHasher.HashPassword(password);

        // Assert
        hash.Should().NotBeNullOrWhiteSpace();
        hash.Should().NotBe(password);
    }

    [Fact]
    public void VerifyPassword_Should_Return_True_For_Correct_Password()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        const string password = "MySecurePassword123!";

        var hash = passwordHasher.HashPassword(password);

        // Act
        var result = passwordHasher.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void VerifyPassword_Should_Return_False_For_Incorrect_Password()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        const string password = "MySecurePassword123!";
        const string wrongPassword = "WrongPassword123!";

        var hash = passwordHasher.HashPassword(password);

        // Act
        var result = passwordHasher.VerifyPassword(wrongPassword, hash);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HashPassword_Should_Generate_Different_Hashes_For_Same_Password()
    {
        // Arrange
        var passwordHasher = new PasswordHasher();
        const string password = "MySecurePassword123!";

        // Act
        var hash1 = passwordHasher.HashPassword(password);
        var hash2 = passwordHasher.HashPassword(password);

        // Assert
        hash1.Should().NotBe(hash2);

        passwordHasher.VerifyPassword(password, hash1).Should().BeTrue();
        passwordHasher.VerifyPassword(password, hash2).Should().BeTrue();
    }
}