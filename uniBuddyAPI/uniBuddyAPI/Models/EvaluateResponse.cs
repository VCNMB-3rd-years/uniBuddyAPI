namespace uniBuddyAPI.Models
{
    public class EvaluateResponse
    {
        //progress for each of the vouchers
        public List<VoucherProgress> Progress { get; set; } = new List<VoucherProgress>();
        //earned vouchers
        public List<EarnedVoucher> NewlyAwarded { get; set; } = new List<EarnedVoucher>();
    }
}
