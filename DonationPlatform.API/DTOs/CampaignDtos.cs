using System;
using System.Collections.Generic;

namespace DonationPlatform.API.DTOs
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal GoalAmount { get; set; }
        public decimal CurrentAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; }
        public int OrganizerId { get; set; }
        public OrganizerDto Organizer { get; set; }
    }

    public class CreateCampaignRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal GoalAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int OrganizerId { get; set; }
    }

    public class UpdateCampaignRequest
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal GoalAmount { get; set; }
        public DateTime EndDate { get; set; }
    }
}
