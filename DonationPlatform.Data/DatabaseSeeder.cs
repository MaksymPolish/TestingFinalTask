using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bogus;
using DonationPlatform.Core.Entities;

namespace DonationPlatform.Data
{
    public class DatabaseSeeder
    {
        private readonly DonationPlatformDbContext _context;

        public DatabaseSeeder(DonationPlatformDbContext context)
        {
            _context = context;
        }

        public async Task SeedAsync(int numOrganizers = 8, int campaignsPerOrganizer = 4, int donationsPerCampaign = 20)
        {
            Console.WriteLine("Starting database seed...");

            // Check if data exists
            if (_context.Organizers.Any() || _context.Campaigns.Any() || _context.Donations.Any())
            {
                Console.WriteLine("Database already contains data. Skipping seed.");
                return;
            }

            // Organizer faker
            var organizerFaker = new Faker<Organizer>()
                .RuleFor(o => o.Name, f => f.Company.CompanyName())
                .RuleFor(o => o.Email, f => f.Internet.Email())
                .RuleFor(o => o.Organization, f => f.Company.CompanyName())
                .RuleFor(o => o.IsVerified, f => f.Random.Bool(0.85f));

            // Campaign faker
            var campaignFaker = new Faker<Campaign>()
                .RuleFor(c => c.Title, f => f.Random.Words(3).Join(" "))
                .RuleFor(c => c.Description, f => f.Lorem.Paragraphs(2, 3))
                .RuleFor(c => c.GoalAmount, f => Math.Round(f.Random.Decimal(5000, 250000), 2))
                .RuleFor(c => c.CurrentAmount, (f, c) => Math.Round(f.Random.Decimal(0, c.GoalAmount * 0.95m), 2))
                .RuleFor(c => c.StartDate, f => 
                    DateTime.SpecifyKind(f.Date.PastDateOnly().ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc))
                .RuleFor(c => c.EndDate, (f, c) => 
                    DateTime.SpecifyKind(
                        f.Date.BetweenDateOnly(
                            DateOnly.FromDateTime(c.StartDate),
                            DateOnly.FromDateTime(c.StartDate.AddMonths(12))
                        ).ToDateTime(TimeOnly.MinValue),
                        DateTimeKind.Utc
                    ))
                .RuleFor(c => c.Status, f => f.PickRandom<CampaignStatus>());

            // Donation faker
            var donationFaker = new Faker<Donation>()
                .RuleFor(d => d.DonorName, f => f.Person.FullName)
                .RuleFor(d => d.DonorEmail, f => f.Internet.Email())
                .RuleFor(d => d.Amount, f => Math.Round(f.Random.Decimal(10, 1000), 2))
                .RuleFor(d => d.Message, f => f.Lorem.Sentences(1, 3).Join(" "))
                .RuleFor(d => d.CreatedAt, f => 
                    DateTime.SpecifyKind(f.Date.PastDateOnly().ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc))
                .RuleFor(d => d.IsAnonymous, f => f.Random.Bool(0.15f));

            // Seed organizers
            var organizers = organizerFaker.Generate(numOrganizers);
            _context.Organizers.AddRange(organizers);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {organizers.Count} organizers");

            // Seed campaigns
            var campaigns = new List<Campaign>();
            foreach (var organizer in organizers)
            {
                var orgCampaigns = campaignFaker
                    .RuleFor(c => c.OrganizerId, _ => organizer.Id)
                    .Generate(campaignsPerOrganizer);
                campaigns.AddRange(orgCampaigns);
            }
            _context.Campaigns.AddRange(campaigns);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {campaigns.Count} campaigns");

            // Seed donations
            var donations = new List<Donation>();
            foreach (var campaign in campaigns)
            {
                var campaignDonations = donationFaker
                    .RuleFor(d => d.CampaignId, _ => campaign.Id)
                    .Generate(donationsPerCampaign);
                donations.AddRange(campaignDonations);
            }
            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {donations.Count} donations");

            Console.WriteLine("Database seed completed successfully");
        }
    }
}
