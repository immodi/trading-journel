using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Requests.User;
using Domain.Entities;
using FluentAssertions;
using Infrastructure.Services;
using Moq;

namespace Infrastructure.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repositoryMock = new();
    private readonly Mock<IPasswordHasher> _passwordHasherMock = new();

    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(
            _repositoryMock.Object,
            _passwordHasherMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_Should_Return_All_Users()
    {
        // Arrange
        var users = new List<User>
        {
            new()
            {
                Id = 1,
                Username = "user1",
                Email = "user1@test.com",
                PasswordHash = "hash1"
            },
            new()
            {
                Id = 2,
                Username = "user2",
                Email = "user2@test.com",
                PasswordHash = "hash2"
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync())
            .ReturnsAsync(users);

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetByIdAsync_Should_Return_User()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "user",
            Email = "user@test.com",
            PasswordHash = "hash"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        // Act
        var result = await _service.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.Username.Should().Be("user");
    }

    [Fact]
    public async Task AddAsync_Should_Add_User()
    {
        // Arrange
        var request = new CreateUserRequest
        {
            Username = "user",
            Email = "user@test.com",
            Password = "password"
        };

        _passwordHasherMock
            .Setup(h => h.HashPassword(request.Password))
            .Returns("hashedPassword");

        // Act
        await _service.AddAsync(request);

        // Assert
        _passwordHasherMock.Verify(
            h => h.HashPassword(request.Password),
            Times.Once);

        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<User>(u =>
                u.Username == request.Username &&
                u.Email == request.Email &&
                u.PasswordHash == "hashedPassword")),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Update_User()
    {
        // Arrange
        var user = new User
        {
            Id = 1,
            Username = "old",
            Email = "old@test.com",
            PasswordHash = "oldHash"
        };

        var request = new UpdateUserRequest
        {
            Username = "new",
            Email = "new@test.com",
            Password = "newPassword"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(user);

        _passwordHasherMock
            .Setup(h => h.HashPassword(request.Password))
            .Returns("newHash");

        // Act
        await _service.UpdateAsync(1, request);

        // Assert
        user.Username.Should().Be("new");
        user.Email.Should().Be("new@test.com");
        user.PasswordHash.Should().Be("newHash");

        _repositoryMock.Verify(r => r.UpdateAsync(user), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_When_User_Not_Found()
    {
        // Arrange
        var request = new UpdateUserRequest();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync((User?)null);

        // Act
        var act = () => _service.UpdateAsync(1, request);

        // Assert
        await act.Should()
            .ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task DeleteAsync_Should_Delete_User()
    {
        // Act
        await _service.DeleteAsync(1);

        // Assert
        _repositoryMock.Verify(r => r.DeleteAsync(1), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }
}