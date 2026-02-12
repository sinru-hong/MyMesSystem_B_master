using System.Collections;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using MyMesSystem_B.Models;
using MyMesSystem_B.ModelServices;
using MyMesSystem_B.Services;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;
using ClosedXML.Excel;
using MyMesSystem_B.Helpers;

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
            string fullPath = "";

            if (fileName.StartsWith(@"\\"))
            {
                fullPath = fileName; 
            }
            else
            {
                string targetFolder = @"\\localhost\CompanyData\上傳檔案存放區";
                fullPath = Path.Combine(targetFolder, fileName);
            }

            try
            {
                string remotePath = @"\\localhost\CompanyData";
                NetworkConnection.Connect(remotePath, @"洪欣汝", "haz123");

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound($"檔案不存在。請檢查路徑：{fullPath}");
                }

                var bytes = System.IO.File.ReadAllBytes(fullPath);
                return File(bytes, "application/octet-stream", Path.GetFileName(fullPath));
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"錯誤: {ex.Message}");
            }
        }

        [HttpPost("UpdateMasterData")]
        public async Task<IActionResult> UpdateMasterData([FromServices] UploadPathService uploadPathService, [FromForm] int id, [FromForm] string? remark, [FromForm] string modifier)
        {
            if (id <= 0) return BadRequest(new { Success = false, Message = "無效的 ID" });

            var result = await uploadPathService.UpdateData(id, remark, modifier);

            return result.Success
                ? Ok(new { success = true, message = result.Message })
                : BadRequest(new { success = false, message = result.Message });
        }

        [HttpPost("DeleteMasterData")]
        public async Task<IActionResult> DeleteMasterData([FromServices] UploadPathModelService uploadPathModelService, [FromForm] int id, [FromForm] string modifier)
        {
            try
            {
                int rows = await uploadPathModelService.DeleteUploadPathAsync(id, modifier);
                if (rows > 0) return Ok(new { message = "刪除成功" });
                return BadRequest(new { message = "找不到該筆資料" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }


        [HttpGet("ExportToExcel")]
        public async Task<IActionResult> ExportToExcel([FromServices] UploadPathService uploadPathService, [FromQuery] string? creator, [FromQuery] string? date)
        {
            var data = await uploadPathService.GetFiles(creator, date);

            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("資料匯出");

                // 1. 設定標題
                worksheet.Cell(1, 1).Value = "序號";
                worksheet.Cell(1, 2).Value = "檔案路徑";
                worksheet.Cell(1, 3).Value = "備註";
                worksheet.Cell(1, 4).Value = "建立人";
                worksheet.Cell(1, 5).Value = "建立時間";
                worksheet.Cell(1, 6).Value = "修改人";
                worksheet.Cell(1, 7).Value = "修改時間";

                // 2. 填入資料
                for (int i = 0; i < data.Count; i++)
                {
                    var item = data[i];
                    worksheet.Cell(i + 2, 1).Value = i + 1;
                    worksheet.Cell(i + 2, 2).Value = item.FilePath;
                    worksheet.Cell(i + 2, 3).Value = item.Remark;
                    worksheet.Cell(i + 2, 4).Value = item.Creator;
                    worksheet.Cell(i + 2, 5).Value = item.CreateTime;
                    worksheet.Cell(i + 2, 6).Value = item.LastModifier;
                    worksheet.Cell(i + 2, 7).Value = item.LastModifyTime;
                }

                // 💡 3. 關鍵步驟：根據內容自動調整所有欄位寬度
                worksheet.Columns().AdjustToContents();

                using (var stream = new MemoryStream())
                {
                    workbook.SaveAs(stream);
                    var content = stream.ToArray();
                    return File(content,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        $"Export_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
                }
            }
        }

        [HttpPost("ImportExcel")]
        public async Task<IActionResult> ImportExcel([FromServices] UploadPathService uploadPathService, IFormFile file, [FromForm] string creator)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "請選取 Excel 檔案。" });

            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var result = await uploadPathService.ImportFromExcelAsync(stream, creator);
                    return Ok(new { message = result.Message, count = result.SuccessCount });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
