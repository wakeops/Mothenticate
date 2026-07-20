using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mothenticate.Data.Entities;
using Mothenticate.Data.Repositories;
using Mothenticate.UserManagement.Services;

namespace Mothenticate.Tests;

public class UserServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IUserRepository> _userRepoMock = new();
    private readonly Mock<IGroupRepository> _groupRepoMock = new();
    private readonly Mock<IRoleRepository> _roleRepoMock = new();
    private readonly Mock<IAppSettingsService> _settingsServiceMock = new();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(),
            Mock.Of<IOptions<IdentityOptions>>(),
            Mock.Of<IPasswordHasher<ApplicationUser>>(),
            Array.Empty<IUserValidator<ApplicationUser>>(),
            Array.Empty<IPasswordValidator<ApplicationUser>>(),
            Mock.Of<ILookupNormalizer>(),
            Mock.Of<IdentityErrorDescriber>(),
            Mock.Of<IServiceProvider>(),
            Mock.Of<ILogger<UserManager<ApplicationUser>>>());

        _service = new UserService(
            _userManagerMock.Object,
            _userRepoMock.Object,
            _groupRepoMock.Object,
            _roleRepoMock.Object,
            _settingsServiceMock.Object,
            Mock.Of<ILogger<UserService>>());
    }

    [Fact]
    public async Task CreateAsync_WhenUseEmailAsUsername_SetsUserNameToEmail()
    {
        ApplicationUser? captured = null;
        _settingsServiceMock.Setup(s => s.GetAsync(default)).ReturnsAsync(new AppSettings { UseEmailAsUsername = true });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password1!"))
            .Callback<ApplicationUser, string>((u, _) => captured = u)
            .ReturnsAsync(IdentityResult.Success);

        await _service.CreateAsync("ignored_username", "user@test.com", "Password1!");

        Assert.Equal("user@test.com", captured!.UserName);
    }

    [Fact]
    public async Task CreateAsync_WhenUseEmailAsUsernameDisabled_SetsUserNameToUsername()
    {
        ApplicationUser? captured = null;
        _settingsServiceMock.Setup(s => s.GetAsync(default)).ReturnsAsync(new AppSettings { UseEmailAsUsername = false });
        _userManagerMock.Setup(m => m.CreateAsync(It.IsAny<ApplicationUser>(), "Password1!"))
            .Callback<ApplicationUser, string>((u, _) => captured = u)
            .ReturnsAsync(IdentityResult.Success);

        await _service.CreateAsync("jsmith", "user@test.com", "Password1!");

        Assert.Equal("jsmith", captured!.UserName);
    }

    [Fact]
    public async Task UpdateIdentifiersAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var result = await _service.UpdateIdentifiersAsync("missing", "newname", "new@test.com");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }

    [Fact]
    public async Task LockAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var result = await _service.LockAsync("missing");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }

    [Fact]
    public async Task UnlockAsync_ReturnsFailure_WhenUserNotFound()
    {
        _userManagerMock.Setup(m => m.FindByIdAsync("missing")).ReturnsAsync((ApplicationUser?)null);

        var result = await _service.UnlockAsync("missing");

        Assert.False(result.Succeeded);
        Assert.Contains(result.Errors, e => e.Code == "UserNotFound");
    }

    [Fact]
    public async Task GetGroupsAsync_ReturnsOnlyGroupsUserBelongsTo()
    {
        var group1 = new Group { Id = 1, Name = "Admins" };
        var group2 = new Group { Id = 2, Name = "Devs" };
        var user = new ApplicationUser { Id = "user1" };

        _groupRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([group1, group2]);
        _groupRepoMock.Setup(r => r.GetMembersAsync(1, default)).ReturnsAsync([user]);
        _groupRepoMock.Setup(r => r.GetMembersAsync(2, default)).ReturnsAsync([]);

        var result = await _service.GetGroupsAsync("user1");

        Assert.Single(result);
        Assert.Equal("Admins", result[0].Name);
    }

    [Fact]
    public async Task GetEffectiveRolesAsync_ReturnsDistinctRolesAcrossGroups()
    {
        var group1 = new Group { Id = 1, Name = "Group1" };
        var group2 = new Group { Id = 2, Name = "Group2" };
        var user = new ApplicationUser { Id = "user1" };
        var sharedRole = new AppRole { Id = 1, Name = "Editor" };
        var exclusiveRole = new AppRole { Id = 2, Name = "Viewer" };

        _groupRepoMock.Setup(r => r.GetAllAsync(default)).ReturnsAsync([group1, group2]);
        _groupRepoMock.Setup(r => r.GetMembersAsync(1, default)).ReturnsAsync([user]);
        _groupRepoMock.Setup(r => r.GetMembersAsync(2, default)).ReturnsAsync([user]);
        _roleRepoMock.Setup(r => r.GetByGroupAsync(1, default)).ReturnsAsync([sharedRole, exclusiveRole]);
        _roleRepoMock.Setup(r => r.GetByGroupAsync(2, default)).ReturnsAsync([sharedRole]);

        var result = await _service.GetEffectiveRolesAsync("user1");

        Assert.Equal(2, result.Count);
        Assert.All(result, r => Assert.NotNull(r.Name));
    }
}
