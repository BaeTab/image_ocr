using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using Windows.Globalization;
using Windows.Graphics.Imaging;
using Windows.Media.Ocr;
using Windows.Storage.Streams;
using WinRtOcrResult = Windows.Media.Ocr.OcrResult;

namespace image_ocr.Ocr
{
    /// <summary>
    /// Windows 10/11 에 내장된 OCR 엔진(Windows.Media.Ocr).
    /// 별도 모델 다운로드가 필요 없으며, 설치된 언어 인식팩을 사용한다.
    /// 한국어 인식팩이 있으면 한국어를 우선 사용한다.
    /// </summary>
    public sealed class WindowsOcrEngine : IOcrEngine
    {
        private readonly OcrEngine? _engine;
        private readonly string? _languageTag;

        public WindowsOcrEngine()
        {
            _engine = CreateEngine(out _languageTag);
        }

        public string DisplayName =>
            _engine != null && _languageTag != null
                ? $"Windows 내장 OCR ({_languageTag})"
                : "Windows 내장 OCR";

        public bool IsAvailable => _engine != null;

        public string? UnavailableReason =>
            IsAvailable
                ? null
                : "이 PC에 사용 가능한 Windows OCR 언어팩이 없습니다. " +
                  "설정 > 시간 및 언어 > 언어 및 지역 에서 언어 옵션의 '광학 문자 인식(OCR)' 기능을 추가하세요.";

        public async Task<OcrResult> RecognizeAsync(Bitmap image, CancellationToken cancellationToken = default)
        {
            if (_engine == null)
                throw new InvalidOperationException(UnavailableReason);

            var sw = Stopwatch.StartNew();

            // Windows OCR 은 최대 크기 제한이 있어 초과 시 비율 유지 축소.
            using Bitmap prepared = EnsureWithinMaxDimension(image, (int)OcrEngine.MaxImageDimension);

            using SoftwareBitmap softwareBitmap = await ToSoftwareBitmapAsync(prepared, cancellationToken)
                .ConfigureAwait(false);

            WinRtOcrResult raw = await _engine.RecognizeAsync(softwareBitmap).AsTask(cancellationToken)
                .ConfigureAwait(false);

            // 줄 구조를 보존해서 텍스트를 조립한다.
            var sb = new StringBuilder();
            foreach (OcrLine line in raw.Lines)
                sb.AppendLine(line.Text);

            sw.Stop();
            return new OcrResult(
                Text: sb.ToString().TrimEnd('\r', '\n'),
                EngineName: DisplayName,
                Elapsed: sw.Elapsed,
                Confidence: null); // Windows OCR 은 신뢰도 점수를 노출하지 않음
        }

        private static OcrEngine? CreateEngine(out string? languageTag)
        {
            // 1) 한국어 인식팩 우선
            var korean = new Language("ko");
            if (OcrEngine.IsLanguageSupported(korean))
            {
                var e = OcrEngine.TryCreateFromLanguage(korean);
                if (e != null)
                {
                    languageTag = korean.LanguageTag;
                    return e;
                }
            }

            // 2) 사용자 프로필 언어 기반
            var byProfile = OcrEngine.TryCreateFromUserProfileLanguages();
            if (byProfile != null)
            {
                languageTag = byProfile.RecognizerLanguage?.LanguageTag;
                return byProfile;
            }

            // 3) 사용 가능한 아무 언어
            foreach (Language lang in OcrEngine.AvailableRecognizerLanguages)
            {
                var e = OcrEngine.TryCreateFromLanguage(lang);
                if (e != null)
                {
                    languageTag = lang.LanguageTag;
                    return e;
                }
            }

            languageTag = null;
            return null;
        }

        /// <summary>System.Drawing.Bitmap 을 WinRT SoftwareBitmap(Bgra8) 으로 변환.</summary>
        private static async Task<SoftwareBitmap> ToSoftwareBitmapAsync(Bitmap bitmap, CancellationToken ct)
        {
            byte[] bytes;
            using (var ms = new MemoryStream())
            {
                bitmap.Save(ms, ImageFormat.Png);
                bytes = ms.ToArray();
            }

            using var stream = new InMemoryRandomAccessStream();
            using var writer = new DataWriter(stream);
            writer.WriteBytes(bytes);
            await writer.StoreAsync().AsTask(ct).ConfigureAwait(false);
            await writer.FlushAsync().AsTask(ct).ConfigureAwait(false);
            writer.DetachStream(); // 성공 경로: using 이 stream 을 닫지 않도록 먼저 분리
            stream.Seek(0);

            BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream).AsTask(ct).ConfigureAwait(false);
            SoftwareBitmap decoded = await decoder.GetSoftwareBitmapAsync().AsTask(ct).ConfigureAwait(false);

            // RecognizeAsync 는 Bgra8/Gray8 만 허용 → Bgra8 로 정규화.
            if (decoded.BitmapPixelFormat != BitmapPixelFormat.Bgra8 ||
                decoded.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
            {
                SoftwareBitmap converted = SoftwareBitmap.Convert(
                    decoded, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                decoded.Dispose();
                return converted;
            }

            return decoded;
        }

        /// <summary>최대 변 길이를 초과하면 비율을 유지하며 축소한 새 Bitmap 을 반환. 아니면 원본 복제.</summary>
        private static Bitmap EnsureWithinMaxDimension(Bitmap source, int maxDimension)
        {
            if (source.Width <= maxDimension && source.Height <= maxDimension)
                return new Bitmap(source);

            double scale = (double)maxDimension / Math.Max(source.Width, source.Height);
            int newWidth = Math.Max(1, (int)(source.Width * scale));
            int newHeight = Math.Max(1, (int)(source.Height * scale));

            var resized = new Bitmap(newWidth, newHeight);
            using var g = Graphics.FromImage(resized);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, newWidth, newHeight);
            return resized;
        }
    }
}
