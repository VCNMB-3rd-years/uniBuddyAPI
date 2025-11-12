namespace uniBuddyAPI.Models
{
    public class VoucherRule
    {
        public string Id { get; set; } = "";
        //what the voucher is measured in like mins studied
        public string MetricType { get; set; } = "";
        //amount of the metric needed to earn the voucher
        public int Threshold { get; set; }
        public string Title { get; set; } = "";
        public string? Description { get; set; }
        public bool Active { get; set; } = true;
    }
}
