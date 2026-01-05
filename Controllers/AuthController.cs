using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;

namespace MyMesSystem_B.Controllers
{
    [ApiController] // 確保支援 API 特性（如自動模型驗證）
    [Route("api/[controller]")] // 這裡定義了 api/auth 的路徑
    public class AuthController : ControllerBase
    {
        // 這裡模擬資料庫，之後請換成真正的資料庫查詢
        private static string MockPassword = "1234";
        private static bool MockIsFirstLogin = true;

        [HttpPost("login")] // 這會讓網址變成 api/auth/login
        public IActionResult Login([FromBody] LoginRequest request)
        {
            // 1. 驗證帳號密碼 (範例：admin / 1234)
            if (request.Username == "admin" && request.Password == MockPassword)
            {
                // 2. 驗證成功，回傳你剛才問的 LoginResponse
                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "登入成功",
                    IsFirstLogin = MockIsFirstLogin,
                    Token = "fake-jwt-token-for-test", // 之後再實作真正的 JWT
                    Username = request.Username
                });
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
        public string Username { get; set; }
    }

    // 接收前端傳來的資料結構
    public class LoginRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
