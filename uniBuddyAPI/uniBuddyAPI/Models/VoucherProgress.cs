namespace uniBuddyAPI.Models
{
    public class VoucherProgress
    {
        public string VoucherId { get; set; } = "";
        public string Title { get; set; } = "";
        public string MetricType { get; set; } = "";
        //amount needed to achieve the voucher
        public int Threshold { get; set; }
        //current amount towards the voucher
        public int Current { get; set; }
        //percent towards the voucher
        public int Percent { get; set; }
        //achieved yes or no
        public bool Achieved { get; set; }
    }
}
