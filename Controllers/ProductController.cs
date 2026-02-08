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
            return Ok(data); 
        }

        [HttpGet("complex")]
        public IActionResult GetComplexData([FromServices] ProductService productService)
        {
            var data = productService.GetComplexProductList();
            return Ok(data); 
        }

        [HttpGet("GetUsers")]
        public IActionResult GetUsers() // 暫時移除 [FromServices]
        {
            return Ok(new { message = "連線成功" });
        }
    }
}
