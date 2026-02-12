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
        private readonly string _remotePath = @"\\localhost\\CompanyData";

        public UploadPathService(UploadPathModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<(bool Success, string Message)> ProcessAndSaveData(IFormFile? file, string? remark, string? filePath, string? creator)
        {
            try
            {
                bool connected = NetworkConnection.Connect(_remotePath, @"洪欣汝", "haz123");
                if (!Directory.Exists(_remotePath) && !connected)
                {
                    return (false, "無法存取遠端共享資料夾，請檢查權限設定或網路連線。");
                }

                string fileNameForDb = (file != null) ? file.FileName : (filePath ?? "");

                int newId = await _modelService.AddUploadPathAsync(fileNameForDb, remark, creator ?? "Unknown");
                if (newId <= 0) return (false, "資料庫寫入失敗");

                if (file != null && file.Length > 0)
                {
                    try
                    {
                        string targetFolder = Path.Combine(_remotePath, "上傳檔案存放區");

                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        string fileName = Path.GetFileName(file.FileName);
                        string fullSavePath = Path.Combine(targetFolder, fileName);

                        using (var stream = new FileStream(fullSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await file.CopyToAsync(stream);
                            await stream.FlushAsync();
                        }

                        await _modelService.UpdateFilePathAsync(newId, fullSavePath);

                        Console.WriteLine($"[成功 檔案已存至共享區並更新資料庫: {fullSavePath}");
                    }
                    catch (Exception ex)
                    {
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

            return rows > 0
                ? (true, "修改成功")
                : (false, "找不到該筆資料或資料未變更");
        }

        public async Task<(int SuccessCount, string Message)> ImportFromExcelAsync(Stream excelStream, string creator)
        {
            int successCount = 0;
            StringBuilder errorLog = new StringBuilder();

            string remotePath = @"\\localhost\CompanyData";
            string targetFolder = Path.Combine(remotePath, "上傳檔案存放區");

            try
            {
                bool connected = NetworkConnection.Connect(remotePath, @"洪欣汝", "haz123");

                if (!Directory.Exists(remotePath) && !connected)
                {
                    return (0, "無法存取遠端共享資料夾，請檢查網路權限。");
                }

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

                                File.Copy(sourceFilePath, finalSavePath, true);

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