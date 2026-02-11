using MyMesSystem_B.Models;
using Microsoft.EntityFrameworkCore; // 記得加上這個
using System.Collections;
using ClosedXML.Parser;
using MyMesSystem_B.Data;

namespace MyMesSystem_B.ModelServices
{
    public class ProductModelService
    {
        // 1. 改為注入 DbContext
        private readonly MyDbContext _context;

        public ProductModelService(MyDbContext context)
        {
            _context = context;
        }

        // 2. 取得產品列表 (強型別 List<Product>)
        public List<Product> GetProductsFromDb()
        {
            // EF Core 會自動幫你 Open Connection、執行 SQL、關閉 Connection
            return _context.Products.ToList();
        }

        // 3. 取得產品列表 (Hashtable 格式)
        public IList GetProductsAsHashTable()
        {
            IList list = new ArrayList();

            // 這裡使用 Select 將資料投影出來
            var products = _context.Products
                .Select(p => new { p.ProductID, p.ProductName, p.Price })
                .ToList();
         //   string sql = @"
	        //SELECT P.ProductID, P.ProductName, P.Price
	        //--, I.StockQty 
	        //FROM Products P
	        //--LEFT JOIN Inventory I ON P.ProductID = I.ProductID";

            foreach (var p in products)
            {
                Hashtable ht = new Hashtable();
                ht["ProductID"] = p.ProductID;
                ht["ProductName"] = p.ProductName;
                ht["Price"] = p.Price;
                list.Add(ht);
            }

            return list;
        }
    }
}