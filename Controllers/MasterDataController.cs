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
        //post測試
        //[HttpPost("SaveMasterData")]
        //public IActionResult SaveMasterData()
        //{
        //    return Ok(new { message = "連線成功，代表路徑沒錯，是參數規格有誤" });
        //}
        [HttpPost("SaveMasterData")]
        public async Task<IActionResult> SaveMasterData([FromForm] IFormFile? file, [FromForm] string? remark, [FromForm] string? filePath,
    [FromForm] string? creator)
        {
            try
            {
                // 1. 處理檔案實體儲存
                if (file != null && file.Length > 0)
                {
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
                    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

                    var savePath = Path.Combine(folderPath, file.FileName);
                    using (var stream = new FileStream(savePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                // 2. 處理資料庫邏輯
                // 這裡你可以將接收到的 filePath 存入資料庫的「檔案路徑」欄位
                // 從UI直接選取的資料無法直接取得電腦的檔案路徑，但excel讀取的可以
                Console.WriteLine($"接收到的檔案路徑文字為: {filePath}");

                return Ok(new { success = true, message = "保存成功！" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "後端保存失敗: " + ex.Message });
            }
        }
    }
}
