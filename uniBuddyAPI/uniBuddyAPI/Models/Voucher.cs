namespace uniBuddyAPI.Models
{
    public class Voucher
    {
        public string RewardId { get; set; }
        public string UserId { get; set; }
        public string RewardType { get; set; } 
        public string QrCode { get; set; }       
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
