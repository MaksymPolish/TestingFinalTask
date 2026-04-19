using System;

namespace DonationPlatform.API.DTOs
{
    public class DonationDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class CreateDonationRequest
    {
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public bool IsAnonymous { get; set; }
    }

    public class CampaignStatsDto
    {
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public int DonorCount { get; set; }
    }
}
