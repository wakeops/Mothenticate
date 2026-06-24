using Moq;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.Tests;

public class UserPropertyServiceTests
{
    private readonly Mock<IUserPropertyRepository> _repoMock = new();
    private readonly UserPropertyService _service;

    public UserPropertyServiceTests()
    {
        _service = new UserPropertyService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateDefinitionAsync_ThrowsInvalidOperation_WhenNameAlreadyExists()
    {
        var existing = new UserProperty { Id = 1, Name = "bio", DisplayName = "Bio", Type = PropertyType.Text };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([existing]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateDefinitionAsync("bio", "Biography", PropertyType.Text));
    }

    [Fact]
    public async Task CreateDefinitionAsync_ThrowsInvalidOperation_WhenNameMatchesCaseInsensitively()
    {
        var existing = new UserProperty { Id = 1, Name = "BIO", DisplayName = "Bio", Type = PropertyType.Text };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([existing]);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.CreateDefinitionAsync("bio", "Biography", PropertyType.Text));
    }

    [Fact]
    public async Task CreateDefinitionAsync_CreatesProperty_WhenNameIsUnique()
    {
        var created = new UserProperty { Id = 5, Name = "bio", DisplayName = "Bio", Type = PropertyType.Text };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<UserProperty>(), default)).ReturnsAsync(created);

        var result = await _service.CreateDefinitionAsync("bio", "Bio", PropertyType.Text, isRequired: true);

        Assert.Equal(5, result.Id);
        _repoMock.Verify(r => r.CreateAsync(
            It.Is<UserProperty>(p => p.Name == "bio" && p.IsRequired),
            default), Times.Once);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_ThrowsInvalidOperation_WhenPropertyNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((UserProperty?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateDefinitionAsync(99, "Display", PropertyType.Text, false, false, false));
    }

    [Fact]
    public async Task UpdateDefinitionAsync_UpdatesFields_WhenPropertyExists()
    {
        var property = new UserProperty { Id = 1, Name = "bio", DisplayName = "Old", Type = PropertyType.Text };
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(property);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<UserProperty>(), default)).Returns(Task.CompletedTask);

        await _service.UpdateDefinitionAsync(1, "Biography", PropertyType.Text, isRequired: true, isHidden: false, isReadOnly: true);

        Assert.Equal("Biography", property.DisplayName);
        Assert.True(property.IsRequired);
        Assert.True(property.IsReadOnly);
    }
}
