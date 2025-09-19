namespace uniBuddyAPI.Models
{
    public class UserSettings
    {
        public string UserId { get; set; }
        public string Language { get; set; }
        public bool NotificationsEnabled { get; set; }
        public bool AutoSync { get; set; }
    }
}
