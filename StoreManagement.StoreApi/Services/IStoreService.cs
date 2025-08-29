public interface IStoreService
{
    Task<IEnumerable<StoreResponseDto>> GetStoresAsync(int companyId);
    Task<StoreResponseDto?> GetStoreByIdAsync(int companyId, int storeId);
    Task<StoreResponseDto> CreateStoreAsync(int companyId, CreateStoreDto createStoreDto);
    Task<StoreResponseDto?> UpdateStoreAsync(int companyId, int storeId, UpdateStoreDto updateStoreDto);
    Task<bool> DeleteStoreAsync(int companyId, int storeId);
}