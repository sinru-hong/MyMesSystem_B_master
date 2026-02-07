using Microsoft.Data.SqlClient;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace MyMesSystem_B.ModelServices
{
    public class UsersModelService
    {
        private readonly string _connectionString;

        public UsersModelService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public IList GetUsers(string userKeyword = "")
        {
            IList list = new ArrayList(); // 用來存放多個 Hashtable

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 模擬一個複雜的 Join 查詢（假設有庫存表 Inventory）
                string sql = @"
	                select * from Users";

                if (!string.IsNullOrWhiteSpace(userKeyword))
                    sql += $@"
	                where EmplNo like '%{userKeyword}%' or Username like '%{userKeyword}%' ";

                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    Hashtable ht = new Hashtable();
                    foreach (DataColumn dc in dt.Columns)
                    {
                        // 將欄位名稱當作 Key，儲存格內容當作 Value
                        ht[dc.ColumnName] = dr[dc];
                    }
                    list.Add(ht);
                }
            }
            return list;
        }
    }
}
