using System.Collections;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Services;

namespace MyMesSystem_B.Controllers
{
    [ApiController] 
    [Route("api/[controller]")] 
    public class UsersController : ControllerBase
    {
        //private static string MockPassword = "1234";
        //private static bool MockIsFirstLogin = true;

        public string GetMd5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                // 將 Byte 陣列轉換為 16 進位字串
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        [HttpPost("login")] 
        public IActionResult Login([FromBody] LoginRequest request, [FromServices] UsersService usersService)
        {
            var data = usersService.GetUsers();
            foreach (Hashtable user in data)
            {
                var EmplNo = user["EmplNo"]?.ToString();
                var PasswordHash = user["PasswordHash"]?.ToString();
                string encryptedRequestPassword = GetMd5Hash(request.Password);

                if (EmplNo == request.EmplNo && PasswordHash == encryptedRequestPassword)
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
