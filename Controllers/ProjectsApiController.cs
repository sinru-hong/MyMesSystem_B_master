using Microsoft.AspNetCore.Mvc;

namespace MyMesSystem_B.Controllers
{
    [Route("api/ProjectsApi")] 
    [ApiController]
    public class ProjectsApiController : ControllerBase
    {
        [HttpPost("ProcessApiDemo")]
        public IActionResult ProcessApiDemo([FromBody] System.Text.Json.JsonElement data)
        {
            var errors = new List<string>();

            if (!data.TryGetProperty("EquipmentCode", out var code) || string.IsNullOrWhiteSpace(code.GetString()))
            {
                errors.Add("設備代碼 (EquipmentCode) 不能為空。");
            }

            if (!data.TryGetProperty("Qcqty", out var qty) || qty.GetInt32() <= 0)
            {
                errors.Add("檢驗數量 (Qcqty) 必須大於 0。");
            }

            if (errors.Any())
            {
                return BadRequest(new
                {
                    success = false,
                    message = "資料驗證失敗",
                    errorDetails = errors
                });
            }

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