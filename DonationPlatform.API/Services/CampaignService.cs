using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonationPlatform.Core.Entities;
using DonationPlatform.Data;
using Microsoft.EntityFrameworkCore;

namespace DonationPlatform.API.Services
{
    public interface ICampaignService
    {
        Task<List<Campaign>> GetActiveCampaignsAsync();
        Task<Campaign> GetCampaignByIdAsync(int id);
        Task<Campaign> CreateCampaignAsync(Campaign campaign);
        Task<Campaign> UpdateCampaignAsync(int id, Campaign campaign);
        Task<Campaign> CloseCampaignAsync(int id);
    }

    public class CampaignService : ICampaignService
    {
        private readonly DonationPlatformDbContext _context;

        public CampaignService(DonationPlatformDbContext context)
        {
            _context = context;
        }

        public async Task<List<Campaign>> GetActiveCampaignsAsync()
        {
            return await _context.Campaigns
                .Where(c => c.Status == CampaignStatus.Active)
                .Include(c => c.Organizer)
                .Include(c => c.Donations)
                .ToListAsync();
        }

        public async Task<Campaign> GetCampaignByIdAsync(int id)
        {
            return await _context.Campaigns
                .Include(c => c.Organizer)
                .Include(c => c.Donations)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Campaign> CreateCampaignAsync(Campaign campaign)
        {
            // Verify organizer is verified
            var organizer = await _context.Organizers.FindAsync(campaign.OrganizerId);
            if (organizer == null || !organizer.IsVerified)
            {
                throw new InvalidOperationException("Only verified organizers can create campaigns.");
            }

            campaign.Status = CampaignStatus.Active;
            campaign.CurrentAmount = 0;

            _context.Campaigns.Add(campaign);
            await _context.SaveChangesAsync();

            return campaign;
        }

        public async Task<Campaign> UpdateCampaignAsync(int id, Campaign updatedCampaign)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campaign not found.");
            }

            campaign.Title = updatedCampaign.Title ?? campaign.Title;
            campaign.Description = updatedCampaign.Description ?? campaign.Description;
            campaign.GoalAmount = updatedCampaign.GoalAmount;
            campaign.EndDate = updatedCampaign.EndDate;

            await _context.SaveChangesAsync();
            return campaign;
        }

        public async Task<Campaign> CloseCampaignAsync(int id)
        {
            var campaign = await _context.Campaigns.FindAsync(id);
            if (campaign == null)
            {
                throw new KeyNotFoundException("Campaign not found.");
            }

            campaign.Status = CampaignStatus.Cancelled;
            await _context.SaveChangesAsync();
            return campaign;
        }
    }
}
