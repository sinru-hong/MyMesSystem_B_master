using Microsoft.Data.SqlClient;
using MyMesSystem_B.Models;
using System.Collections;
using System.Data;

namespace MyMesSystem_B.ModelServices
{
    public class ProductModelService
    {
        private readonly string _connectionString;
        public ProductModelService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        public List<Product> GetProductsFromDb()
        {
            var list = new List<Product>();
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 這就是你筆記說的：底層處理開啟、執行語句、回傳
                string sql = "SELECT ProductID, ProductName, Price FROM Products";
                SqlDataAdapter da = new SqlDataAdapter(sql, conn);
                DataTable dt = new DataTable();
                da.Fill(dt);

                foreach (DataRow dr in dt.Rows)
                {
                    list.Add(new Product
                    {
                        ProductID = dr["ProductID"].ToString(),
                        ProductName = dr["ProductName"].ToString(),
                        Price = Convert.ToInt32(dr["Price"])
                    });
                }
            }
            return list;
        }

        public IList GetProductsAsHashTable()
        {
            IList list = new ArrayList(); // 用來存放多個 Hashtable

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                // 模擬一個複雜的 Join 查詢（假設有庫存表 Inventory）
                string sql = @"
	        SELECT P.ProductID, P.ProductName, P.Price
	        --, I.StockQty 
	        FROM Products P
	        --LEFT JOIN Inventory I ON P.ProductID = I.ProductID";

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
