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
}
