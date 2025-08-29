using System.ComponentModel.DataAnnotations;

public class Store
    {
        [Key]
        public int StoreId { get; set; }
        
        [Required]
        public int CompanyId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(500)]
        public string Address { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }