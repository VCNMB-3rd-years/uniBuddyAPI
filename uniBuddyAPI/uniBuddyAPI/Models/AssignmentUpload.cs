namespace uniBuddyAPI.Models
{
    public class AssignmentUpload
    {
        public string UploadId { get; set; } = "";
        public string AssignmentId { get; set; } = "";
        public string UserId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public DateTime UploadedAt { get; set; }
    }
}
