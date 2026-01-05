namespace MyMesSystem_B.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; } // 修改密碼後更新此處
        public bool IsFirstLogin { get; set; }   // 修改密碼後改為 false
        public DateTime CreatedAt { get; set; }
    }
}
