using Microsoft.Data.SqlClient;
using MyMesSystem_B.Models;
using System.Data;

namespace MyMesSystem_B.ModelServices
{
    public class UploadPathModelService
    {
        private readonly string _connectionString;

        public UploadPathModelService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<int> AddUploadPathAsync(string fileName, string? remark, string creator)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = @"
            INSERT INTO UploadPath (FilePath, Remark, Creator, CreateTime, IsDeleted)
            VALUES (@FilePath, @Remark, @Creator, GETDATE(), 0);
            SELECT CAST(SCOPE_IDENTITY() as int);";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FilePath", fileName); 
                    cmd.Parameters.AddWithValue("@Remark", (object)remark ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Creator", creator);

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    object result = await cmd.ExecuteScalarAsync();
                    return (result != null) ? (int)result : 0;
                }
            }
        }

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
        public async Task<List<UploadPath>> GetUploadFilesAsync(string? creator, string? dateRange)
        {
            var list = new List<UploadPath>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                string sql = "SELECT * FROM UploadPath WHERE IsDeleted = 0";

                // 1. 處理人員關鍵字
                if (!string.IsNullOrEmpty(creator))
                {
                    sql += " AND Creator LIKE @Creator";
                }

                // 2. 處理日期範疇解析
                DateTime? startDate = null;
                DateTime? endDate = null;

                if (!string.IsNullOrEmpty(dateRange))
                {
                    // 利用 Flatpickr 預設的分隔符號拆分字串 (例如 "2026/02/01 至 2026/02/11")
                    var dates = dateRange.Split(new[] { " 至 ", " to " }, StringSplitOptions.RemoveEmptyEntries);

                    if (dates.Length == 2)
                    {
                        // 成功拆分出兩個日期
                        if (DateTime.TryParse(dates[0], out DateTime start) && DateTime.TryParse(dates[1], out DateTime end))
                        {
                            startDate = start.Date;
                            endDate = end.Date.AddDays(1); // 包含結束當天的整天
                            sql += " AND CreateTime >= @StartDate AND CreateTime < @EndDate";
                        }
                    }
                    else if (dates.Length == 1 && DateTime.TryParse(dates[0], out DateTime singleDate))
                    {
                        // 若只選了一天
                        startDate = singleDate.Date;
                        endDate = singleDate.Date.AddDays(1);
                        sql += " AND CreateTime >= @StartDate AND CreateTime < @EndDate";
                    }
                }

                sql += " ORDER BY CreateTime DESC";

                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrEmpty(creator))
                        cmd.Parameters.AddWithValue("@Creator", $"%{creator}%");

                    if (startDate.HasValue && endDate.HasValue)
                    {
                        cmd.Parameters.AddWithValue("@StartDate", startDate.Value);
                        cmd.Parameters.AddWithValue("@EndDate", endDate.Value);
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
                    // 明確指定參數型別，對效能更好
                    cmd.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                    cmd.Parameters.Add("@Remark", SqlDbType.NVarChar).Value = (object)remark ?? DBNull.Value;
                    cmd.Parameters.Add("@Modifier", SqlDbType.NVarChar).Value = modifier;

                    if (conn.State == ConnectionState.Closed) await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<int> DeleteUploadPathAsync(int id, string modifier)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
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