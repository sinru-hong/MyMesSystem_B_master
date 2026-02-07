using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;

namespace MyMesSystem_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get([FromServices] ProductService productService)
        {
            var data = productService.GetFakeProducts();
            return Ok(data); // 這會回傳 JSON [ { "productID": "A01", ... } ]
        }

        [HttpGet("complex")]
        public IActionResult GetComplexData([FromServices] ProductService productService)
        {
            var data = productService.GetComplexProductList();
            return Ok(data); // 雖然是 Hashtable，回傳依然是 [ { "ProductID": "...", "StockQty": 10 }, ... ]
        }

        [HttpGet("GetUsers")]
        public IActionResult GetUsers() // 暫時移除 [FromServices]
        {
            return Ok(new { message = "連線成功" });
        }
    }
}
