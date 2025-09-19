namespace uniBuddyAPI.Models
{
    public class User
    {
        public string UserId { get; set; }
        public string Name { get; set; }            
        public string Surname { get; set; }         
        public string Email { get; set; }           
        public string Password { get; set; } 
        public string PasswordHash { get; set; }
    }
}
