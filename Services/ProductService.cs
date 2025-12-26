using System.Collections;
using System.Collections.Generic;
using MyMesSystem_B.Models;
using MyMesSystem_B.ModelServices;

namespace MyMesSystem_B.Services
{
    public class ProductService
    {
        private readonly ProductModelService _modelService;

        // 建構子：跟系統說我需要這個 Service
        public ProductService(ProductModelService modelService)
        {
            _modelService = modelService;
        }

        public List<Product> GetFakeProducts()
        {
            // 使用注入進來的物件實體
            List<Product> returnList = _modelService.GetProductsFromDb();
            return returnList;
        }

        public IList GetComplexProductList()
        {
            // 這裡可以直接回傳 ModelService 抓回來的 Hashtable 清單
            return _modelService.GetProductsAsHashTable();
        }
    }
}
