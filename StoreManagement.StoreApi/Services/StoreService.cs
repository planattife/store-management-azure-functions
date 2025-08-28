
using Microsoft.EntityFrameworkCore;

public class StoreService : IStoreService
{
    private readonly AppDbContext _context;

    public StoreService(AppDbContext context)
    {
        _context = context;
    }

    public Task<StoreResponseDto> CreateStoreAsync(int companyId, CreateStoreDto createStoreDto)
    {
        throw new NotImplementedException();
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
