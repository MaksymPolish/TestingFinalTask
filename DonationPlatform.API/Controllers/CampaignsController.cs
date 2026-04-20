using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DonationPlatform.API.DTOs;
using DonationPlatform.API.Services;
using DonationPlatform.Core.Entities;
using Microsoft.AspNetCore.Mvc;

namespace DonationPlatform.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaignService;
        private readonly IDonationService _donationService;

        public CampaignsController(ICampaignService campaignService, IDonationService donationService)
        {
            _campaignService = campaignService;
            _donationService = donationService;
        }

        /// Get all active campaigns
        [HttpGet]
        public async Task<ActionResult<List<CampaignDto>>> GetActiveCampaigns()
        {
            var campaigns = await _campaignService.GetActiveCampaignsAsync();
            var dtos = campaigns.Select(c => MapToDto(c)).ToList();
            return Ok(dtos);
        }

        /// Get campaign by id with progress
        [HttpGet("{id}")]
        public async Task<ActionResult<CampaignDto>> GetCampaign(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound("Campaign not found.");
            }

            return Ok(MapToDto(campaign));
        }

        /// Create a new campaign
        [HttpPost]
        public async Task<ActionResult<CampaignDto>> CreateCampaign(CreateCampaignRequest request)
        {
            var campaign = new Campaign
            {
                Title = request.Title,
                Description = request.Description,
                GoalAmount = request.GoalAmount,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                OrganizerId = request.OrganizerId
            };

            try
            {
                var created = await _campaignService.CreateCampaignAsync(campaign);
                return CreatedAtAction(nameof(GetCampaign), new { id = created.Id }, MapToDto(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /// Update a campaign
        [HttpPut("{id}")]
        public async Task<ActionResult<CampaignDto>> UpdateCampaign(int id, UpdateCampaignRequest request)
        {
            var campaign = new Campaign
            {
                Title = request.Title,
                Description = request.Description,
                GoalAmount = request.GoalAmount,
                EndDate = request.EndDate
            };

            try
            {
                var updated = await _campaignService.UpdateCampaignAsync(id, campaign);
                return Ok(MapToDto(updated));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// Make a donation to a campaign
        [HttpPost("{id}/donate")]
        public async Task<ActionResult<DonationDto>> Donate(int id, CreateDonationRequest request)
        {
            var donation = new Donation
            {
                DonorName = request.DonorName,
                DonorEmail = request.DonorEmail,
                Amount = request.Amount,
                Message = request.Message,
                IsAnonymous = request.IsAnonymous
            };

            try
            {
                var created = await _donationService.CreateDonationAsync(id, donation);
                return CreatedAtAction(null, new { id = created.Id }, MapDonationToDto(created));
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        /// Get donations for a campaign (anonymous donations hide donor names)
        [HttpGet("{id}/donations")]
        public async Task<ActionResult<List<DonationDto>>> GetDonations(int id)
        {
            var donations = await _donationService.GetCampaignDonationsAsync(id);
            var dtos = donations.Select(d => MapDonationToDto(d)).ToList();
            return Ok(dtos);
        }

        /// Get campaign statistics
        [HttpGet("{id}/stats")]
        public async Task<ActionResult<CampaignStatsDto>> GetStats(int id)
        {
            var campaign = await _campaignService.GetCampaignByIdAsync(id);
            if (campaign == null)
            {
                return NotFound("Campaign not found.");
            }

            var (totalAmount, averageAmount, donorCount) = await _donationService.GetCampaignStatsAsync(id);
            return Ok(new CampaignStatsDto
            {
                TotalAmount = totalAmount,
                AverageAmount = averageAmount,
                DonorCount = donorCount
            });
        }

        /// Close a campaign
        [HttpPatch("{id}/close")]
        public async Task<ActionResult<CampaignDto>> CloseCampaign(int id)
        {
            try
            {
                var closed = await _campaignService.CloseCampaignAsync(id);
                return Ok(MapToDto(closed));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private CampaignDto MapToDto(Campaign campaign)
        {
            return new CampaignDto
            {
                Id = campaign.Id,
                Title = campaign.Title,
                Description = campaign.Description,
                GoalAmount = campaign.GoalAmount,
                CurrentAmount = campaign.CurrentAmount,
                StartDate = campaign.StartDate,
                EndDate = campaign.EndDate,
                Status = campaign.Status.ToString(),
                OrganizerId = campaign.OrganizerId,
                Organizer = campaign.Organizer != null ? new OrganizerDto
                {
                    Id = campaign.Organizer.Id,
                    Name = campaign.Organizer.Name,
                    Email = campaign.Organizer.Email,
                    Organization = campaign.Organizer.Organization,
                    IsVerified = campaign.Organizer.IsVerified
                } : null
            };
        }

        private DonationDto MapDonationToDto(Donation donation)
        {
            return new DonationDto
            {
                Id = donation.Id,
                CampaignId = donation.CampaignId,
                DonorName = donation.DonorName,
                DonorEmail = donation.DonorEmail,
                Amount = donation.Amount,
                Message = donation.Message,
                CreatedAt = donation.CreatedAt,
                IsAnonymous = donation.IsAnonymous
            };
        }
    }
}
