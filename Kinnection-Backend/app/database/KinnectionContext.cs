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

            // Template Models
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

            // User Models
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Password>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.User);
            });

            // Tree Models
            modelBuilder.Entity<Tree>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Member>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Tree);
            });

            // Relationship Models
            modelBuilder.Entity<Education>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });

            modelBuilder.Entity<Hobbies>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });

            modelBuilder.Entity<MemberEmail>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });

            modelBuilder.Entity<MemberPhone>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });

            modelBuilder.Entity<ParentChild>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Parent);
                entity.HasOne(d => d.Child);
            });

            modelBuilder.Entity<Residence>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });

            modelBuilder.Entity<Spouse>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Husband);
                entity.HasOne(d => d.Wife);
            });

            modelBuilder.Entity<Work>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(d => d.Member);
            });
        }
    }
}