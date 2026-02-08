using Microsoft.Data.SqlClient;
using MyMesSystem_B.Models;
using System.Data;

namespace MyMesSystem_B.ModelServices
{
    public class UploadPathModelService
    {
        private readonly string _connectionString;

        // 從 IConfiguration 注入連線字串
        public UploadPathModelService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> AddUploadPathAsync(string filePath, string? remark, string creator)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // SQL 原生指令
                string sql = @"
                    INSERT INTO UploadPath (FilePath, Remark, Creator, CreateTime, LastModifier, LastModifyTime)
                    VALUES (@FilePath, @Remark, @Creator, GETDATE(), @LastModifier, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 綁定參數防止 SQL 注入
                    cmd.Parameters.AddWithValue("@FilePath", filePath);
                    cmd.Parameters.AddWithValue("@Remark", (object)remark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Creator", creator);
                    cmd.Parameters.AddWithValue("@LastModifier", creator);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

                    // 執行並回傳受影響筆數 (1 代表成功)
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<UploadPath>> GetUploadFilesAsync(string? creator, string? date)
        {
            var list = new List<UploadPath>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 1. 基本 SQL 語句
                string sql = "SELECT * FROM UploadPath WHERE 1=1";

                // 2. 動態拼接過濾條件 (安全起見仍使用參數化)
                if (!string.IsNullOrEmpty(creator)) sql += " AND Creator LIKE @Creator";
                if (!string.IsNullOrEmpty(date)) sql += " AND CAST(CreateTime AS DATE) = @Date";

                sql += " ORDER BY CreateTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(creator)) cmd.Parameters.AddWithValue("@Creator", $"%{creator}%");
                    if (!string.IsNullOrEmpty(date)) cmd.Parameters.AddWithValue("@Date", date);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new UploadPath
                            {
                                Id = (int)reader["Id"],
                                FilePath = reader["FilePath"].ToString() ?? "",
                                Remark = reader["Remark"]?.ToString(),
                                Creator = reader["Creator"].ToString() ?? "",
                                CreateTime = (DateTime)reader["CreateTime"],
                                LastModifier = reader["LastModifier"]?.ToString(),
                                LastModifyTime = reader["LastModifyTime"] as DateTime?
                            });
                        }
                    }
                }
            }
            return list;
        }
    }
}