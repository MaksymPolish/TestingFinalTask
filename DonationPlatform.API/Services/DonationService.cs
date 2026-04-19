using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonationPlatform.Core.Entities;
using DonationPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationPlatform.API.Services
{
    public interface IDonationService
    {
        Task<Donation> CreateDonationAsync(int campaignId, Donation donation);
        Task<List<Donation>> GetCampaignDonationsAsync(int campaignId);
        Task<(decimal TotalAmount, decimal AverageAmount, int DonorCount)> GetCampaignStatsAsync(int campaignId);
    }

    public class DonationService : IDonationService
    {
        private readonly DonationPlatformDbContext _context;

        public DonationService(DonationPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<Donation> CreateDonationAsync(int campaignId, Donation donation)
        {
            var campaign = await _context.Campaigns.FindAsync(campaignId);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campaign not found.");
            }

            if (campaign.Status != CampaignStatus.Active)
            {
                throw new InvalidOperationException("Cannot donate to closed or cancelled campaigns.");
            }

            if (donation.Amount < 1)
            {
                throw new InvalidOperationException("Donation amount must be at least $1.");
            }

            donation.CampaignId = campaignId;
            donation.CreatedAt = DateTime.UtcNow;

            _context.Donations.Add(donation);

            // Update campaign current amount
            campaign.CurrentAmount += donation.Amount;

            // Check if campaign goal is reached
            if (campaign.CurrentAmount >= campaign.GoalAmount)
            {
                campaign.Status = CampaignStatus.Completed;
            }

            await _context.SaveChangesAsync();
            return donation;
        }

        public async Task<List<Donation>> GetCampaignDonationsAsync(int campaignId)
        {
            var donations = await _context.Donations
                .Where(d => d.CampaignId == campaignId)
                .ToListAsync();

            // Hide donor names for anonymous donations
            foreach (var donation in donations.Where(d => d.IsAnonymous))
            {
                donation.DonorName = "Anonymous";
                donation.DonorEmail = null;
            }

            return donations;
        }

        public async Task<(decimal TotalAmount, decimal AverageAmount, int DonorCount)> GetCampaignStatsAsync(int campaignId)
        {
            var donations = await _context.Donations
                .Where(d => d.CampaignId == campaignId)
                .ToListAsync();

            var totalAmount = donations.Sum(d => d.Amount);
            var averageAmount = donations.Count > 0 ? totalAmount / donations.Count : 0;
            var donorCount = donations.Count;

            return (totalAmount, averageAmount, donorCount);
        }
    }
}
