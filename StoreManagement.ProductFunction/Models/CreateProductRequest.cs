namespace StoreManagement.ProductFunction.Models
{
    public class CreateProductRequest
    {
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
    }
}
