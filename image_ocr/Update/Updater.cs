using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace image_ocr.Update
{
    /// <summary>GitHub Releases 를 확인해 새 버전이 있으면 단일 exe 를 교체하는 자동 업데이트기.</summary>
    public static class Updater
    {
        private const string Owner = "BaeTab";
        private const string Repo = "image_ocr";
        private const string AssetName = "ImageOcr.exe";

        private static readonly HttpClient Http = CreateClient();

        public static Version CurrentVersion =>
            Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

        public sealed record UpdateInfo(Version Version, string DownloadUrl, string Tag, string Notes);

        private static HttpClient CreateClient()
        {
            var http = new HttpClient { Timeout = TimeSpan.FromSeconds(20) };
            http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("ImageOcr", "1.0"));
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
            return http;
        }

        /// <summary>최신 릴리즈를 조회한다. 새 버전이 없거나 조회 실패면 null.</summary>
        public static async Task<UpdateInfo?> CheckForUpdateAsync(CancellationToken ct = default)
        {
            try
            {
                string url = $"https://api.github.com/repos/{Owner}/{Repo}/releases/latest";
                using HttpResponseMessage resp = await Http.GetAsync(url, ct).ConfigureAwait(false);
                if (!resp.IsSuccessStatusCode) return null;

                await using Stream stream = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                using JsonDocument doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct).ConfigureAwait(false);
                JsonElement root = doc.RootElement;

                string tag = root.TryGetProperty("tag_name", out var t) ? t.GetString() ?? "" : "";
                if (!TryParseVersion(tag, out Version? latest) || latest is null) return null;
                if (latest <= CurrentVersion) return null;

                // 자산에서 ImageOcr.exe 다운로드 URL 찾기.
                string? downloadUrl = null;
                if (root.TryGetProperty("assets", out var assets) && assets.ValueKind == JsonValueKind.Array)
                {
                    foreach (JsonElement a in assets.EnumerateArray())
                    {
                        string name = a.TryGetProperty("name", out var n) ? n.GetString() ?? "" : "";
                        if (string.Equals(name, AssetName, StringComparison.OrdinalIgnoreCase))
                        {
                            downloadUrl = a.TryGetProperty("browser_download_url", out var d) ? d.GetString() : null;
                            break;
                        }
                    }
                }
                if (downloadUrl is null) return null;

                string notes = root.TryGetProperty("body", out var b) ? b.GetString() ?? "" : "";
                return new UpdateInfo(latest, downloadUrl, tag, notes);
            }
            catch
            {
                return null; // 네트워크/파싱 실패는 조용히 무시(업데이트는 선택적 기능).
            }
        }

        /// <summary>새 exe 를 내려받아 현재 프로세스 종료 후 교체·재실행한다.</summary>
        public static async Task DownloadAndApplyAsync(UpdateInfo info, IProgress<int>? progress = null, CancellationToken ct = default)
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "ImageOcr_update");
            Directory.CreateDirectory(tempDir);
            string newExe = Path.Combine(tempDir, "ImageOcr.new.exe");

            // 다운로드(진행률 보고).
            using (HttpResponseMessage resp = await Http.GetAsync(info.DownloadUrl, HttpCompletionOption.ResponseHeadersRead, ct).ConfigureAwait(false))
            {
                resp.EnsureSuccessStatusCode();
                long? total = resp.Content.Headers.ContentLength;
                await using Stream src = await resp.Content.ReadAsStreamAsync(ct).ConfigureAwait(false);
                await using FileStream dst = File.Create(newExe);

                byte[] buffer = new byte[81920];
                long read = 0;
                int n;
                while ((n = await src.ReadAsync(buffer, ct).ConfigureAwait(false)) > 0)
                {
                    await dst.WriteAsync(buffer.AsMemory(0, n), ct).ConfigureAwait(false);
                    read += n;
                    if (total is > 0)
                        progress?.Report((int)(read * 100 / total.Value));
                }
            }

            string targetExe = Environment.ProcessPath
                ?? Path.Combine(AppContext.BaseDirectory, "ImageOcr.exe");
            int pid = Environment.ProcessId;

            // 현재 프로세스 종료를 기다렸다가 교체·재실행하는 배치 스크립트.
            string bat = Path.Combine(tempDir, "apply_update.bat");
            string script =
                "@echo off\r\n" +
                "chcp 65001 >nul\r\n" +
                ":waitloop\r\n" +
                $"tasklist /FI \"PID eq {pid}\" 2>nul | find \"{pid}\" >nul\r\n" +
                "if not errorlevel 1 (\r\n" +
                "  timeout /t 1 /nobreak >nul\r\n" +
                "  goto waitloop\r\n" +
                ")\r\n" +
                $"copy /Y \"{newExe}\" \"{targetExe}\" >nul\r\n" +
                $"start \"\" \"{targetExe}\"\r\n" +
                $"del \"{newExe}\" >nul 2>&1\r\n";
            File.WriteAllText(bat, script, System.Text.Encoding.UTF8);

            Process.Start(new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{bat}\"",
                CreateNoWindow = true,
                UseShellExecute = false,
                WorkingDirectory = tempDir,
            });
        }

        private static bool TryParseVersion(string tag, out Version? version)
        {
            version = null;
            if (string.IsNullOrWhiteSpace(tag)) return false;
            string cleaned = tag.TrimStart('v', 'V').Trim();
            // "1.2.3" 또는 "1.2.3.4" 만 받아들인다.
            return Version.TryParse(cleaned, out version);
        }
    }
}
