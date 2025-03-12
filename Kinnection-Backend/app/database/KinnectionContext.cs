using Microsoft.EntityFrameworkCore;

namespace Kinnection
{
    public class KinnectionContext : DbContext
    {
        public KinnectionContext(DbContextOptions<KinnectionContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        
        public DbSet<Authentication> Authentications { get; set; }
        
        public DbSet<Password> Passwords { get; set; }

        public DbSet<Tree> Trees { get; set; }

        public DbSet<Member> Members { get; set; }

        public DbSet<Education> Educations { get; set; }

        public DbSet<Encryption> EncryptionKeys { get; set; }

        public DbSet<Hobby> Hobbies { get; set; }

        public DbSet<MemberEmail> Emails { get; set; }

        public DbSet<MemberPhone> Phones { get; set; }

        public DbSet<ParentalRelationship> ParentalRelationships { get; set; }

        public DbSet<Residence> Residences { get; set; }

        public DbSet<Spouse> Spouses { get; set; }

        public DbSet<Work> Works { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Encryption Models
            modelBuilder.Entity<Encryption>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            // User Models
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Authentication>(entity =>
            {
                entity.HasKey(e => e.ID);
            });

            modelBuilder.Entity<Password>(entity =>
            {
                entity.HasKey(e => e.ID);
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

            modelBuilder.Entity<Hobby>(entity =>
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

            modelBuilder.Entity<ParentalRelationship>(entity =>
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