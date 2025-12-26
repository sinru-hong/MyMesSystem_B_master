using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;

namespace MyMesSystem_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _service;

        // 由系統自動注入 Service
        //💡 為什麼要這樣改？（對應你的筆記）
        //解決報錯：透過注入，系統會幫你 new 出物件參考，解決「需要有物件參考」的錯誤。

        //符合架構：這就是你筆記中提到的 「依賴注入(DI)」 實作。

        //生命週期管理：使用 AddScoped，系統會確保在一次網頁請求中，大家都共用同一個實體，節省記憶體並方便管理資料庫連線。
        public ProductController(ProductService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var data = _service.GetFakeProducts();
            return Ok(data); // 這會回傳 JSON [ { "productID": "A01", ... } ]
        }

        [HttpGet("complex")]
        public IActionResult GetComplexData()
        {
            var data = _service.GetComplexProductList();
            return Ok(data); // 雖然是 Hashtable，回傳依然是 [ { "ProductID": "...", "StockQty": 10 }, ... ]
        }
    }
}
