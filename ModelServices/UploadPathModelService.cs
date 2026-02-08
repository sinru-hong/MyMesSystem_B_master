using Microsoft.Data.SqlClient;
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
    }
}