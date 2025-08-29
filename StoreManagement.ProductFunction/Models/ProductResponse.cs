namespace StoreManagement.ProductFunction.Models
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public int StoreId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
