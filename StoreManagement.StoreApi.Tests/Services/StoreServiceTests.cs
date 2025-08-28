using Microsoft.EntityFrameworkCore;

namespace StoreManagement.StoreApi.Tests.Services;

public class StoreServiceTests
{
    private AppDbContext GetInMemoryDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task GetStoresAsync_WhenCompanyHasStores_ReturnsStores()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var stores = new List<Store>
        {
            new Store { StoreId = 1, CompanyId = companyId, Name = "Store 1", Address = "Address 1" },
            new Store { StoreId = 2, CompanyId = companyId, Name = "Store 2", Address = "Address 2" },
            new Store { StoreId = 3, CompanyId = 2, Name = "Store 3", Address = "Address 3" }
        };

        context.Store.AddRange(stores);
        await context.SaveChangesAsync();

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.GetStoresAsync(companyId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.All(result, store => Assert.Equal(companyId, store.CompanyId));
    }

    [Fact]
    public async Task GetStoresAsync_WhenCompanyHasNoStores_ReturnsEmpty()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var storeService = new StoreService(context);

        // Act
        var result = await storeService.GetStoresAsync(999);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetStoreByIdAsync_WhenStoreExistsForCompany_ReturnsStore()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var storeId = 1;
        var store = new Store
        {
            StoreId = storeId,
            CompanyId = companyId,
            Name = "Test Store",
            Address = "Test Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        IStoreService storeService = new StoreService(context);

        // Act
        var result = await storeService.GetStoreByIdAsync(companyId, storeId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal("Test Store", result.Name);
        Assert.Equal("Test Address", result.Address);
    }

    [Fact]
    public async Task GetStoreByIdAsync_WhenStoreNotFound_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        IStoreService storeService = new StoreService(context);

        // Act
        var result = await storeService.GetStoreByIdAsync(1, 999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStoreByIdAsync_WhenStoreExistsButFromADifferentCompany_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var store = new Store
        {
            StoreId = 1,
            CompanyId = 1,
            Name = "Company 1 Store",
            Address = "Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        IStoreService storeService = new StoreService(context);

        // Act
        var result = await storeService.GetStoreByIdAsync(2, 1);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateStoreAsync_WithValidData_CreatesAndReturnsStore()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var createStoreDto = new CreateStoreDto
        {
            Name = "New Store",
            Address = "123 Main Street"
        };

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.CreateStoreAsync(companyId, createStoreDto);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.StoreId > 0);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal("New Store", result.Name);
        Assert.Equal("123 Main Street", result.Address);
        Assert.True(result.CreatedAt <= DateTime.UtcNow);
    }

    [Fact]
    public async Task CreateStoreAsync_StoreIsPersistedInDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var createStoreDto = new CreateStoreDto
        {
            Name = "Persistent Store",
            Address = "123 Main Street"
        };

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.CreateStoreAsync(companyId, createStoreDto);

        // Assert
        var savedStore = await context.Store
            .FirstOrDefaultAsync(s => s.StoreId == result.StoreId);

        Assert.NotNull(savedStore);
        Assert.Equal(companyId, savedStore.CompanyId);
        Assert.Equal("Persistent Store", savedStore.Name);
        Assert.Equal("123 Main Street", savedStore.Address);
    }

    [Theory]
    [InlineData("", "123 Main Street")]
    [InlineData("New Store", "")]
    public async Task CreateStoreAsync_WithInvalidData_ThrowsException(string name, string address)
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var createStoreDto = new CreateStoreDto
        {
            Name = name,
            Address = address
        };

