using System.Drawing;

namespace image_ocr.Ocr
{
    /// <summary>
    /// OCR 엔진 공통 인터페이스. Windows 내장 OCR, Tesseract 등이 이를 구현한다.
    /// </summary>
    public interface IOcrEngine
    {
        /// <summary>UI 드롭다운에 표시할 엔진 이름 (예: "Windows 내장 OCR").</summary>
        string DisplayName { get; }

        /// <summary>이 엔진이 현재 PC에서 실제로 사용 가능한지(언어팩/네이티브 DLL 존재 등).</summary>
        bool IsAvailable { get; }

        /// <summary>사용 불가일 때 사용자에게 보여줄 안내 문구. 사용 가능하면 null.</summary>
        string? UnavailableReason { get; }

        /// <summary>
        /// 이미지에서 텍스트를 추출한다. UI를 막지 않도록 항상 백그라운드에서 호출한다.
        /// </summary>
        Task<OcrResult> RecognizeAsync(Bitmap image, CancellationToken cancellationToken = default);
    }

    /// <summary>OCR 실행 결과.</summary>
    public sealed record OcrResult(
        string Text,
        string EngineName,
        TimeSpan Elapsed,
        double? Confidence)
    {
        /// <summary>추출된 문자 수(공백 포함).</summary>
        public int CharCount => Text.Length;

        /// <summary>줄 수.</summary>
        public int LineCount =>
            string.IsNullOrEmpty(Text)
                ? 0
                : Text.Split('\n').Length;
    }
}
