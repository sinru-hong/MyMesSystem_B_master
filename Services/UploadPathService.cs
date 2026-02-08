using MyMesSystem_B.Models;
using MyMesSystem_B.ModelServices;

namespace MyMesSystem_B.Services
{
    public class UploadPathService
    {
        private readonly UploadPathModelService _modelService;

        public UploadPathService(UploadPathModelService modelService)
        {
            _modelService = modelService;
        }

        public async Task<(bool Success, string Message)> ProcessAndSaveData(IFormFile? file, string? remark, string? filePath, string? creator)
        {
            try
            {
                // A. 決定資料庫要存的名稱
                string fileNameForDb = (file != null) ? file.FileName : (filePath ?? "");

                // B. 先存入資料庫
                int rows = await _modelService.AddUploadPathAsync(fileNameForDb, remark, creator ?? "Unknown");

                if (rows <= 0) return (false, "資料庫寫入失敗");

                // C. 處理檔案複製 (C:\Users\洪欣汝\OneDrive\自我學習區\上傳檔案存放區)
                if (file != null && file.Length > 0)
                {
                    try
                    {
                        string rootPath = @"C:\Users\洪欣汝\OneDrive\自我學習區";
                        string targetFolder = Path.Combine(rootPath, "上傳檔案存放區");

                        // 1. 確保資料夾存在
                        if (!Directory.Exists(targetFolder))
                        {
                            Directory.CreateDirectory(targetFolder);
                        }

                        // 2. 清理檔名 (避免非法字元導致報錯)
                        string fileName = Path.GetFileName(file.FileName);
                        string fullSavePath = Path.Combine(targetFolder, fileName);

                        // 3. 儲存檔案
                        using (var stream = new FileStream(fullSavePath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            await file.CopyToAsync(stream);
                            await stream.FlushAsync(); // 確保緩衝區寫入硬碟
                        }

                        Console.WriteLine($"檔案成功儲存至: {fullSavePath}");
                    }
                    catch (Exception ex)
                    {
                        // 💡 這裡非常重要！如果報錯，你會在 Output 視窗看到原因 (例如：存取被拒)
                        throw new Exception($"實體檔案複製失敗: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("警告: 接收到的 file 物件為 null 或長度為 0");
                }

                return (true, "保存成功");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<List<UploadPath>> GetFiles(string? creator, string? date)
        {
            return await _modelService.GetUploadFilesAsync(creator, date);
        }
    }
}