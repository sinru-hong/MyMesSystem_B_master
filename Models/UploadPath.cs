namespace MyMesSystem_B.Models
{
    public class UploadPath
    {
        public int Id { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? Remark { get; set; }
        public string Creator { get; set; } = string.Empty;
        public DateTime CreateTime { get; set; } = DateTime.Now;
        public string? LastModifier { get; set; }
        public DateTime? LastModifyTime { get; set; }
    }
}
