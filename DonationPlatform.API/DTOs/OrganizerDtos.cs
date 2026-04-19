namespace DonationPlatform.API.DTOs
{
    public class OrganizerDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public bool IsVerified { get; set; }
    }

    public class CreateOrganizerRequest
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
    }
}
