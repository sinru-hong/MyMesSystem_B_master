using System.Collections;
using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Models;
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
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> SaveMasterData([FromServices] UploadPathService uploadPathService, [FromForm] IFormFile? file, [FromForm] string? remark, [FromForm] string? filePath, [FromForm] string? creator)
        {
            var result = await uploadPathService.ProcessAndSaveData(file, remark, filePath, creator);

            if (result.Success)
                return Ok(new { success = true, message = result.Message });
            else
                return StatusCode(500, new { success = false, message = "儲存失敗: " + result.Message });
        }

        [HttpGet("GetUploadFiles")]
        public async Task<IActionResult> GetUploadFiles([FromServices] UploadPathService uploadPathService, [FromQuery] string? creator, [FromQuery] string? date)
        {
            try
            {
                var data = await uploadPathService.GetFiles(creator, date);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "查詢失敗: " + ex.Message });
            }
        }

        [HttpGet("DownloadFile")]
        public IActionResult DownloadFile([FromQuery] string fileName)
        {
            string fullPath = Path.Combine(@"C:\Users\洪欣汝\OneDrive\自我學習區\上傳檔案存放區", fileName);

            if (!System.IO.File.Exists(fullPath)) return NotFound("檔案不存在");

            var bytes = System.IO.File.ReadAllBytes(fullPath);
            // 自動辨識 MIME 類型
            return File(bytes, "application/octet-stream", fileName);
        }

        [HttpPost("UpdateMasterData")]
        public async Task<IActionResult> UpdateMasterData([FromServices] UploadPathService uploadPathService, [FromForm] int id, [FromForm] string? remark, [FromForm] string modifier)
        {
            var result = await uploadPathService.UpdateData(id, remark, modifier);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
