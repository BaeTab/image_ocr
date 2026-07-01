using System.IO;
using System.Reflection;

namespace image_ocr.Ocr
{
    /// <summary>
    /// 단일 exe(단일파일 게시) 환경에서 Tesseract 가 필요로 하는 네이티브 DLL 과
    /// tessdata 를 준비한다.
    /// - 일반 빌드: 실행 파일 옆에 tessdata\ 와 x64\ 가 그대로 있으므로 그것을 사용.
    /// - 단일파일: exe 안에 임베드된 리소스를 사용자 캐시 폴더로 추출하고,
    ///   Tesseract 네이티브 탐색 경로(CustomSearchPath)를 그 폴더로 지정한다.
    /// </summary>
    public static class RuntimeAssets
    {
        private static readonly object Gate = new();
        private static string? _tessdataPath;

        /// <summary>tessdata 폴더의 실제 경로를 반환한다(필요 시 추출·경로설정 수행).</summary>
        public static string EnsureTessdata()
        {
            if (_tessdataPath != null) return _tessdataPath;

            lock (Gate)
            {
                if (_tessdataPath != null) return _tessdataPath;

                string baseDir = AppContext.BaseDirectory;
                string looseTess = Path.Combine(baseDir, "tessdata");
                bool looseNative = File.Exists(Path.Combine(baseDir, "x64", "tesseract50.dll"));

                // 1) 실행 파일 옆에 파일이 그대로 있으면 그대로 사용(추출 불필요).
                if (looseNative &&
                    File.Exists(Path.Combine(looseTess, "kor.traineddata")))
                {
                    _tessdataPath = looseTess;
                    return _tessdataPath;
                }

                // 2) 단일파일 등: 임베드 리소스를 사용자 캐시로 추출.
                Assembly asm = typeof(RuntimeAssets).Assembly;
                string version = asm.GetName().Version?.ToString() ?? "1";
                string cache = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ImageOcr", "runtime-" + version);
                string tessDir = Path.Combine(cache, "tessdata");
                string nativeDir = Path.Combine(cache, "x64");
                Directory.CreateDirectory(tessDir);
                Directory.CreateDirectory(nativeDir);

                foreach (string res in asm.GetManifestResourceNames())
                {
                    if (res.StartsWith("tessdata/", StringComparison.Ordinal))
                    {
                        Extract(asm, res, Path.Combine(tessDir, res["tessdata/".Length..]));
                    }
                    else if (res.StartsWith("runtime/", StringComparison.Ordinal))
                    {
                        string file = res["runtime/".Length..];
                        Extract(asm, res, Path.Combine(nativeDir, file)); // {cache}\x64\
                        Extract(asm, res, Path.Combine(cache, file));      // {cache}\ (로더 변형 대비)
                    }
                }

                // 네이티브 DLL 을 여기서 찾도록 Tesseract 에 알려준다(엔진 생성 전 필수).
                Tesseract.TesseractEnviornment.CustomSearchPath = cache;

                _tessdataPath = tessDir;
                return _tessdataPath;
            }
        }

        private static void Extract(Assembly asm, string resourceName, string destPath)
        {
            using Stream? src = asm.GetManifestResourceStream(resourceName);
            if (src == null) return;

            // 이미 같은 크기로 추출돼 있으면 건너뛴다.
            if (File.Exists(destPath) && new FileInfo(destPath).Length == src.Length)
                return;

            using FileStream dst = File.Create(destPath);
            src.CopyTo(dst);
        }
    }
}
