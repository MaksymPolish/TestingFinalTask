using AutoFixture;
using AutoFixture.Kernel;
using Moq;
using Xunit;
using DonationPlatform.API.Services;
using DonationPlatform.Core.Entities;
using DonationPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationPlatform.Tests.Unit
{
    public class DonationServiceTests
    {
        private readonly Fixture _fixture;
        private readonly DonationPlatformDbContext _context;
        private readonly DonationService _service;

        public DonationServiceTests()
        {
            _fixture = new Fixture();
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            var options = new DbContextOptionsBuilder<DonationPlatformDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DonationPlatformDbContext(options);
            _service = new DonationService(_context);
        }

        [Fact]
        public async Task CreateDonationAsync_WithValidData_ShouldCreateDonation()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .With(c => c.Status, CampaignStatus.Active)
                .With(c => c.GoalAmount, 10000m)
                .With(c => c.CurrentAmount, 0m)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var donation = new Donation
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 100,
                Message = "Great cause",
                IsAnonymous = false
            };

            // Act
            var result = await _service.CreateDonationAsync(campaign.Id, donation);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(100, result.Amount);
            Assert.Equal(campaign.Id, result.CampaignId);
        }

        [Fact]
        public async Task CreateDonationAsync_WithAmountLessThanOne_ShouldThrowException()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .With(c => c.Status, CampaignStatus.Active)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var donation = new Donation
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 0.50m,
                Message = "Too small",
                IsAnonymous = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateDonationAsync(campaign.Id, donation));
        }

        [Fact]
        public async Task CreateDonationAsync_WithClosedCampaign_ShouldThrowException()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .With(c => c.Status, CampaignStatus.Cancelled)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var donation = new Donation
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 100,
                Message = "Message",
                IsAnonymous = false
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateDonationAsync(campaign.Id, donation));
        }

        [Fact]
        public async Task CreateDonationAsync_WhenGoalReached_ShouldCompleteCampaign()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .With(c => c.Status, CampaignStatus.Active)
                .With(c => c.GoalAmount, 100m)
                .With(c => c.CurrentAmount, 0m)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var donation = new Donation
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 100,
                Message = "Final donation",
                IsAnonymous = false
            };

            // Act
            await _service.CreateDonationAsync(campaign.Id, donation);
            
            var updatedCampaign = await _context.Campaigns.FindAsync(campaign.Id);

            // Assert
            Assert.Equal(CampaignStatus.Completed, updatedCampaign.Status);
            Assert.Equal(100, updatedCampaign.CurrentAmount);
        }

        [Fact]
        public async Task GetCampaignDonationsAsync_WithAnonymous_ShouldMaskDonorInfo()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var anonymousDonation = new Donation
            {
                CampaignId = campaign.Id,
                DonorName = "Real Name",
                DonorEmail = "real@example.com",
                Amount = 50,
                Message = "Test message",
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow
            };

            _context.Donations.Add(anonymousDonation);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetCampaignDonationsAsync(campaign.Id);

            // Assert
            Assert.Single(result);
            Assert.Equal("Anonymous", result.First().DonorName);
            Assert.Null(result.First().DonorEmail);
        }

        [Fact]
        public async Task GetCampaignStatsAsync_ShouldCalculateCorrectly()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Without(o => o.Campaigns)
                .Create();
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .Without(c => c.Donations)
                .Without(c => c.Organizer)
                .Create();
            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var donations = new[]
            {
                new Donation { CampaignId = campaign.Id, Amount = 100, Message = "Test", CreatedAt = DateTime.UtcNow, DonorName = "D1", DonorEmail = "d1@ex.com" },
                new Donation { CampaignId = campaign.Id, Amount = 50, Message = "Test", CreatedAt = DateTime.UtcNow, DonorName = "D2", DonorEmail = "d2@ex.com" },
                new Donation { CampaignId = campaign.Id, Amount = 50, Message = "Test", CreatedAt = DateTime.UtcNow, DonorName = "D3", DonorEmail = "d3@ex.com" }
            };

            _context.Donations.AddRange(donations);
            await _context.SaveChangesAsync();

            // Act
            var (total, average, count) = await _service.GetCampaignStatsAsync(campaign.Id);

            // Assert
            Assert.Equal(200, total);
            Assert.Equal(200m / 3, average);
            Assert.Equal(3, count);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
