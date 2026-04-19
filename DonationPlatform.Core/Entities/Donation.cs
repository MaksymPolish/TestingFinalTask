using System;

namespace DonationPlatform.Core.Entities
{
    public class Donation
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public string DonorName { get; set; }
        public string DonorEmail { get; set; }
        public decimal Amount { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsAnonymous { get; set; }

        // Navigation properties
        public Campaign Campaign { get; set; }
    }
}
