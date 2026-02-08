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

        public async Task<int> AddUploadPathAsync(string fileName, string? remark, string creator)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 💡 使用 SCOPE_IDENTITY() 取得剛產生的自增 ID
                string sql = @"
            INSERT INTO UploadPath (FilePath, Remark, Creator, CreateTime, IsDeleted)
            VALUES (@FilePath, @Remark, @Creator, GETDATE(), 0);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FilePath", fileName); // 初始先存檔名
                    cmd.Parameters.AddWithValue("@Remark", (object)remark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Creator", creator);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    // 💡 改用 ExecuteScalarAsync 取得回傳的 ID
                    object result = await cmd.ExecuteScalarAsync();
                    return (result != null) ? (int)result : 0;
                }
            }
        }

        // 💡 新增一個更新路徑的專用方法
        public async Task UpdateFilePathAsync(int id, string fullPath)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "UPDATE UploadPath SET FilePath = @FilePath WHERE Id = @Id";
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@FilePath", fullPath);
                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<UploadPath>> GetUploadFilesAsync(string? creator, string? date)
        {
            var list = new List<UploadPath>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 1. 基本 SQL 語句
                string sql = "SELECT * FROM UploadPath WHERE IsDeleted = 0";

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

        public async Task<int> DeleteUploadPathAsync(int id, string modifier)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 💡 軟刪除：更新 IsDeleted 狀態，並記錄是誰刪除的
                string sql = @"
            UPDATE UploadPath 
            SET IsDeleted = 1, 
                LastModifier = @Modifier, 
                LastModifyTime = GETDATE()
            WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);
                    cmd.Parameters.AddWithValue("@Modifier", modifier);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}