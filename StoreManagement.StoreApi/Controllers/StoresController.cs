using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class StoresController : ControllerBase
{
    private readonly IStoreService _storeService;

    public StoresController(IStoreService storeService)
    {
        _storeService = storeService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<StoreResponseDto>>> GetStores()
    {
        try
        {
            var companyId = GetCompanyIdFromHeader();
            var stores = await _storeService.GetStoresAsync(companyId);
            return Ok(stores);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<StoreResponseDto>> GetStore(int id)
    {
        try
        {
            var companyId = GetCompanyIdFromHeader();
            var store = await _storeService.GetStoreByIdAsync(companyId, id);
            
            if (store == null)
                return NotFound($"Store with ID {id} not found");
            
            return Ok(store);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    public async Task<ActionResult<StoreResponseDto>> CreateStore(CreateStoreDto createStoreDto)
    {
        try
        {
            var companyId = GetCompanyIdFromHeader();
            var store = await _storeService.CreateStoreAsync(companyId, createStoreDto);
            
            return CreatedAtAction(
                nameof(GetStore), 
                new { id = store.StoreId }, 
                store);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<StoreResponseDto>> UpdateStore(int id, UpdateStoreDto updateStoreDto)
    {
        try
        {
            var companyId = GetCompanyIdFromHeader();
            var store = await _storeService.UpdateStoreAsync(companyId, id, updateStoreDto);
            
            if (store == null)
                return NotFound($"Store with ID {id} not found");
            
            return Ok(store);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteStore(int id)
    {
        try
        {
            var companyId = GetCompanyIdFromHeader();
            var result = await _storeService.DeleteStoreAsync(companyId, id);
            
            if (!result)
                return NotFound($"Store with ID {id} not found");
            
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private int GetCompanyIdFromHeader()
    {
        if (Request.Headers.TryGetValue("X-Company-Id", out var companyIdHeader))
        {
            if (int.TryParse(companyIdHeader.First(), out var companyId))
            {
                return companyId;
            }
        }
        throw new ArgumentException("X-Company-Id header is required and must be a valid integer");
    }
}