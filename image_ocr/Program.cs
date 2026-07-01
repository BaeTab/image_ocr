using System.Drawing;
using System.IO;
using System.Text;
using image_ocr.Ocr;

namespace image_ocr
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // 헤드리스 셀프테스트: OCR 파이프라인을 GUI 없이 검증한다.
            //   ImageOcr.exe --selftest <이미지경로> <결과txt경로>
            // 이미지가 없으면 샘플 이미지를 자동 생성해서 인식한다.
            if (args.Length >= 3 && args[0] == "--selftest")
            {
                SelfTest(args[1], args[2]);
                return;
            }

            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            var form = new Form1();

            // "연결 프로그램"/명령줄로 이미지 경로를 넘기면 시작 시 자동 로드.
            if (args.Length >= 1 && File.Exists(args[0]))
                form.LoadImageOnStartup(args[0]);

            Application.Run(form);
        }

        private static void SelfTest(string imagePath, string reportPath)
        {
            var report = new StringBuilder();
            try
            {
                if (!File.Exists(imagePath))
                {
                    CreateSampleImage(imagePath);
                    report.AppendLine($"[샘플 이미지 생성] {imagePath}");
                }

                using var bmp = new Bitmap(imagePath);
                report.AppendLine($"[입력] {imagePath} ({bmp.Width}x{bmp.Height})");
                report.AppendLine();

                IOcrEngine[] engines = { new WindowsOcrEngine(), new TesseractOcrEngine() };
                foreach (IOcrEngine engine in engines)
                {
                    report.AppendLine($"===== {engine.DisplayName} =====");
                    if (!engine.IsAvailable)
                    {
                        report.AppendLine($"사용 불가: {engine.UnavailableReason}");
                        report.AppendLine();
                        continue;
                    }

                    try
                    {
                        // STA 스레드에서 WinRT 동기 대기 시 교착을 피하려 스레드풀에서 실행.
                        OcrResult r = Task.Run(() => engine.RecognizeAsync(bmp)).GetAwaiter().GetResult();
                        report.AppendLine($"소요: {r.Elapsed.TotalSeconds:0.00}s, 문자수: {r.CharCount}, " +
                            $"신뢰도: {(r.Confidence.HasValue ? (r.Confidence.Value * 100).ToString("0.0") + "%" : "N/A")}");
                        report.AppendLine("--- 인식 결과 ---");
                        report.AppendLine(r.Text);
                    }
                    catch (Exception ex)
                    {
                        report.AppendLine($"오류: {ex}");
                    }
                    report.AppendLine();
                }

                foreach (IOcrEngine engine in engines)
                    (engine as IDisposable)?.Dispose();
            }
            catch (Exception ex)
            {
                report.AppendLine($"셀프테스트 실패: {ex}");
            }

            File.WriteAllText(reportPath, report.ToString(), new UTF8Encoding(true));
        }

        /// <summary>알려진 한/영 텍스트가 담긴 샘플 이미지를 생성한다(검증용).</summary>
        private static void CreateSampleImage(string path)
        {
            using var bmp = new Bitmap(1000, 320);
            using var g = Graphics.FromImage(bmp);
            g.Clear(Color.White);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            using var font = new Font("맑은 고딕", 36f, FontStyle.Regular);
            using var brush = new SolidBrush(Color.Black);
            g.DrawString("안녕하세요 Hello World", font, brush, new PointF(30, 30));
            g.DrawString("OCR 테스트 12345", font, brush, new PointF(30, 110));
            g.DrawString("이미지에서 텍스트 추출", font, brush, new PointF(30, 190));

            string? dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir))
                Directory.CreateDirectory(dir);
            bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
        }
    }
}
