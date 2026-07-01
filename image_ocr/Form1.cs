using System.Drawing.Imaging;
using System.IO;
using System.Text;
using DevExpress.XtraEditors;
using image_ocr.Ocr;
using image_ocr.Update;

namespace image_ocr
{
    public partial class Form1 : XtraForm
    {
        // OCR 엔진 목록. comboEngine 의 항목 순서와 1:1 대응한다.
        private readonly List<IOcrEngine> _engines = new();
        private TesseractOcrEngine? _tesseract; // 언어 설정용 참조

        // 현재 로드된 이미지(이 Form 이 소유·해제 책임을 진다).
        private Bitmap? _currentImage;

        // 마지막 인식 결과의 단어 좌표(원본 이미지 픽셀 기준) — 하이라이트용.
        private IReadOnlyList<OcrWordBox>? _lastWords;

        private bool _isRunning;
        private readonly AppSettings _settings;

        // 코드로 생성하는 인식 모드(PSM) 드롭다운(디자이너 재직렬화 이슈 회피).
        private LabelControl _labelPsm = null!;
        private ComboBoxEdit _comboPsm = null!;
        private static readonly (string label, OcrPageMode mode)[] PsmOptions =
        {
            ("단락 (권장)", OcrPageMode.Block),
            ("자동", OcrPageMode.Auto),
            ("한 줄", OcrPageMode.Line),
            ("성김 텍스트", OcrPageMode.Sparse),
        };

