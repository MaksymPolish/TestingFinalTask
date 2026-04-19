using AutoFixture;
using Moq;
using Xunit;
using DonationPlatform.API.Services;
using DonationPlatform.Core.Entities;
using DonationPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationPlatform.Tests.Unit
{
    public class CampaignServiceTests
    {
        private readonly Fixture _fixture;
        private readonly DonationPlatformDbContext _context;
        private readonly CampaignService _service;

        public CampaignServiceTests()
        {
            _fixture = new Fixture();
            var options = new DbContextOptionsBuilder<DonationPlatformDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new DonationPlatformDbContext(options);
            _service = new CampaignService(_context);
        }

        [Fact]
        public async Task CreateCampaignAsync_WithVerifiedOrganizer_ShouldCreateCampaign()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Create();
            
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .Create();

            // Act
            var result = await _service.CreateCampaignAsync(campaign);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(CampaignStatus.Active, result.Status);
            Assert.Equal(0, result.CurrentAmount);
        }

        [Fact]
        public async Task CreateCampaignAsync_WithUnverifiedOrganizer_ShouldThrowException()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, false)
                .Create();
            
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaign = _fixture.Build<Campaign>()
                .With(c => c.OrganizerId, organizer.Id)
                .Create();

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.CreateCampaignAsync(campaign));
        }

        [Fact]
        public async Task GetActiveCampaignsAsync_ShouldReturnOnlyActiveCampaigns()
        {
            // Arrange
            var organizer = _fixture.Build<Organizer>()
                .With(o => o.IsVerified, true)
                .Create();
            
            _context.Organizers.Add(organizer);
            await _context.SaveChangesAsync();

            var campaigns = new[]
            {
                _fixture.Build<Campaign>()
                    .With(c => c.OrganizerId, organizer.Id)
                    .With(c => c.Status, CampaignStatus.Active)
                    .Create(),
                _fixture.Build<Campaign>()
                    .With(c => c.OrganizerId, organizer.Id)
                    .With(c => c.Status, CampaignStatus.Completed)
                    .Create(),
                _fixture.Build<Campaign>()
                    .With(c => c.OrganizerId, organizer.Id)
                    .With(c => c.Status, CampaignStatus.Active)
                    .Create()
            };

            _context.Campaigns.AddRange(campaigns);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetActiveCampaignsAsync();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.All(result, c => Assert.Equal(CampaignStatus.Active, c.Status));
        }

        [Fact]
        public async Task UpdateCampaignAsync_ShouldUpdateCampaignDetails()
        {
            // Arrange
            var campaign = _fixture.Build<Campaign>()
                .With(c => c.Title, "Original Title")
                .With(c => c.GoalAmount, 1000m)
                .Create();

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            var updatedCampaign = new Campaign
            {
                Title = "Updated Title",
                GoalAmount = 2000m,
                EndDate = DateTime.UtcNow.AddDays(30)
            };

            // Act
            var result = await _service.UpdateCampaignAsync(campaign.Id, updatedCampaign);

            // Assert
            Assert.Equal("Updated Title", result.Title);
            Assert.Equal(2000m, result.GoalAmount);
        }

        [Fact]
        public async Task CloseCampaignAsync_ShouldChangeStatusToCancelled()
        {
            // Arrange
            var campaign = _fixture.Build<Campaign>()
                .With(c => c.Status, CampaignStatus.Active)
                .Create();

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.CloseCampaignAsync(campaign.Id);

            // Assert
            Assert.Equal(CampaignStatus.Cancelled, result.Status);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
