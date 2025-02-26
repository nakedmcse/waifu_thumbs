using Microsoft.EntityFrameworkCore;

namespace Thumbnails
{
    public class DBContext(string connection) : DbContext
    {
        public DbSet<Model.file_upload_model> file_upload_model { get; set; }
        public DbSet<Model.thumbnail_cache_model> thumbnail_cache_model { get; set; }
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseNpgsql(connection);
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.file_upload_model>().ToTable("file_upload_model", t => t.ExcludeFromMigrations());
            modelBuilder.Entity<Model.thumbnail_cache_model>().ToTable("thumbnail_cache_model", t => t.ExcludeFromMigrations());
        }
    }
}