using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Store> Store { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Store>(entity =>
            {
                entity.Property(s => s.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.HasIndex(s => s.CompanyId)
                    .HasDatabaseName("IX_Stores_CompanyId");
            });
        }
}