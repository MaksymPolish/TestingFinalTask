using System;
using System.Collections.Generic;

namespace DonationPlatform.Core.Entities
{
    public class Campaign
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public CampaignStatus Status { get; set; }
        public int OrganizerId { get; set; }

        // Navigation properties
        public Organizer Organizer { get; set; }
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
    }

    public enum CampaignStatus
    {
        Active = 0,
        Completed = 1,
        Cancelled = 2
    }
}
