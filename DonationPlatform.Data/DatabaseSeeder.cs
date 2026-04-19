using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using DonationPlatform.Core.Entities;

namespace DonationPlatform.Data
{
    public class DatabaseSeeder
    {
        private readonly DonationPlatformDbContext _context;
        private readonly IFixture _fixture;
        private readonly Random _random = new Random();

        public DatabaseSeeder(DonationPlatformDbContext context)
        {
            _context = context;
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        public async Task SeedAsync(int numOrganizers = 50, int campaignsPerOrganizer = 10, int donationsPerCampaign = 20)
        {
            Console.WriteLine("Starting database seed...");

            if (_context.Organizers.Any() || _context.Campaigns.Any() || _context.Donations.Any())
            {
                Console.WriteLine("Database already contains data. Skipping seed.");
                return;
            }

            var organizers = new List<Organizer>();
            for (int i = 0; i < numOrganizers; i++)
            {
                var organizer = _fixture.Build<Organizer>()
                    .Without(o => o.Campaigns)
                    .With(o => o.IsVerified, _random.NextDouble() > 0.15)
                    .Create();
                organizers.Add(organizer);
            }

            _context.Organizers.AddRange(organizers);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {organizers.Count} organizers");

            var campaigns = new List<Campaign>();
            foreach (var organizer in organizers)
            {
                for (int i = 0; i < campaignsPerOrganizer; i++)
                {
                    var campaign = _fixture.Build<Campaign>()
                        .With(c => c.OrganizerId, organizer.Id)
                        .With(c => c.GoalAmount, (decimal)_random.Next(5000, 250000))
                        .With(c => c.CurrentAmount, (decimal)_random.Next(0, 150000))
                        .With(c => c.StartDate, DateTime.SpecifyKind(
                            _fixture.Create<DateTime>().AddMonths(-12),
                            DateTimeKind.Utc))
                        .With(c => c.EndDate, DateTime.SpecifyKind(
                            _fixture.Create<DateTime>().AddMonths(6),
                            DateTimeKind.Utc))
                        .With(c => c.Status, (CampaignStatus)_random.Next(0, 3))
                        .Without(c => c.Donations)
                        .Without(c => c.Organizer)
                        .Create();
                    campaigns.Add(campaign);
                }
            }

            _context.Campaigns.AddRange(campaigns);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {campaigns.Count} campaigns");

            var donations = new List<Donation>();
            foreach (var campaign in campaigns)
            {
                for (int i = 0; i < donationsPerCampaign; i++)
                {
                    var donation = _fixture.Build<Donation>()
                        .With(d => d.CampaignId, campaign.Id)
                        .With(d => d.Amount, (decimal)_random.Next(10, 1000))
                        .With(d => d.CreatedAt, DateTime.SpecifyKind(
                            _fixture.Create<DateTime>(),
                            DateTimeKind.Utc))
                        .With(d => d.IsAnonymous, _random.NextDouble() > 0.85)
                        .Without(d => d.Campaign)
                        .Create();
                    donations.Add(donation);
                }
            }

            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Created {donations.Count} donations");

            Console.WriteLine("Database seed completed successfully");
        }
    }
}
