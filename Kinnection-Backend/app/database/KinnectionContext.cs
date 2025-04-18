using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

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

        private ValueConverter DateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
            v => v.HasValue ? v.Value.ToDateTime(TimeOnly.MinValue) : null,
            v => v.HasValue ? DateOnly.FromDateTime(v.Value) : null
        );

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
                entity.HasOne(e => e.Tree);
                entity.Property(e => e.DOB)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.DOD)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            // Relationship Models
            modelBuilder.Entity<Education>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
                entity.Property(e => e.Started)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.Ended)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Hobby>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
                entity.Property(e => e.Started)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.Ended)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<MemberEmail>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
            });

            modelBuilder.Entity<MemberPhone>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
            });

            modelBuilder.Entity<ParentalRelationship>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Parent);
                entity.HasOne(e => e.Child);
                entity.Property(e => e.Adopted)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Residence>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
                entity.Property(e => e.Started)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.Ended)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Spouse>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Husband);
                entity.HasOne(e => e.Wife);
                entity.Property(e => e.Started)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.Ended)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });

            modelBuilder.Entity<Work>(entity =>
            {
                entity.HasKey(e => e.ID);
                entity.HasOne(e => e.Member);
                entity.Property(e => e.Started)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
                entity.Property(e => e.Ended)
                    .HasConversion(DateOnlyConverter)
                    .HasColumnType("date");
            });
        }
    }
}