        var storeService = new StoreService(context);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => storeService.CreateStoreAsync(companyId, createStoreDto));
    }

    [Fact]
    public async Task UpdateStoreAsync_WithValidData_UpdatesAndReturnsStore()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var storeId = 1;

        var originalStore = new Store
        {
            StoreId = storeId,
            CompanyId = companyId,
            Name = "New Name",
            Address = "New Address"
        };

        context.Store.Add(originalStore);
        await context.SaveChangesAsync();

        var updateStoreDto = new UpdateStoreDto
        {
            Name = "Updated Name",
            Address = "Updated Address"
        };

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.UpdateStoreAsync(companyId, storeId, updateStoreDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(storeId, result.StoreId);
        Assert.Equal(companyId, result.CompanyId);
        Assert.Equal("Updated Name", result.Name);
        Assert.Equal("Updated Address", result.Address);
    }

    [Fact]
    public async Task UpdateStoreAsync_StoreIsPersistedInDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var storeId = 1;

        var originalStore = new Store
        {
            StoreId = storeId,
            CompanyId = companyId,
            Name = "Before Update",
            Address = "Before Address"
        };

        context.Store.Add(originalStore);
        await context.SaveChangesAsync();

        var updateStoreDto = new UpdateStoreDto
        {
            Name = "After Update",
            Address = "After Address"
        };

        var storeService = new StoreService(context);

        // Act
        await storeService.UpdateStoreAsync(companyId, storeId, updateStoreDto);

        // Assert
        var updatedStore = await context.Store
            .FirstOrDefaultAsync(s => s.StoreId == storeId);

        Assert.NotNull(updatedStore);
        Assert.Equal("After Update", updatedStore.Name);
        Assert.Equal("After Address", updatedStore.Address);
    }

    [Fact]
    public async Task UpdateStoreAsync_WhenStoreNotFound_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var updateStoreDto = new UpdateStoreDto
        {
            Name = "Updated Name",
            Address = "Updated Address"
        };

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.UpdateStoreAsync(1, 999, updateStoreDto);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateStoreAsync_WhenStoreBelongsToDifferentCompany_ReturnsNull()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var store = new Store
        {
            StoreId = 1,
            CompanyId = 1,
            Name = "Company 1 Store",
            Address = "Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        var updateStoreDto = new UpdateStoreDto
        {
            Name = "Company 2 Store",
            Address = "New Address"
        };

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.UpdateStoreAsync(2, 1, updateStoreDto);

        // Assert
        Assert.Null(result);

        var unchangedStore = await context.Store.FindAsync(1);
        Assert.Equal("Company 1 Store", unchangedStore?.Name);
    }

    [Theory]
    [InlineData("", "Address Update")]
    [InlineData("Name Update", "")]
    public async Task UpdateStoreAsync_WithInvalidData_ThrowsException(string name, string address)
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var store = new Store
        {
            StoreId = 1,
            CompanyId = 1,
            Name = "Valid Store",
            Address = "Valid Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        var updateStoreDto = new UpdateStoreDto
        {
            Name = name,
            Address = address
        };

        var storeService = new StoreService(context);

        // Act and Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => storeService.UpdateStoreAsync(1, 1, updateStoreDto));
    }

    [Fact]
    public async Task DeleteStoreAsync_WhenStoreExists_DeletesAndReturnsTrue()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var storeId = 1;

        var store = new Store
        {
            StoreId = storeId,
            CompanyId = companyId,
            Name = "Store to Delete",
            Address = "Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        var storeService = new StoreService(context);

        // Act
        var result = await storeService.DeleteStoreAsync(companyId, storeId);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteStoreAsync_StoreIsRemovedFromDatabase()
    {
        // Arrange
        using var context = GetInMemoryDbContext();

        var companyId = 1;
        var storeId = 1;

        var store = new Store
        {
            StoreId = storeId,
            CompanyId = companyId,
            Name = "Store to Delete",
            Address = "Address"
        };

        context.Store.Add(store);
        await context.SaveChangesAsync();

        var storeService = new StoreService(context);

        // Act
        await storeService.DeleteStoreAsync(companyId, storeId);

        // Assert
        var deletedStore = await context.Store.FindAsync(storeId);
        Assert.Null(deletedStore);

        var storeCount = await context.Store.CountAsync();
        Assert.Equal(0, storeCount);
    }

    [Fact]
    public async Task DeleteStoreAsync_WhenStoreNotFound_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        var storeService = new StoreService(context);

        // Act
        var result = await storeService.DeleteStoreAsync(1, 999);

        // Assert
        Assert.False(result);
    }
    
    [Fact]
    public async Task DeleteStoreAsync_WhenStoreBelongsToDifferentCompany_ReturnsFalse()
    {
        // Arrange
        using var context = GetInMemoryDbContext();
        
        var store = new Store
        {
            StoreId = 1,
            CompanyId = 1,
            Name = "Company 1 Store",
            Address = "Address"
        };
        
        context.Store.Add(store);
        await context.SaveChangesAsync();
        
        var storeService = new StoreService(context);
        
        // Act
        var result = await storeService.DeleteStoreAsync(2, 1);
        
        // Assert
        Assert.False(result);
        
        var unchangedStore = await context.Store.FindAsync(1);
        Assert.NotNull(unchangedStore);
        Assert.Equal("Company 1 Store", unchangedStore.Name);
    }
}
