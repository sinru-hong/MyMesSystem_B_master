using System.Text;
using MyMesSystem_B.Models;
using MyMesSystem_B.ModelServices;
using ExcelDataReader; // 💡 記得安裝 NuGet 套件
using System.IO;
using MyMesSystem_B.Helpers;
//using System.Text;

namespace MyMesSystem_B.Services
{
    public class UploadPathService
    {
        private readonly UploadPathModelService _modelService;
        private readonly string _remotePath = @"\\localhost\\CompanyData"; // 網路共享路徑

        public UploadPathService(UploadPathModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<(bool Success, string Message)> ProcessAndSaveData(IFormFile? file, string? remark, string? filePath, string? creator)
        {
            try
            {
                // 💡 步驟 1: 確保網路連線。建議將 _remotePath 設為 @"\\localhost\CompanyData"
                bool connected = NetworkConnection.Connect(_remotePath, @"洪欣汝", "haz123");

                // 💡 步驟 2: 權限檢查。Directory.Exists 會確認目前帳號是否真的能看到該資料夾
                if (!Directory.Exists(_remotePath) && !connected)
                {
                    return (false, "無法存取遠端共享資料夾，請檢查權限設定或網路連線。");
                }

                // A. 決定資料庫要存的名稱
                string fileNameForDb = (file != null) ? file.FileName : (filePath ?? "");

                // B. 先存入資料庫取得 ID (此時 FilePath 欄位暫存檔名)
                int newId = await _modelService.AddUploadPathAsync(fileNameForDb, remark, creator ?? "Unknown");
                if (newId <= 0) return (false, "資料庫寫入失敗");

                // C. 處理實體檔案複製
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        // 💡 使用 Path.Combine 結合共享路徑，確保路徑斜線正確
                        string targetFolder = Path.Combine(_remotePath, "上傳檔案存放區");

                        // 1. 確保子資料夾存在
                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        // 2. 準備完整儲存路徑
                        string fileName = Path.GetFileName(file.FileName);
                        string fullSavePath = Path.Combine(targetFolder, fileName);

                        // 3. 儲存檔案
                        using (var stream = new FileStream(fullSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await file.CopyToAsync(stream);
                            await stream.FlushAsync();
                        }

                        // 💡 關鍵步驟：將「最終的網路 UNC 路徑」更新回資料庫
                        await _modelService.UpdateFilePathAsync(newId, fullSavePath);

                        Console.WriteLine($"[成功 檔案已存至共享區並更新資料庫: {fullSavePath}");
                    }
                    catch (Exception ex)
                    {
                        // 若檔案存取失敗，建議在此處記錄日誌
                        throw new Exception($"共享資料夾 IO 失敗: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("資訊: 僅新增資料庫紀錄，未接收到實體檔案。");
                }

                return (true, "保存成功");
            }
            catch (Exception ex)
            {
                // 捕捉所有未預期的錯誤並回傳前端
                return (false, $"系統執行錯誤: {ex.Message}");
            }
        }

        public async Task<List<UploadPath>> GetFiles(string? creator, string? date)
        {
            return await _modelService.GetUploadFilesAsync(creator, date);
        }

        public async Task<(bool Success, string Message)> UpdateData(int id, string? remark, string modifier)
        {
            int rows = await _modelService.UpdateUploadPathAsync(id, remark, modifier);
            return rows > 0 ? (true, "修改成功") : (false, "找不到該筆資料");
        }

        public async Task<(int SuccessCount, string Message)> ImportFromExcelAsync(Stream excelStream, string creator)
        {
            int successCount = 0;
            StringBuilder errorLog = new StringBuilder();

            // 💡 定義基礎共享路徑與目標資料夾
            string remotePath = @"\\localhost\CompanyData";
            string targetFolder = Path.Combine(remotePath, "上傳檔案存放區");

            try
            {
                // 💡 步驟 1: 執行模擬登入 (傳入你的帳號與密碼)
                // 注意：密碼建議從設定檔讀取，不建議寫死
                bool connected = NetworkConnection.Connect(remotePath, @"洪欣汝", "haz123");

                // 💡 步驟 2: 雙重檢查權限 (即使連線回傳失敗，若目錄已存在則繼續)
                if (!Directory.Exists(remotePath) && !connected)
                {
                    return (0, "無法存取遠端共享資料夾，請檢查網路權限。");
                }

                // 1. 確保目標資料夾存在
                if (!Directory.Exists(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

                using (var reader = ExcelReaderFactory.CreateReader(excelStream))
                {
                    if (!reader.Read()) return (0, "Excel 檔案內容為空");

                    while (reader.Read())
                    {
                        string? sourceFilePath = reader.GetValue(1)?.ToString()?.Trim();
                        string? remark = reader.GetValue(2)?.ToString();

                        if (string.IsNullOrEmpty(sourceFilePath)) continue;

                        if (File.Exists(sourceFilePath))
                        {
                            try
                            {
                                string fileName = Path.GetFileName(sourceFilePath);
                                string finalSavePath = Path.Combine(targetFolder, fileName);

                                // 💡 步驟 3: 執行跨網路的檔案複製 
                                File.Copy(sourceFilePath, finalSavePath, true);

                                // 💡 步驟 4: 更新資料庫紀錄 
                                int newId = await _modelService.AddUploadPathAsync(fileName, remark, creator);
                                await _modelService.UpdateFilePathAsync(newId, finalSavePath);

                                successCount++;
                            }
                            catch (Exception ex)
                            {
                                errorLog.AppendLine($"檔案 [{sourceFilePath} 處理失敗: {ex.Message}");
                            }
                        }
                        else
                        {
                            errorLog.AppendLine($"找不到來源檔案: {sourceFilePath}");
                        }
                    }
                }

                string finalMsg = $"導入完成。成功: {successCount} 筆。";
                if (errorLog.Length > 0) finalMsg += " 部分錯誤已記錄於後端日誌。";

                return (successCount, finalMsg);
            }
            catch (Exception ex)
            {
                return (0, $"導入程序崩潰: {ex.Message}");
            }
        }
    }
}