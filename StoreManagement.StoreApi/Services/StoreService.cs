
using Microsoft.EntityFrameworkCore;

public class StoreService : IStoreService
{
    private readonly AppDbContext _context;

    public StoreService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<StoreResponseDto> CreateStoreAsync(int companyId, CreateStoreDto createStoreDto)
    {
        if (string.IsNullOrWhiteSpace(createStoreDto.Name))
            throw new ArgumentException("Store name is required", nameof(createStoreDto.Name));
        
        if (string.IsNullOrWhiteSpace(createStoreDto.Address))
            throw new ArgumentException("Store address is required", nameof(createStoreDto.Address));
        
        var store = new Store
        {
            CompanyId = companyId,
            Name = createStoreDto.Name.Trim(),
            Address = createStoreDto.Address.Trim(),
            CreatedAt = DateTime.UtcNow
        };
        
        _context.Store.Add(store);
        await _context.SaveChangesAsync();
        
        return new StoreResponseDto
        {
            StoreId = store.StoreId,
            CompanyId = store.CompanyId,
            Name = store.Name,
            Address = store.Address,
            CreatedAt = store.CreatedAt
        };
    }

    public Task<bool> DeleteStoreAsync(int companyId, int storeId)
    {
        throw new NotImplementedException();
    }

    public async Task<StoreResponseDto?> GetStoreByIdAsync(int companyId, int storeId)
    {
        var store = await _context.Store
            .Where(s => s.CompanyId == companyId && s.StoreId == storeId)
            .Select(s => new StoreResponseDto
            {
                StoreId = s.StoreId,
                CompanyId = s.CompanyId,
                Name = s.Name,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            })
            .FirstOrDefaultAsync();
            
            return store;
    }

    public async Task<IEnumerable<StoreResponseDto>> GetStoresAsync(int companyId)
    {
        var stores = await _context.Store
            .Where(s => s.CompanyId == companyId)
            .Select(s => new StoreResponseDto
            {
                StoreId = s.StoreId,
                CompanyId = s.CompanyId,
                Name = s.Name,
                Address = s.Address,
                CreatedAt = s.CreatedAt
            })
            .ToListAsync();

        return stores;
    }

    public Task<bool> StoreExistsAsync(int companyId, int storeId)
    {
        throw new NotImplementedException();
    }

    public Task<StoreResponseDto?> UpdateStoreAsync(int companyId, int storeId, UpdateStoreDto updateStoreDto)
    {
        throw new NotImplementedException();
    }
}
