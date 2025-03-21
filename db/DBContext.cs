using Microsoft.EntityFrameworkCore;

namespace Thumbnails
{
    public class DBContext(string connection, Dao.DbType dbType) : DbContext
    {
        public DbSet<Model.thumbnail_cache_model> thumbnail_cache_model { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            switch (dbType)
            {
                case Dao.DbType.Postgres:
                    optionsBuilder.UseNpgsql(connection);
                    break;
                case Dao.DbType.SQLite:
                    optionsBuilder.UseSqlite(connection);
                    break;
                default:
                    optionsBuilder.UseSqlite(connection);
                    break;
            }
        }
        
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.thumbnail_cache_model>().ToTable("thumbnail_cache_model", t => t.ExcludeFromMigrations());
        }
    }
}