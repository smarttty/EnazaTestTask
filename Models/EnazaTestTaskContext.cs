using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.ComponentModel.DataAnnotations;

namespace EnazaTestTask.Models
{
    public partial class EnazaTestTaskContext : DbContext
    {
        IServiceProvider _serviceProvider;
        public EnazaTestTaskContext(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public EnazaTestTaskContext(DbContextOptions<EnazaTestTaskContext> options, IServiceProvider serviceProvider)
            : base(options)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<UserGroup> UserGroups { get; set; } = null!;
        public virtual DbSet<UserState> UserStates { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("User");

                entity.HasIndex(e => e.Login, "UQ__User__5E55825B81D20FD8")
                    .IsUnique();

                entity.Property(e => e.UserId).ValueGeneratedOnAdd();

                entity.Property(e => e.CreatedDate).HasColumnType("date");

                entity.Property(e => e.Login)
                    .HasMaxLength(500)
                    .IsUnicode(false);

                entity.Property(e => e.Password)
                    .HasMaxLength(128)
                    .IsUnicode(false);

                entity.HasOne(d => d.UserGroup)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserGroupId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK__User__UserGroupI__3D5E1FD2");

                entity.HasOne(d => d.UserState)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.UserStateId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .HasConstraintName("FK__User__UserStateI__3E52440B");
            });

            modelBuilder.Entity<UserGroup>(entity =>
            {
                entity.ToTable("UserGroup");

                entity.HasIndex(e => e.Code, "UQ__UserGrou__A25C5AA72099819C")
                    .IsUnique();

                entity.Property(e => e.UserGroupId).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(1000)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserState>(entity =>
            {
                entity.ToTable("UserState");

                entity.HasIndex(e => e.Code, "UQ__UserStat__A25C5AA7A30E8DED")
                    .IsUnique();

                entity.Property(e => e.UserStateId).ValueGeneratedOnAdd();

                entity.Property(e => e.Code)
                    .HasMaxLength(10)
                    .IsUnicode(false);

                entity.Property(e => e.Description)
                    .HasMaxLength(500)
                    .IsUnicode(false);
            });

            modelBuilder.Entity<UserGroup>().Ignore(c => c.Users);
            modelBuilder.Entity<UserState>().Ignore(c => c.Users);

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            ChangeTracker.Entries()
               .Where(e => e.State is EntityState.Added or EntityState.Modified)
               .Select(e => e.Entity)
               .ToList()
               .ForEach(entity =>
               {
                   var validationContext = new ValidationContext(entity);
                   validationContext.InitializeServiceProvider(type => _serviceProvider.GetService(type));
                   Validator.ValidateObject(
                    entity,
                    validationContext,
                    validateAllProperties: true);
               });
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
