using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace image_ocr.Ocr
{
    /// <summary>
    /// Tesseract 오픈소스 OCR 엔진.
    /// 실행 파일 옆의 tessdata\ 폴더에 있는 학습데이터(kor, eng)를 사용한다.
    /// 어느 PC에서나 동일하게 동작하며 문서/긴 텍스트 인식 정확도가 높다.
    /// </summary>
    public sealed class TesseractOcrEngine : IOcrEngine, IDisposable
    {
        private readonly string _tessdataPath;
        private readonly string _language;   // 예: "kor+eng"
        private readonly bool _available;
        private readonly string? _unavailableReason;

        private readonly object _gate = new();
        private TesseractEngine? _engine;
        private volatile bool _disposed;

        public TesseractOcrEngine()
        {
            // 단일파일 환경에서도 동작하도록 tessdata 경로를 확보(+ 네이티브 탐색경로 설정).
            _tessdataPath = RuntimeAssets.EnsureTessdata();

            var languages = new List<string>();
            if (File.Exists(Path.Combine(_tessdataPath, "kor.traineddata"))) languages.Add("kor");
            if (File.Exists(Path.Combine(_tessdataPath, "eng.traineddata"))) languages.Add("eng");

            if (languages.Count == 0)
            {
                _available = false;
                _language = "eng";
                _unavailableReason =
                    $"tessdata 폴더에 학습데이터가 없습니다: {_tessdataPath}\n" +
                    "kor.traineddata / eng.traineddata 파일이 필요합니다.";
            }
            else
            {
                _available = true;
                _language = string.Join('+', languages);
            }
        }

        public string DisplayName => $"Tesseract ({_language})";

        public bool IsAvailable => _available;

        public string? UnavailableReason => _unavailableReason;

        public Task<OcrResult> RecognizeAsync(Bitmap image, CancellationToken cancellationToken = default)
        {
            if (!_available)
                throw new InvalidOperationException(_unavailableReason);

            // Tesseract 는 동기/블로킹 API 이므로 백그라운드 스레드에서 실행한다.
            return Task.Run(() =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var sw = Stopwatch.StartNew();

                // TesseractEngine 은 스레드 안전하지 않으므로 생성·처리 전체를 직렬화한다.
                lock (_gate)
                {
                    ObjectDisposedException.ThrowIf(_disposed, this);
                    _engine ??= new TesseractEngine(_tessdataPath, _language, EngineMode.Default);

                    // Bitmap → PNG 바이트 → Pix (System.Drawing 직접 의존 없이 안전하게 변환).
                    byte[] png;
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        png = ms.ToArray();
                    }

                    using Pix pix = Pix.LoadFromMemory(png);
                    using Page page = _engine.Process(pix);

                    string text = page.GetText() ?? string.Empty;
                    float confidence = page.GetMeanConfidence(); // 0.0 ~ 1.0

                    sw.Stop();
                    return new OcrResult(
                        Text: NormalizeLineEndings(text).TrimEnd('\r', '\n'),
                        EngineName: DisplayName,
                        Elapsed: sw.Elapsed,
                        Confidence: confidence);
                }
            }, cancellationToken);
        }

        private static string NormalizeLineEndings(string text) =>
            text.Replace("\r\n", "\n").Replace('\r', '\n');

        public void Dispose()
        {
            lock (_gate)
            {
                _disposed = true;
                _engine?.Dispose();
                _engine = null;
            }
        }
    }
}
