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

                // 2. 動態拼接過濾條件
                if (!string.IsNullOrEmpty(creator))
                {
                    sql += " AND Creator LIKE @Creator";
                }

                // 💡 調整日期查詢邏輯：使用範圍比對
                if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out DateTime startDate))
                {
                    // 取得隔天的日期 (當天 00:00:00 到 隔天 00:00:00)
                    DateTime endDate = startDate.AddDays(1);

                    sql += " AND CreateTime >= @StartDate AND CreateTime < @EndDate";
                }

                sql += " ORDER BY CreateTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    // 參數化查詢
                    if (!string.IsNullOrEmpty(creator))
                    {
                        cmd.Parameters.AddWithValue("@Creator", $"%{creator}%");
                    }

                    if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out startDate))
                    {
                        // 設定當天與隔天的參數
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Date); // 2026-02-08 00:00:00
                        cmd.Parameters.AddWithValue("@EndDate", startDate.Date.AddDays(1)); // 2026-02-09 00:00:00
                    }

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

        public async Task<int> UpdateUploadPathAsync(int id, string? remark, string modifier)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
            UPDATE UploadPath 
            SET Remark = @Remark, 
                LastModifier = @Modifier, 
                LastModifyTime = GETDATE()
            WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Remark", (object)remark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Modifier", modifier);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}