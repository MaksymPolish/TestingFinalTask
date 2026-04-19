using System.Net;
using Xunit;
using DonationPlatform.API;
using DonationPlatform.API.DTOs;
using DonationPlatform.Core.Entities;
using DonationPlatform.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace DonationPlatform.Tests.Integration
{
    public class CampaignsEndpointsTests : IAsyncLifetime
    {
        private readonly WebApplicationFactory<Program> _factory;
        private HttpClient _client;
        private DonationPlatformDbContext _dbContext;
        private Organizer _verifiedOrganizer;
        private Campaign _activeCampaign;

        public CampaignsEndpointsTests()
        {
            _factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(DbContextOptions<DonationPlatformDbContext>));
                        
                        if (descriptor != null)
                        {
                            services.Remove(descriptor);
                        }

                        services.AddDbContext<DonationPlatformDbContext>(options =>
                            options.UseInMemoryDatabase("integration-test-" + Guid.NewGuid().ToString()));
                    });
                });
        }

        public async Task InitializeAsync()
        {
            _client = _factory.CreateClient();
            
            var scope = _factory.Services.CreateScope();
            _dbContext = scope.ServiceProvider.GetRequiredService<DonationPlatformDbContext>();

            // Seed test data
            _verifiedOrganizer = new Organizer
            {
                Name = "Test Charity",
                Email = "test@charity.org",
                Organization = "Charity Foundation",
                IsVerified = true
            };

            _dbContext.Organizers.Add(_verifiedOrganizer);
            await _dbContext.SaveChangesAsync();

            _activeCampaign = new Campaign
            {
                Title = "Help Local Children",
                Description = "Support education",
                GoalAmount = 10000,
                CurrentAmount = 0,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(3),
                Status = CampaignStatus.Active,
                OrganizerId = _verifiedOrganizer.Id
            };

            _dbContext.Campaigns.Add(_activeCampaign);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DisposeAsync()
        {
            _client?.Dispose();
            _factory?.Dispose();
        }

        [Fact]
        public async Task GetCampaigns_ShouldReturnOkWithActiveCampaigns()
        {
            // Act
            var response = await _client.GetAsync("/api/campaigns");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var campaigns = JsonSerializer.Deserialize<List<CampaignDto>>(content, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(campaigns);
            Assert.Contains(campaigns, c => c.Id == _activeCampaign.Id);
        }

        [Fact]
        public async Task GetCampaignById_ShouldReturnOkWithCampaignDetails()
        {
            // Act
            var response = await _client.GetAsync($"/api/campaigns/{_activeCampaign.Id}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            var content = await response.Content.ReadAsStringAsync();
            var campaign = JsonSerializer.Deserialize<CampaignDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(campaign);
            Assert.Equal(_activeCampaign.Id, campaign.Id);
            Assert.Equal("Help Local Children", campaign.Title);
        }

        [Fact]
        public async Task CreateCampaign_WithValidData_ShouldReturn201()
        {
            // Arrange
            var request = new CreateCampaignRequest
            {
                Title = "New Campaign",
                Description = "New description",
                GoalAmount = 5000,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(1),
                OrganizerId = _verifiedOrganizer.Id
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync("/api/campaigns", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        [Fact]
        public async Task Donate_WithValidData_ShouldReturn201()
        {
            // Arrange
            var donationRequest = new CreateDonationRequest
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 100,
                Message = "Great cause",
                IsAnonymous = false
            };

            var json = JsonSerializer.Serialize(donationRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/campaigns/{_activeCampaign.Id}/donate", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            // Verify campaign amount was updated
            var updatedCampaign = await _dbContext.Campaigns.FindAsync(_activeCampaign.Id);
            Assert.Equal(100, updatedCampaign.CurrentAmount);
        }

        [Fact]
        public async Task Donate_WithInvalidAmount_ShouldReturnBadRequest()
        {
            // Arrange
            var donationRequest = new CreateDonationRequest
            {
                DonorName = "John Doe",
                DonorEmail = "john@example.com",
                Amount = 0.50m,
                Message = "Too small",
                IsAnonymous = false
            };

            var json = JsonSerializer.Serialize(donationRequest);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await _client.PostAsync($"/api/campaigns/{_activeCampaign.Id}/donate", content);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task GetCampaignStats_ShouldReturnCorrectStatistics()
        {
            // Arrange - Add some donations
            var donations = new[]
            {
                new Donation { CampaignId = _activeCampaign.Id, DonorName = "D1", DonorEmail = "d1@ex.com", Amount = 100, CreatedAt = DateTime.UtcNow },
                new Donation { CampaignId = _activeCampaign.Id, DonorName = "D2", DonorEmail = "d2@ex.com", Amount = 50, CreatedAt = DateTime.UtcNow }
            };

            _dbContext.Donations.AddRange(donations);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/campaigns/{_activeCampaign.Id}/stats");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var stats = JsonSerializer.Deserialize<CampaignStatsDto>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(stats);
            Assert.Equal(150, stats.TotalAmount);
            Assert.Equal(2, stats.DonorCount);
        }

        [Fact]
        public async Task AnonymousDonation_ShouldMaskDonorInfo()
        {
            // Arrange
            var donation = new Donation
            {
                CampaignId = _activeCampaign.Id,
                DonorName = "Real Name",
                DonorEmail = "real@example.com",
                Amount = 50,
                IsAnonymous = true,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Donations.Add(donation);
            await _dbContext.SaveChangesAsync();

            // Act
            var response = await _client.GetAsync($"/api/campaigns/{_activeCampaign.Id}/donations");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var content = await response.Content.ReadAsStringAsync();
            var donations_result = JsonSerializer.Deserialize<List<DonationDto>>(content,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(donations_result);
            Assert.Single(donations_result);
            Assert.Equal("Anonymous", donations_result[0].DonorName);
            Assert.Null(donations_result[0].DonorEmail);
        }
    }
}
