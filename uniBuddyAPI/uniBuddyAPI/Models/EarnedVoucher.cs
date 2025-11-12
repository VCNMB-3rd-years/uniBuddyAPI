namespace uniBuddyAPI.Models
{
    public class EarnedVoucher
    {
        public string VoucherId { get; set; } = "";
        public string Title { get; set; } = "";
        public string MetricType { get; set; } = "";
        public int Threshold { get; set; }
        public int ValueAtAward { get; set; }
        //date the voucher was awarded
        public DateTime AwardedAt { get; set; }
        public string RedeemCode { get; set; } = "";
        public bool Redeemed { get; set; }
        public DateTime? RedeemedAt { get; set; }
    }
}
