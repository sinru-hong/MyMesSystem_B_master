using Microsoft.AspNetCore.Mvc;

namespace MyMesSystem_B.Controllers
{
    [Route("api/ProjectsApi")] // 💡 直接指定路徑，不再使用 [controller]
    [ApiController]
    public class ProjectsApiController : ControllerBase
    {
        [HttpPost("ProcessApiDemo")]
        public IActionResult ProcessApiDemo([FromBody] System.Text.Json.JsonElement data)
        {
            var errors = new List<string>();

            // 💡 模擬驗證 1：檢查 EquipmentCode 是否為空
            if (!data.TryGetProperty("EquipmentCode", out var code) || string.IsNullOrWhiteSpace(code.GetString()))
            {
                errors.Add("設備代碼 (EquipmentCode) 不能為空。");
            }

            // 💡 模擬驗證 2：檢查 Qcqty 是否小於等於 0
            if (!data.TryGetProperty("Qcqty", out var qty) || qty.GetInt32() <= 0)
            {
                errors.Add("檢驗數量 (Qcqty) 必須大於 0。");
            }

            // 判斷驗證是否通過
            if (errors.Any())
            {
                // 傳送失敗：得到哪一個欄位輸入有誤
                return BadRequest(new
                {
                    success = false,
                    message = "資料驗證失敗",
                    errorDetails = errors
                });
            }

            // 傳送成功：得到正確資料回傳
            return Ok(new
            {
                success = true,
                message = "API 處理成功",
                echoData = data
            });
        }

        [HttpGet("ProcessGetDemo")]
        public IActionResult ProcessGetDemo([FromQuery] string message)
        {
            if (string.IsNullOrEmpty(message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "GET 請求失敗：message 參數不得為空"
                });
            }

            return Ok(new
            {
                success = true,
                message = "這是 GET 請求的回應",
                receivedValue = message
            });
        }
    }
}