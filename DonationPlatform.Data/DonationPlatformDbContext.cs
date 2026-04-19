using DonationPlatform.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace DonationPlatform.Data
{
    public class DonationPlatformDbContext : DbContext
    {
        public DonationPlatformDbContext(DbContextOptions<DonationPlatformDbContext> options) 
            : base(options)
        {
        }

        public DbSet<Campaign> Campaigns { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<Organizer> Organizers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Campaign
            modelBuilder.Entity<Campaign>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Campaign>()
                .HasOne(c => c.Organizer)
                .WithMany(o => o.Campaigns)
                .HasForeignKey(c => c.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Campaign>()
                .HasMany(c => c.Donations)
                .WithOne(d => d.Campaign)
                .HasForeignKey(d => d.CampaignId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Campaign>()
                .Property(c => c.GoalAmount)
                .HasColumnType("numeric(18,2)");

            modelBuilder.Entity<Campaign>()
                .Property(c => c.CurrentAmount)
                .HasColumnType("numeric(18,2)");

            // Configure Donation
            modelBuilder.Entity<Donation>()
                .HasKey(d => d.Id);

            modelBuilder.Entity<Donation>()
                .Property(d => d.Amount)
                .HasColumnType("numeric(18,2)");

            // Configure Organizer
            modelBuilder.Entity<Organizer>()
                .HasKey(o => o.Id);

            modelBuilder.Entity<Organizer>()
                .Property(o => o.Email)
                .IsRequired();
        }
    }
}
