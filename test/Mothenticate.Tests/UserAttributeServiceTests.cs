using Moq;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.Tests;

public class UserAttributeServiceTests
{
    private readonly Mock<IUserAttributeRepository> _repoMock = new();
    private readonly UserAttributeService _service;

    public UserAttributeServiceTests()
    {
        _service = new UserAttributeService(_repoMock.Object);
    }

    [Fact]
    public async Task CreateDefinitionAsync_ThrowsInvalidOperation_WhenNameAlreadyExists()
    {
        var existing = new UserAttribute { Id = 1, Name = "bio", DisplayName = "Bio", InputType = AttributeInputType.String };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([existing]);

        var candidate = new UserAttribute { Name = "bio", DisplayName = "Biography", InputType = AttributeInputType.String };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateDefinitionAsync(candidate));
    }

    [Fact]
    public async Task CreateDefinitionAsync_ThrowsInvalidOperation_WhenNameMatchesCaseInsensitively()
    {
        var existing = new UserAttribute { Id = 1, Name = "BIO", DisplayName = "Bio", InputType = AttributeInputType.String };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([existing]);

        var candidate = new UserAttribute { Name = "bio", DisplayName = "Biography", InputType = AttributeInputType.String };

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateDefinitionAsync(candidate));
    }

    [Fact]
    public async Task CreateDefinitionAsync_CreatesAttribute_WhenNameIsUnique()
    {
        var created = new UserAttribute { Id = 5, Name = "bio", DisplayName = "Bio", InputType = AttributeInputType.String };
        _repoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([]);
        _repoMock.Setup(r => r.CreateAsync(It.IsAny<UserAttribute>(), default)).ReturnsAsync(created);

        var result = await _service.CreateDefinitionAsync(new UserAttribute { Name = "bio", DisplayName = "Bio", InputType = AttributeInputType.String, IsRequired = true });

        Assert.Equal(5, result.Id);
        _repoMock.Verify(r => r.CreateAsync(
            It.Is<UserAttribute>(a => a.Name == "bio" && a.IsRequired),
            default), Times.Once);
    }

    [Fact]
    public async Task UpdateDefinitionAsync_ThrowsInvalidOperation_WhenAttributeNotFound()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99, default)).ReturnsAsync((UserAttribute?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.UpdateDefinitionAsync(new UserAttribute { Id = 99, Name = "bio", DisplayName = "Display", InputType = AttributeInputType.String }));
    }

    [Fact]
    public async Task UpdateDefinitionAsync_UpdatesFields_WhenAttributeExists()
    {
        var attribute = new UserAttribute { Id = 1, Name = "bio", DisplayName = "Old", InputType = AttributeInputType.String };
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(attribute);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<UserAttribute>(), default)).Returns(Task.CompletedTask);

        await _service.UpdateDefinitionAsync(new UserAttribute
        {
            Id = 1,
            Name = "bio",
            DisplayName = "Biography",
            InputType = AttributeInputType.String,
            IsRequired = true
        });

        Assert.Equal("Biography", attribute.DisplayName);
        Assert.True(attribute.IsRequired);
    }

    [Fact]
    public async Task DeleteDefinitionAsync_ThrowsInvalidOperation_WhenAttributeIsBuiltIn()
    {
        var attribute = new UserAttribute { Id = 1, Name = "username", DisplayName = "Username", InputType = AttributeInputType.String, IsBuiltIn = true };
        _repoMock.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(attribute);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.DeleteDefinitionAsync(1));

        _repoMock.Verify(r => r.DeleteAsync(It.IsAny<int>(), default), Times.Never);
    }
}
