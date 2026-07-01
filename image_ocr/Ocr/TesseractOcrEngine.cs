using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using Tesseract;

namespace image_ocr.Ocr
{
    /// <summary>
    /// Tesseract 오픈소스 OCR 엔진.
    /// tessdata\ 폴더의 학습데이터(kor, eng 등)를 사용하며, 인식 언어를 런타임에 바꿀 수 있다.
    /// 단어별 좌표(하이라이트용)도 함께 반환한다.
    /// </summary>
    public sealed class TesseractOcrEngine : IOcrEngine, IDisposable
    {
        private readonly string _tessdataPath;
        private readonly bool _available;
        private readonly string? _unavailableReason;
        private readonly IReadOnlyList<string> _availableLanguages;

        private readonly object _gate = new();
        private TesseractEngine? _engine;
        private string _language;   // 예: "kor+eng"
        private volatile bool _disposed;

        // 페이지 분할 모드. 한글 텍스트 블록은 SingleBlock 이 Auto 보다 정확도가 훨씬 높다.
        private volatile PageSegMode _psm = PageSegMode.SingleBlock;

        public TesseractOcrEngine()
        {
            // 단일파일 환경에서도 동작하도록 tessdata 경로를 확보(+ 네이티브 탐색경로 설정).
            _tessdataPath = RuntimeAssets.EnsureTessdata();

            _availableLanguages = ScanLanguages(_tessdataPath);

            if (_availableLanguages.Count == 0)
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
                // 기본: kor+eng 우선, 없으면 있는 것 전부.
                var prefer = new[] { "kor", "eng" }.Where(_availableLanguages.Contains).ToList();
                _language = string.Join('+', prefer.Count > 0 ? prefer : _availableLanguages);
            }
        }

        public string DisplayName => $"Tesseract ({_language})";

        public bool IsAvailable => _available;

        public string? UnavailableReason => _unavailableReason;

        /// <summary>tessdata 에서 감지된 사용 가능한 언어 코드(예: kor, eng).</summary>
        public IReadOnlyList<string> AvailableLanguages => _availableLanguages;

        /// <summary>현재 선택된 언어 코드 목록.</summary>
        public IReadOnlyList<string> SelectedLanguages =>
            _language.Split('+', StringSplitOptions.RemoveEmptyEntries);

        /// <summary>인식 모드(페이지 분할)를 설정한다. 기본은 단락(Block).</summary>
        public void SetPageMode(OcrPageMode mode) => _psm = mode switch
        {
            OcrPageMode.Auto => PageSegMode.Auto,
            OcrPageMode.Line => PageSegMode.SingleLine,
            OcrPageMode.Sparse => PageSegMode.SparseText,
            _ => PageSegMode.SingleBlock,
        };

        /// <summary>인식 언어를 바꾼다. 다음 인식 때 엔진이 새 언어로 재생성된다.</summary>
        public void SetLanguages(IEnumerable<string> languages)
        {
            var langs = languages
                .Where(l => _availableLanguages.Contains(l))
                .Distinct()
                .ToList();
            if (langs.Count == 0) return;

            string joined = string.Join('+', langs);
            lock (_gate)
            {
                if (joined == _language) return;
                _language = joined;
                _engine?.Dispose();   // 다음 인식 때 새 언어로 재생성
                _engine = null;
            }
        }

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
                    if (_engine == null)
                    {
                        _engine = new TesseractEngine(_tessdataPath, _language, EngineMode.Default);
                        // 정확도 향상: 해상도 힌트(없으면 저해상 이미지 인식이 크게 나빠짐) + 단어 간격 보존.
                        _engine.SetVariable("user_defined_dpi", "300");
                        _engine.SetVariable("preserve_interword_spaces", "1");
                    }

                    // Bitmap → PNG 바이트 → Pix (System.Drawing 직접 의존 없이 안전하게 변환).
                    byte[] png;
                    using (var ms = new MemoryStream())
                    {
                        image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                        png = ms.ToArray();
                    }

                    using Pix pix = Pix.LoadFromMemory(png);
                    using Page page = _engine.Process(pix, _psm);

                    string text = page.GetText() ?? string.Empty;
                    float confidence = page.GetMeanConfidence(); // 0.0 ~ 1.0
                    var words = ExtractWords(page);

                    sw.Stop();
                    return new OcrResult(
                        Text: NormalizeLineEndings(text).TrimEnd('\r', '\n'),
                        EngineName: DisplayName,
                        Elapsed: sw.Elapsed,
                        Confidence: confidence,
                        Words: words);
                }
            }, cancellationToken);
        }

        private static List<OcrWordBox> ExtractWords(Page page)
        {
            var words = new List<OcrWordBox>();
            using ResultIterator iter = page.GetIterator();
            iter.Begin();
            do
            {
                if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Rect r))
                {
                    string w = iter.GetText(PageIteratorLevel.Word) ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(w))
                        words.Add(new OcrWordBox(w, new Rectangle(r.X1, r.Y1, r.Width, r.Height)));
                }
            }
            while (iter.Next(PageIteratorLevel.Word));
            return words;
        }

        private static IReadOnlyList<string> ScanLanguages(string tessdataPath)
        {
            if (!Directory.Exists(tessdataPath)) return Array.Empty<string>();
            return Directory.GetFiles(tessdataPath, "*.traineddata")
                .Select(f => Path.GetFileNameWithoutExtension(f))
                .Where(n => !string.Equals(n, "osd", StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n)
                .ToList();
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
