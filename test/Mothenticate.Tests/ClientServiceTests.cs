using Microsoft.AspNetCore.Identity;
using Moq;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.IdentityProvider.Services;
using OpenIddict.Abstractions;

namespace Mothenticate.Tests;

public class ClientServiceTests
{
    private readonly Mock<IOpenIddictApplicationManager> _appManagerMock = new();
    private readonly Mock<IClientSecretRepository> _secretRepoMock = new();
    private readonly Mock<IPasswordHasher<ClientSecret>> _hasherMock = new();
    private readonly ClientService _service;

    public ClientServiceTests()
    {
        _service = new ClientService(
            _appManagerMock.Object,
            _secretRepoMock.Object,
            _hasherMock.Object);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsTrue_WhenSecretMatches()
    {
        var secret = new ClientSecret
        {
            Id = 1,
            ApplicationId = "app1",
            SecretHash = "hashed-value",
            ExpiresAt = null
        };
        _secretRepoMock.Setup(r => r.GetByApplicationIdAsync("app1", default)).ReturnsAsync([secret]);
        _hasherMock.Setup(h => h.VerifyHashedPassword(
                It.IsAny<ClientSecret>(), "hashed-value", "correct-secret"))
            .Returns(PasswordVerificationResult.Success);

        var result = await _service.ValidateSecretAsync("app1", "correct-secret");

        Assert.True(result);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsFalse_WhenSecretDoesNotMatch()
    {
        var secret = new ClientSecret
        {
            Id = 1,
            ApplicationId = "app1",
            SecretHash = "hashed-value",
            ExpiresAt = null
        };
        _secretRepoMock.Setup(r => r.GetByApplicationIdAsync("app1", default)).ReturnsAsync([secret]);
        _hasherMock.Setup(h => h.VerifyHashedPassword(
                It.IsAny<ClientSecret>(), "hashed-value", "wrong-secret"))
            .Returns(PasswordVerificationResult.Failed);

        var result = await _service.ValidateSecretAsync("app1", "wrong-secret");

        Assert.False(result);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsFalse_WhenSecretIsExpired()
    {
        var expired = new ClientSecret
        {
            Id = 1,
            ApplicationId = "app1",
            SecretHash = "hashed-value",
            ExpiresAt = DateTimeOffset.UtcNow.AddHours(-1)
        };
        _secretRepoMock.Setup(r => r.GetByApplicationIdAsync("app1", default)).ReturnsAsync([expired]);

        var result = await _service.ValidateSecretAsync("app1", "any-secret");

        Assert.False(result);
        _hasherMock.Verify(
            h => h.VerifyHashedPassword(It.IsAny<ClientSecret>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task ValidateSecretAsync_ReturnsFalse_WhenNoSecretsExist()
    {
        _secretRepoMock.Setup(r => r.GetByApplicationIdAsync("app1", default))
            .ReturnsAsync(Array.Empty<ClientSecret>());

        var result = await _service.ValidateSecretAsync("app1", "any-secret");

        Assert.False(result);
    }

    [Fact]
    public async Task AddSecretAsync_HashesAndStoresSecret()
    {
        ClientSecret? captured = null;
        _hasherMock.Setup(h => h.HashPassword(It.IsAny<ClientSecret>(), "my-secret"))
            .Returns("hashed-my-secret");
        _secretRepoMock.Setup(r => r.CreateAsync(It.IsAny<ClientSecret>(), default))
            .Callback<ClientSecret, CancellationToken>((s, _) => captured = s)
            .ReturnsAsync(new ClientSecret { Id = 42, ApplicationId = "app1", SecretHash = "hashed-my-secret" });

        var id = await _service.AddSecretAsync("app1", "my-secret", "Test secret");

        Assert.Equal(42, id);
        Assert.Equal("app1", captured!.ApplicationId);
        Assert.Equal("hashed-my-secret", captured.SecretHash);
        Assert.Equal("Test secret", captured.Description);
    }
}
