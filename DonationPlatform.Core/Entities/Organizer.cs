using System.Collections.Generic;

namespace DonationPlatform.Core.Entities
{
    public class Organizer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public bool IsVerified { get; set; }

        // Navigation properties
        public ICollection<Campaign> Campaigns { get; set; } = new List<Campaign>();
    }
}