        private static readonly string[] SupportedExtensions =
            { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp" };

        public Form1()
        {
            InitializeComponent();
            SetupImageCanvas();
            SetupPsmControl();

            _settings = AppSettings.Load();

            TryLoadWindowIcon();
            InitializeEngines();
            InitializeLanguages();
            ApplySettings();
            WireEvents();
            UpdateStatus("준비됨 — 이미지를 열거나 드래그앤드롭 하세요. (이미지 위 드래그로 영역 선택)");
        }

        // ── 초기화 ─────────────────────────────────────────────────────────

        /// <summary>
        /// 이미지 캔버스를 코드로 생성해 Panel1 에 배치한다.
        /// (커스텀 컨트롤이라 WinForms 디자이너가 재직렬화 시 제거하는 문제를 피하기 위함)
        /// </summary>
        private void SetupImageCanvas()
        {
            imageCanvas = new Controls.ImageCanvas
            {
                Dock = DockStyle.Fill,
                Name = "imageCanvas",
                Visible = false,
            };
            splitContainer.Panel1.Controls.Add(imageCanvas);
            imageCanvas.BringToFront();
        }

        /// <summary>인식 모드(PSM) 드롭다운을 코드로 만들어 상단 패널에 배치한다.</summary>
        private void SetupPsmControl()
        {
            _labelPsm = new LabelControl { Text = "모드:", Location = new Point(590, 56), AutoSizeMode = LabelAutoSizeMode.None, Size = new Size(34, 14) };
            _comboPsm = new ComboBoxEdit { Location = new Point(626, 51), Size = new Size(150, 24), Name = "comboPsm" };
            _comboPsm.Properties.TextEditStyle = DevExpress.XtraEditors.Controls.TextEditStyles.DisableTextEditor;
            foreach (var (label, _) in PsmOptions)
                _comboPsm.Properties.Items.Add(label);
            _comboPsm.SelectedIndex = 0;
            _comboPsm.SelectedIndexChanged += (_, _) => ApplyPageMode();

            panelTop.Controls.Add(_labelPsm);
            panelTop.Controls.Add(_comboPsm);
        }

        private void ApplyPageMode()
        {
            int i = Math.Clamp(_comboPsm.SelectedIndex, 0, PsmOptions.Length - 1);
            _tesseract?.SetPageMode(PsmOptions[i].mode);
        }

        /// <summary>임베드된 app.ico(다중 해상도)를 창·작업표시줄 아이콘으로 설정한다.</summary>
        private void TryLoadWindowIcon()
        {
            try
            {
                // 1) 임베드 리소스에서 다중 해상도 아이콘을 그대로 로드(작업표시줄까지 선명).
                using Stream? s = typeof(Form1).Assembly.GetManifestResourceStream("app.ico");
                if (s != null)
                {
                    Icon = new Icon(s);
                    IconOptions.ShowIcon = true;
                    ShowIcon = true;
                    return;
                }

                // 2) 대체: 실행 파일에 박힌 아이콘 추출.
                Icon? exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (exeIcon != null)
                {
                    Icon = exeIcon;
                    IconOptions.ShowIcon = true;
                    ShowIcon = true;
                }
            }
            catch
            {
                // 아이콘 로딩 실패는 치명적이지 않으므로 무시.
            }
        }

        private void InitializeEngines()
        {
            _tesseract = new TesseractOcrEngine();
            _engines.Add(_tesseract);          // 기본
            _engines.Add(new WindowsOcrEngine());

            comboEngine.Properties.Items.Clear();
            foreach (IOcrEngine engine in _engines)
            {
                string label = engine.IsAvailable ? engine.DisplayName : $"{engine.DisplayName} (사용 불가)";
                comboEngine.Properties.Items.Add(label);
            }
            int defaultIndex = _engines.FindIndex(e => e.IsAvailable);
            comboEngine.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        }

        private void InitializeLanguages()
        {
            comboLang.Properties.Items.Clear();
            var langs = _tesseract?.AvailableLanguages ?? (IReadOnlyList<string>)Array.Empty<string>();

            var options = new List<string>();
            if (langs.Contains("kor") && langs.Contains("eng")) options.Add("kor+eng");
            options.AddRange(langs);

            foreach (string o in options)
                comboLang.Properties.Items.Add(o);

            if (comboLang.Properties.Items.Count > 0)
            {
                // 현재 Tesseract 선택 언어와 일치하는 항목 선택.
                string current = string.Join('+', _tesseract?.SelectedLanguages ?? Array.Empty<string>());
                int idx = options.IndexOf(current);
                comboLang.SelectedIndex = idx >= 0 ? idx : 0;
            }
        }

        private void ApplySettings()
        {
            if (_settings.EngineIndex >= 0 && _settings.EngineIndex < _engines.Count)
                comboEngine.SelectedIndex = _settings.EngineIndex;

            if (_settings.Languages.Length > 0 && _tesseract != null)
            {
                _tesseract.SetLanguages(_settings.Languages);
                string joined = string.Join('+', _tesseract.SelectedLanguages);
                for (int i = 0; i < comboLang.Properties.Items.Count; i++)
                    if ((comboLang.Properties.Items[i] as string) == joined) { comboLang.SelectedIndex = i; break; }
            }

            chkGray.Checked = _settings.Grayscale;
            chkContrast.Checked = _settings.EnhanceContrast;
            chkBinarize.Checked = _settings.Binarize;
            chkUpscale.Checked = _settings.Upscale2x;

            if (_settings.PageMode >= 0 && _settings.PageMode < PsmOptions.Length)
                _comboPsm.SelectedIndex = _settings.PageMode;
            ApplyPageMode();

            if (_settings.WindowWidth > 400 && _settings.WindowHeight > 300)
            {
                StartPosition = FormStartPosition.Manual;
                Size = new Size(_settings.WindowWidth, _settings.WindowHeight);
                CenterToScreen();
            }

            UpdateTesseractControlsEnabled();
        }

        private void UpdateTesseractControlsEnabled()
        {
            bool isTess = SelectedEngine is TesseractOcrEngine;
            comboLang.Enabled = isTess;
            _comboPsm.Enabled = isTess;
        }

        private void WireEvents()
        {
            btnOpen.Click += (_, _) => OpenImageViaDialog();
            btnPaste.Click += (_, _) => PasteFromClipboard();
            btnRun.Click += async (_, _) => await RunOcrAsync();
            btnCopy.Click += (_, _) => CopyText();
            btnSave.Click += (_, _) => SaveText();
            btnClear.Click += (_, _) => ClearAll();
            btnBatch.Click += async (_, _) => await BatchAsync();

            comboEngine.SelectedIndexChanged += (_, _) => UpdateTesseractControlsEnabled();
            comboLang.SelectedIndexChanged += (_, _) => ApplyLanguageSelection();
            chkHighlight.CheckedChanged += (_, _) =>
                imageCanvas.Highlights = chkHighlight.Checked ? _lastWords : null;

            imageCanvas.SelectionChanged += (_, _) => OnSelectionChanged();

            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;
            KeyDown += Form1_KeyDown;

            Shown += async (_, _) => await CheckForUpdatesAsync(silent: true);
        }

        private IOcrEngine SelectedEngine => _engines[Math.Max(0, comboEngine.SelectedIndex)];

        private void ApplyLanguageSelection()
        {
            if (_tesseract == null) return;
            if (comboLang.SelectedItem is string sel && sel.Length > 0)
                _tesseract.SetLanguages(sel.Split('+', StringSplitOptions.RemoveEmptyEntries));
        }

        private void OnSelectionChanged()
        {
            Rectangle? sel = imageCanvas.SelectionImageRect;
            if (sel is { } r)
                UpdateStatus($"영역 선택됨: {r.Width}×{r.Height} — 추출 시 이 영역만 인식합니다. (작게 클릭하면 해제)");
            else
                UpdateStatus("영역 선택 해제됨 — 전체 이미지를 인식합니다.");
        }

        // ── 자동 업데이트 ──────────────────────────────────────────────────

        private async Task CheckForUpdatesAsync(bool silent)
        {
            Updater.UpdateInfo? info = await Updater.CheckForUpdateAsync();
            if (IsDisposed) return;

            if (info == null)
            {
                if (!silent)
                    XtraMessageBox.Show(this, $"최신 버전을 사용 중입니다. (v{Updater.CurrentVersion.ToString(3)})",
                        "업데이트", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string notes = string.IsNullOrWhiteSpace(info.Notes) ? "" : $"\n\n{info.Notes}";
            DialogResult r = XtraMessageBox.Show(this,
                $"새 버전 {info.Tag} 이(가) 있습니다. 지금 업데이트할까요?{notes}",
                "업데이트 있음", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (r != DialogResult.Yes) return;

            try
            {
                SetBusy(true);
                var progress = new Progress<int>(p => UpdateStatus($"업데이트 다운로드 중... {p}%"));
                await Updater.DownloadAndApplyAsync(info, progress);
                UpdateStatus("업데이트 적용 준비 완료 — 프로그램을 재시작합니다.");
                Application.Exit();
            }
            catch (Exception ex)
            {
                if (!IsDisposed) { ShowError("업데이트에 실패했습니다.", ex); SetBusy(false); }
            }
        }

        // ── 이미지 입력 ────────────────────────────────────────────────────

        /// <summary>명령줄/연결 프로그램으로 전달된 이미지 경로를 시작 시 로드한다.</summary>
        public void LoadImageOnStartup(string path) => LoadImageFromFile(path);

        private void OpenImageViaDialog()
        {
            using var dialog = new OpenFileDialog
            {
                Title = "이미지 열기",
                Filter = "이미지 파일|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp|모든 파일|*.*",
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                LoadImageFromFile(dialog.FileName);
        }

        private void PasteFromClipboard()
        {
            try
            {
                if (Clipboard.ContainsImage())
                {
                    using Image? clip = Clipboard.GetImage();
                    if (clip != null) { SetImage(new Bitmap(clip), "클립보드 이미지"); return; }
                }
                if (Clipboard.ContainsFileDropList())
                {
                    foreach (string? file in Clipboard.GetFileDropList())
                        if (file != null && IsSupportedImage(file)) { LoadImageFromFile(file); return; }
                }
                UpdateStatus("클립보드에 이미지가 없습니다.");
            }
            catch (Exception ex) { ShowError("붙여넣기 실패", ex); }
        }

        private void Form1_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            bool hasImage =
                e.Data.GetDataPresent(DataFormats.Bitmap) ||
                (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                 e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Any(IsSupportedImage));
            e.Effect = hasImage ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Form1_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;
            try
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
                {
                    var images = files.Where(IsSupportedImage).ToList();
                    if (images.Count > 1)
                    {
                        UpdateStatus($"{images.Count}개 이미지가 감지됨 — [여러 이미지 일괄]로 처리하거나 첫 이미지를 엽니다.");
                    }
                    if (images.Count >= 1) { LoadImageFromFile(images[0]); return; }
                }
                if (e.Data.GetData(DataFormats.Bitmap) is Image dropped)
                    SetImage(new Bitmap(dropped), "드롭한 이미지");
            }
            catch (Exception ex) { ShowError("이미지 불러오기 실패", ex); }
        }

        private void LoadImageFromFile(string path)
        {
            try
            {
                byte[] bytes = File.ReadAllBytes(path);
                using var ms = new MemoryStream(bytes);
                using Image loaded = Image.FromStream(ms);
                SetImage(new Bitmap(loaded), Path.GetFileName(path));
            }
            catch (Exception ex) { ShowError($"이미지를 열 수 없습니다:\n{path}", ex); }
        }

        /// <summary>새 이미지를 소유권과 함께 설정하고 미리보기에 표시한다.</summary>
        private void SetImage(Bitmap image, string source)
        {
            imageCanvas.Image = null;
            _currentImage?.Dispose();

            _currentImage = image;
            _lastWords = null;
            imageCanvas.Image = _currentImage;
            imageCanvas.Highlights = null;
            imageCanvas.Visible = true;
            labelDropHint.Visible = false;

            memoText.Text = string.Empty;
            UpdateStatus($"이미지 로드됨: {source} ({image.Width}×{image.Height}). [텍스트 추출]을 누르세요.");
        }

        // ── OCR 실행 ───────────────────────────────────────────────────────

        private async Task RunOcrAsync()
        {
            if (_isRunning) return;
            if (_currentImage == null) { UpdateStatus("먼저 이미지를 불러오세요."); return; }

            IOcrEngine engine = SelectedEngine;
            if (!engine.IsAvailable)
            {
                XtraMessageBox.Show(this, engine.UnavailableReason ?? "선택한 엔진을 사용할 수 없습니다.",
                    "엔진 사용 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            Rectangle crop = imageCanvas.SelectionImageRect
                ?? new Rectangle(0, 0, _currentImage.Width, _currentImage.Height);
            PreprocessOptions pre = CurrentPreprocess();
            int scale = pre.Upscale2x ? 2 : 1;

            _isRunning = true;
            SetBusy(true);
            string scope = imageCanvas.SelectionImageRect != null ? "선택 영역" : "전체 이미지";
            UpdateStatus($"{engine.DisplayName} 로 {scope} 인식 중...");

            try
            {
                using Bitmap input = CropAndPreprocess(_currentImage, crop, pre);
                OcrResult result = await engine.RecognizeAsync(input);
                if (IsDisposed) return;

                memoText.Text = result.Text;
                _lastWords = MapWordsToImage(result.Words, crop, scale);
                imageCanvas.Highlights = chkHighlight.Checked ? _lastWords : null;

                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    UpdateStatus("인식된 텍스트가 없습니다. 전처리(흑백/대비/이진화)나 언어를 바꿔 보세요.");
                }
                else
                {
                    string conf = result.Confidence.HasValue ? $" · 신뢰도 {result.Confidence.Value * 100:0.0}%" : "";
                    UpdateStatus($"완료 · {result.EngineName} · {scope} · {result.Elapsed.TotalSeconds:0.00}초 · " +
                                 $"{result.CharCount}자 / {result.LineCount}줄 · 단어 {result.Words.Count}개{conf}");
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed) { ShowError("OCR 실행 중 오류가 발생했습니다.", ex); UpdateStatus("오류로 중단됨."); }
            }
            finally
            {
                _isRunning = false;
                if (!IsDisposed) SetBusy(false);
            }
        }

        private PreprocessOptions CurrentPreprocess() => new()
        {
            Grayscale = chkGray.Checked,
            EnhanceContrast = chkContrast.Checked,
            Binarize = chkBinarize.Checked,
            Upscale2x = chkUpscale.Checked,
        };

        /// <summary>선택 영역을 잘라 전처리한 새 Bitmap 을 반환(호출자 소유).</summary>
        private static Bitmap CropAndPreprocess(Bitmap src, Rectangle crop, PreprocessOptions o)
        {
            using Bitmap region = CropBitmap(src, crop);
            return ImagePreprocessor.Apply(region, o);
        }

        private static Bitmap CropBitmap(Bitmap src, Rectangle rect)
        {
            var bmp = new Bitmap(rect.Width, rect.Height, PixelFormat.Format32bppArgb);
            using var g = Graphics.FromImage(bmp);
            g.DrawImage(src, new Rectangle(0, 0, rect.Width, rect.Height), rect, GraphicsUnit.Pixel);
            return bmp;
        }

        /// <summary>OCR 입력(잘림·확대)의 단어 좌표를 원본 이미지 좌표로 되돌린다.</summary>
        private static List<OcrWordBox> MapWordsToImage(IReadOnlyList<OcrWordBox> words, Rectangle crop, int scale)
        {
            var list = new List<OcrWordBox>(words.Count);
            foreach (OcrWordBox w in words)
            {
                var b = w.Bounds;
                list.Add(new OcrWordBox(w.Text, new Rectangle(
                    crop.X + b.X / scale, crop.Y + b.Y / scale, b.Width / scale, b.Height / scale)));
            }
            return list;
        }

        // ── 일괄 처리 ──────────────────────────────────────────────────────

        private async Task BatchAsync()
        {
            if (_isRunning) return;
            IOcrEngine engine = SelectedEngine;
            if (!engine.IsAvailable)
            {
                XtraMessageBox.Show(this, engine.UnavailableReason ?? "선택한 엔진을 사용할 수 없습니다.",
                    "엔진 사용 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using var dlg = new OpenFileDialog
            {
                Title = "일괄 처리할 이미지 여러 개 선택",
                Multiselect = true,
                Filter = "이미지 파일|*.png;*.jpg;*.jpeg;*.bmp;*.gif;*.tif;*.tiff;*.webp|모든 파일|*.*",
            };
            if (dlg.ShowDialog(this) != DialogResult.OK) return;

            string[] files = dlg.FileNames;
            PreprocessOptions pre = CurrentPreprocess();

            _isRunning = true;
            SetBusy(true);
            int ok = 0, fail = 0;
            var summary = new StringBuilder();
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    string file = files[i];
                    UpdateStatus($"일괄 처리 {i + 1}/{files.Length}: {Path.GetFileName(file)}");
                    try
                    {
                        byte[] bytes = File.ReadAllBytes(file);
                        using var ms = new MemoryStream(bytes);
                        using Image loaded = Image.FromStream(ms);
                        using var bmp = new Bitmap(loaded);
                        using Bitmap input = ImagePreprocessor.Apply(bmp, pre);

                        OcrResult r = await engine.RecognizeAsync(input);
                        if (IsDisposed) return;

                        string outPath = Path.Combine(
                            Path.GetDirectoryName(file) ?? ".",
                            Path.GetFileNameWithoutExtension(file) + "_ocr.txt");
                        File.WriteAllText(outPath, r.Text, new UTF8Encoding(true));
                        summary.AppendLine($"✔ {Path.GetFileName(file)} → {Path.GetFileName(outPath)} ({r.CharCount}자)");
                        ok++;
                    }
                    catch (Exception ex)
                    {
                        summary.AppendLine($"�’ {Path.GetFileName(file)} — 실패: {ex.Message}");
                        fail++;
                    }
                }
            }
            finally
            {
                _isRunning = false;
                if (!IsDisposed) SetBusy(false);
            }

            if (IsDisposed) return;
            memoText.Text = summary.ToString();
            UpdateStatus($"일괄 처리 완료 — 성공 {ok} · 실패 {fail} (결과는 각 이미지 옆 *_ocr.txt 로 저장)");
            XtraMessageBox.Show(this, $"일괄 처리 완료\n성공: {ok}\n실패: {fail}\n\n결과는 각 이미지와 같은 폴더에 *_ocr.txt 로 저장했습니다.",
                "일괄 처리", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── 결과 처리 ──────────────────────────────────────────────────────

        private void CopyText()
        {
            if (string.IsNullOrEmpty(memoText.Text)) { UpdateStatus("복사할 텍스트가 없습니다."); return; }
            Clipboard.SetText(memoText.Text);
            UpdateStatus("텍스트를 클립보드에 복사했습니다.");
        }

        private void SaveText()
        {
            if (string.IsNullOrEmpty(memoText.Text)) { UpdateStatus("저장할 텍스트가 없습니다."); return; }
            using var dialog = new SaveFileDialog
            {
                Title = "텍스트 저장",
                Filter = "텍스트 파일 (*.txt)|*.txt|모든 파일|*.*",
                FileName = "ocr_result.txt",
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    File.WriteAllText(dialog.FileName, memoText.Text, new UTF8Encoding(true));
                    UpdateStatus($"저장 완료: {dialog.FileName}");
                }
                catch (Exception ex) { ShowError("파일 저장 실패", ex); }
            }
        }

        private void ClearAll()
        {
            imageCanvas.Image = null;
            _currentImage?.Dispose();
            _currentImage = null;
            _lastWords = null;
            imageCanvas.Highlights = null;
            imageCanvas.Visible = false;
            labelDropHint.Visible = true;
            memoText.Text = string.Empty;
            UpdateStatus("초기화됨.");
        }

        // ── 유틸 ───────────────────────────────────────────────────────────

        private async void Form1_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.O) { OpenImageViaDialog(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.V) { PasteFromClipboard(); e.Handled = true; }
            else if (e.Control && e.KeyCode == Keys.S) { SaveText(); e.Handled = true; }
            else if (e.KeyCode == Keys.F5) { e.Handled = true; await RunOcrAsync(); }
        }

        private static bool IsSupportedImage(string path) =>
            SupportedExtensions.Contains(Path.GetExtension(path).ToLowerInvariant());

        private void SetBusy(bool busy)
        {
            Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
            btnRun.Enabled = !busy;
            btnOpen.Enabled = !busy;
            btnPaste.Enabled = !busy;
            btnClear.Enabled = !busy;
            btnBatch.Enabled = !busy;
            comboEngine.Enabled = !busy;
            bool isTess = SelectedEngine is TesseractOcrEngine;
            comboLang.Enabled = !busy && isTess;
            _comboPsm.Enabled = !busy && isTess;
        }

        private void UpdateStatus(string text) => labelStatus.Text = text;

        private void ShowError(string message, Exception ex) =>
            XtraMessageBox.Show(this, $"{message}\n\n{ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            SaveSettings();

            imageCanvas.Image = null;
            _currentImage?.Dispose();
            _currentImage = null;

            foreach (IOcrEngine engine in _engines)
                (engine as IDisposable)?.Dispose();

            base.OnFormClosed(e);
        }

        private void SaveSettings()
        {
            try
            {
                _settings.EngineIndex = comboEngine.SelectedIndex;
                _settings.Languages = _tesseract?.SelectedLanguages.ToArray() ?? Array.Empty<string>();
                _settings.PageMode = _comboPsm.SelectedIndex;
                _settings.Grayscale = chkGray.Checked;
                _settings.EnhanceContrast = chkContrast.Checked;
                _settings.Binarize = chkBinarize.Checked;
                _settings.Upscale2x = chkUpscale.Checked;
                _settings.WindowWidth = Width;
                _settings.WindowHeight = Height;
                _settings.Save();
            }
            catch { /* 설정 저장 실패는 무시 */ }
        }
    }
}
