using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace image_ocr.Ocr
{
    /// <summary>OCR 정확도를 높이기 위한 이미지 전처리 옵션.</summary>
    public sealed class PreprocessOptions
    {
        public bool Grayscale { get; set; }
        public bool EnhanceContrast { get; set; }
        public bool Binarize { get; set; }   // 이진화(흑/백) — 흑백을 포함
        public bool Upscale2x { get; set; }

        public bool Any => Grayscale || EnhanceContrast || Binarize || Upscale2x;
    }

    /// <summary>전처리 파이프라인: 확대 → 흑백 → 대비 → 이진화 순으로 적용한다.</summary>
    public static class ImagePreprocessor
    {
        /// <summary>옵션에 따라 전처리한 새 Bitmap 을 반환(호출자가 소유·해제). 옵션이 없으면 원본 복제.</summary>
        public static Bitmap Apply(Bitmap source, PreprocessOptions o)
        {
            if (!o.Any) return new Bitmap(source);

            Bitmap current = o.Upscale2x ? Upscale(source, 2) : new Bitmap(source);

            if (o.Grayscale || o.EnhanceContrast || o.Binarize)
            {
                Bitmap adjusted = ApplyColorMatrix(current,
                    grayscale: o.Grayscale || o.Binarize,
                    contrast: o.EnhanceContrast ? 1.4f : 1.0f);
                current.Dispose();
                current = adjusted;
            }

            if (o.Binarize)
            {
                Bitmap bw = Threshold(current);
                current.Dispose();
                current = bw;
            }

            return current;
        }

        private static Bitmap Upscale(Bitmap src, int factor)
        {
            var dst = new Bitmap(src.Width * factor, src.Height * factor, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(dst);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(src, new Rectangle(0, 0, dst.Width, dst.Height));
            return dst;
        }

        private static Bitmap ApplyColorMatrix(Bitmap src, bool grayscale, float contrast)
        {
            ColorMatrix cm;
            if (grayscale)
            {
                // 휘도 기반 흑백 + 대비.
                float c = contrast;
                float t = (1f - c) / 2f; // 대비 중심 0.5 유지
                cm = new ColorMatrix(new[]
                {
                    new[] { 0.299f * c, 0.299f * c, 0.299f * c, 0f, 0f },
                    new[] { 0.587f * c, 0.587f * c, 0.587f * c, 0f, 0f },
                    new[] { 0.114f * c, 0.114f * c, 0.114f * c, 0f, 0f },
                    new[] { 0f, 0f, 0f, 1f, 0f },
                    new[] { t, t, t, 0f, 1f },
                });
            }
            else
            {
                float c = contrast;
                float t = (1f - c) / 2f;
                cm = new ColorMatrix(new[]
                {
                    new[] { c, 0f, 0f, 0f, 0f },
                    new[] { 0f, c, 0f, 0f, 0f },
                    new[] { 0f, 0f, c, 0f, 0f },
                    new[] { 0f, 0f, 0f, 1f, 0f },
                    new[] { t, t, t, 0f, 1f },
                });
            }

            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(dst);
            using var attrs = new ImageAttributes();
            attrs.SetColorMatrix(cm);
            g.DrawImage(src, new Rectangle(0, 0, src.Width, src.Height),
                0, 0, src.Width, src.Height, GraphicsUnit.Pixel, attrs);
            return dst;
        }

        /// <summary>평균 휘도를 임계값으로 흑/백 이진화.</summary>
        private static unsafe Bitmap Threshold(Bitmap src)
        {
            var dst = new Bitmap(src.Width, src.Height, PixelFormat.Format32bppArgb);
            var rect = new Rectangle(0, 0, src.Width, src.Height);
            BitmapData sd = src.LockBits(rect, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData dd = dst.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
            try
            {
                // 1차: 평균 휘도 계산.
                long sum = 0;
                long count = (long)src.Width * src.Height;
                for (int y = 0; y < src.Height; y++)
                {
                    byte* row = (byte*)sd.Scan0 + y * sd.Stride;
                    for (int x = 0; x < src.Width; x++)
                    {
                        byte* p = row + x * 4;
                        sum += (p[2] * 299 + p[1] * 587 + p[0] * 114) / 1000;
                    }
                }
                byte threshold = (byte)(sum / Math.Max(1, count));

                // 2차: 임계값 적용.
                for (int y = 0; y < src.Height; y++)
                {
                    byte* srow = (byte*)sd.Scan0 + y * sd.Stride;
                    byte* drow = (byte*)dd.Scan0 + y * dd.Stride;
                    for (int x = 0; x < src.Width; x++)
                    {
                        byte* sp = srow + x * 4;
                        byte* dp = drow + x * 4;
                        int lum = (sp[2] * 299 + sp[1] * 587 + sp[0] * 114) / 1000;
                        byte v = (byte)(lum >= threshold ? 255 : 0);
                        dp[0] = dp[1] = dp[2] = v;
                        dp[3] = 255;
                    }
                }
            }
            finally
            {
                src.UnlockBits(sd);
                dst.UnlockBits(dd);
            }
            return dst;
        }
    }
}
