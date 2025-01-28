using Microsoft.EntityFrameworkCore;

namespace Kinnection
{
    public class KinnectionContext : DbContext
    {
        public KinnectionContext(DbContextOptions<KinnectionContext> options) : base(options) { }
        public DbSet<Book> Book { get; set; }

        public DbSet<Publisher> Publisher { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Publisher>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.Property(e => e.Name).IsRequired();
            });

            modelBuilder.Entity<Book>(entity =>
            {
                entity.HasKey(e => e.ISBN);
                entity.Property(e => e.Title).IsRequired();
                entity.HasOne(d => d.Publisher).WithMany(p => p.Books);
            });
        }
    }
}