namespace MyMesSystem_B.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } 
        public bool IsFirstLogin { get; set; }  
        public DateTime CreatedAt { get; set; }
    }
}
