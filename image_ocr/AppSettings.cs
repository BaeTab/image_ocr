using System.IO;
using System.Text.Json;

namespace image_ocr
{
    /// <summary>사용자 설정(엔진·언어·전처리·창 크기)을 %APPDATA%\ImageOcr\settings.json 에 보존한다.</summary>
    public sealed class AppSettings
    {
        public int EngineIndex { get; set; }
        public string[] Languages { get; set; } = Array.Empty<string>();
        public int PageMode { get; set; }   // 0=단락(Block), 1=자동, 2=한 줄, 3=성김
        public bool Grayscale { get; set; }
        public bool EnhanceContrast { get; set; }
        public bool Binarize { get; set; }
        public bool Upscale2x { get; set; }
        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }

        private static string FilePath => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ImageOcr", "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    string json = File.ReadAllText(FilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch
            {
                // 손상된 설정은 무시하고 기본값 사용.
            }
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string? dir = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(FilePath, json);
            }
            catch
            {
                // 저장 실패는 치명적이지 않으므로 무시.
            }
        }
    }
}
