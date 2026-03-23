using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BookingSportsField.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Facility> Facilities { get; set; }
        public DbSet<Field> Fields { get; set; }
        public DbSet<Favorite> Favorites { get; set; }
        public DbSet<FacilityImage> Images { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<SupportTicket> SupportTickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => new { f.UserId, f.FacilityId })
                .IsUnique();

            // Booking - Payment
            modelBuilder.Entity<Booking>()
                .HasMany(b => b.Payments)
                .WithOne(p => p.Booking)
                .HasForeignKey(p => p.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser - Booking
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Bookings)
                .WithOne(b => b.User)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser - Review
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Reviews)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser - Facility (FieldOwner)
            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.Facilities)
                .WithOne(f => f.FieldOwner)
                .HasForeignKey(f => f.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Facility - Fields
            modelBuilder.Entity<Facility>()
                .HasMany(f => f.Fields)
                .WithOne(field => field.Facility)
                .HasForeignKey(field => field.FacilityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Facility - Images
            modelBuilder.Entity<Facility>()
                .HasMany(f => f.Images)
                .WithOne(i => i.Facility)
                .HasForeignKey(i => i.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Facility - Reviews
            modelBuilder.Entity<Facility>()
                .HasMany(f => f.Reviews)
                .WithOne(r => r.Facility)
                .HasForeignKey(r => r.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Field - Bookings
            modelBuilder.Entity<Field>()
                .HasMany(f => f.Bookings)
                .WithOne(b => b.Field)
                .HasForeignKey(b => b.FieldId)
                .OnDelete(DeleteBehavior.Cascade);

            // Favorite - Facility
            modelBuilder.Entity<Favorite>()
                .HasOne(fav => fav.Facility)
                .WithMany()
                .HasForeignKey(fav => fav.FacilityId)
                .OnDelete(DeleteBehavior.Restrict);

            // Favorite - User
            modelBuilder.Entity<Favorite>()
                .HasOne(fav => fav.User)
                .WithMany()
                .HasForeignKey(fav => fav.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notification - User
            modelBuilder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany()
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // SupportTicket - User
            modelBuilder.Entity<SupportTicket>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);


        }

    }
}