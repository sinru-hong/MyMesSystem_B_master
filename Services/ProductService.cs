using System.Collections;
using System.Collections.Generic;
using MyMesSystem_B.Models;
using MyMesSystem_B.ModelServices;

namespace MyMesSystem_B.Services
{
    public class ProductService
    {
        private readonly ProductModelService _modelService;

        public ProductService(ProductModelService modelService)
        {
            _modelService = modelService;
        }

        public List<Product> GetFakeProducts()
        {
            List<Product> returnList = _modelService.GetProductsFromDb();
            return returnList;
        }

        public IList GetComplexProductList()
        {
            return _modelService.GetProductsAsHashTable();
        }
    }
}
