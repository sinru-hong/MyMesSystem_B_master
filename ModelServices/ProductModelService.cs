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
            IList list = new ArrayList();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
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
                        ht[dc.ColumnName] = dr[dc];
                    }
                    list.Add(ht);
                }
            }
            return list;
        }
    }
}
