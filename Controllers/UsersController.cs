using System.Collections;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;

namespace MyMesSystem_B.Controllers
{
    [ApiController] // 確保支援 API 特性（如自動模型驗證）
    [Route("api/[controller]")] // 這裡定義了 api/auth 的路徑
    public class UsersController : ControllerBase
    {
        //private static string MockPassword = "1234";
        //private static bool MockIsFirstLogin = true;

        [HttpPost("login")] // 這會讓網址變成 api/auth/login
        public IActionResult Login([FromBody] LoginRequest request, [FromServices] UsersService usersService)
        {
            var data = usersService.GetUsers();
            // 1. 驗證帳號密碼 (範例：admin / 1234)
            foreach (Hashtable user in data)
            {
                var EmplNo = user["EmplNo"]?.ToString();
                var PasswordHash = user["PasswordHash"]?.ToString();
                // 確保欄位名稱與資料庫一致 (例如 "Username")
                if (EmplNo == request.EmplNo &&
                    PasswordHash == request.Password)
                {
                    return Ok(new LoginResponse
                    {
                        Success = true,
                        Message = "登入成功",
                        EmplNo = request.EmplNo
                    });
                }
            }

            // 3. 驗證失敗
            return Ok(new LoginResponse
            {
                Success = false,
                Message = "帳號或密碼錯誤"
            });
        }

    }

    public class LoginResponse 
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public bool IsFirstLogin { get; set; }
        public string Token { get; set; }
        public string EmplNo { get; set; }
    }

    // 接收前端傳來的資料結構
    public class LoginRequest
    {
        public string EmplNo { get; set; }
        public string Password { get; set; }
    }
}
