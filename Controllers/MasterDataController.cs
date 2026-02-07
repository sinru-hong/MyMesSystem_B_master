using System.Collections;
using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace MyMesSystem_B.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        //[HttpGet("GetUsers")]
        //public IActionResult GetUsers() // 暫時移除 [FromServices]
        //{
        //    return Ok(new { message = "連線成功" });
        //}
        [HttpGet("GetUsers")]
        public IActionResult GetUsers([FromServices] UsersService usersService, [FromQuery] string userKeyword = "")
        {
            try
            {
                var data = usersService.GetUsers(userKeyword);
                return Ok(data ?? new ArrayList());
            }
            catch (Exception ex)
            {
                // 💡 在這裡打斷點，看看 ex.Message 是什麼
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
