using Microsoft.EntityFrameworkCore;
using MyMesSystem_B.Models;

namespace MyMesSystem_B.Data
{
    // 必須繼承 DbContext 類別
    public class MyDbContext : DbContext
    {
        // 建構子：接收從 Program.cs 傳進來的連線設定
        public MyDbContext(DbContextOptions<MyDbContext> options)
            : base(options)
        {
        }

        // 定義資料表：這代表資料庫裡有一個 Products 資料表
        // 且會自動與你的 Product.cs 模型對應
        public DbSet<Product> Products { get; set; }

        public DbSet<UploadPath> UploadPath { get; set; }
        public DbSet<User> User { get; set; }
    }
}