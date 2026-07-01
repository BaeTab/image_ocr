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

        // 현재 로드된 이미지(이 Form 이 소유·해제 책임을 진다).
        private Bitmap? _currentImage;

        // OCR 실행 중복 방지.
        private bool _isRunning;

        private static readonly string[] SupportedExtensions =
            { ".png", ".jpg", ".jpeg", ".bmp", ".gif", ".tif", ".tiff", ".webp" };

        public Form1()
        {
            InitializeComponent();

            TryLoadWindowIcon();
            InitializeEngines();
            WireEvents();
            UpdateStatus("준비됨 — 이미지를 열거나 드래그앤드롭 하세요.");
        }

        /// <summary>실행 파일에 내장된 아이콘을 창/작업표시줄에 표시한다.</summary>
        private void TryLoadWindowIcon()
        {
            try
            {
                Icon? exeIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
                if (exeIcon != null)
                {
                    Icon = exeIcon;
                    IconOptions.ShowIcon = true;
                }
            }
            catch
            {
                // 아이콘 로딩 실패는 치명적이지 않으므로 무시.
            }
        }

        // ── 초기화 ─────────────────────────────────────────────────────────

        private void InitializeEngines()
        {
            // Tesseract 를 기본 엔진으로 먼저 등록(Windows 내장 OCR 은 정확도가 낮아 옵션으로만 유지).
            _engines.Add(new TesseractOcrEngine());
            _engines.Add(new WindowsOcrEngine());

            comboEngine.Properties.Items.Clear();
            foreach (IOcrEngine engine in _engines)
            {
                string label = engine.IsAvailable
                    ? engine.DisplayName
                    : $"{engine.DisplayName} (사용 불가)";
                comboEngine.Properties.Items.Add(label);
            }

            // 기본 선택: 사용 가능한 첫 엔진(= Tesseract). 없으면 0번.
            int defaultIndex = _engines.FindIndex(e => e.IsAvailable);
            comboEngine.SelectedIndex = defaultIndex >= 0 ? defaultIndex : 0;
        }

        private void WireEvents()
        {
            btnOpen.Click += (_, _) => OpenImageViaDialog();
            btnPaste.Click += (_, _) => PasteFromClipboard();
            btnRun.Click += async (_, _) => await RunOcrAsync();
            btnCopy.Click += (_, _) => CopyText();
            btnSave.Click += (_, _) => SaveText();
            btnClear.Click += (_, _) => ClearAll();

            // 드래그앤드롭: 폼 전체를 드롭 대상으로.
            DragEnter += Form1_DragEnter;
            DragDrop += Form1_DragDrop;

            // 키보드 단축키 (KeyPreview = true).
            KeyDown += Form1_KeyDown;

            // 시작 시 릴리즈 기반 업데이트 자동 확인(조용히).
            Shown += async (_, _) => await CheckForUpdatesAsync(silent: true);
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
                if (!IsDisposed)
                {
                    ShowError("업데이트에 실패했습니다.", ex);
                    SetBusy(false);
                }
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
                    if (clip != null)
                    {
                        SetImage(new Bitmap(clip), "클립보드 이미지");
                        return;
                    }
                }

                if (Clipboard.ContainsFileDropList())
                {
                    foreach (string? file in Clipboard.GetFileDropList())
                    {
                        if (file != null && IsSupportedImage(file))
                        {
                            LoadImageFromFile(file);
                            return;
                        }
                    }
                }

                UpdateStatus("클립보드에 이미지가 없습니다.");
            }
            catch (Exception ex)
            {
                ShowError("붙여넣기 실패", ex);
            }
        }

        private void Form1_DragEnter(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            bool hasImage =
                e.Data.GetDataPresent(DataFormats.Bitmap) ||
                (e.Data.GetDataPresent(DataFormats.FileDrop) &&
                 e.Data.GetData(DataFormats.FileDrop) is string[] files &&
                 files.Any(IsSupportedImage));

            e.Effect = hasImage ? DragDropEffects.Copy : DragDropEffects.None;
        }

        private void Form1_DragDrop(object? sender, DragEventArgs e)
        {
            if (e.Data == null) return;

            try
            {
                if (e.Data.GetData(DataFormats.FileDrop) is string[] files)
                {
                    string? image = files.FirstOrDefault(IsSupportedImage);
                    if (image != null)
                    {
                        LoadImageFromFile(image);
                        return;
                    }
                }

                if (e.Data.GetData(DataFormats.Bitmap) is Image dropped)
                    SetImage(new Bitmap(dropped), "드롭한 이미지");
            }
            catch (Exception ex)
            {
                ShowError("이미지 불러오기 실패", ex);
            }
        }

        private void LoadImageFromFile(string path)
        {
            try
            {
                // 파일을 잠그지 않도록 바이트로 읽어 메모리에서 디코딩한다.
                byte[] bytes = File.ReadAllBytes(path);
                using var ms = new MemoryStream(bytes);
                using Image loaded = Image.FromStream(ms);
                SetImage(new Bitmap(loaded), Path.GetFileName(path));
            }
            catch (Exception ex)
            {
                ShowError($"이미지를 열 수 없습니다:\n{path}", ex);
            }
        }

        /// <summary>새 이미지를 소유권과 함께 설정하고 미리보기에 표시한다.</summary>
        private void SetImage(Bitmap image, string source)
        {
            pictureEdit.Image = null;
            _currentImage?.Dispose();

            _currentImage = image;
            pictureEdit.Image = _currentImage;
            pictureEdit.Visible = true;
            labelDropHint.Visible = false;

            memoText.Text = string.Empty;
            UpdateStatus($"이미지 로드됨: {source} ({image.Width}×{image.Height}). [텍스트 추출]을 누르세요.");
        }

        // ── OCR 실행 ───────────────────────────────────────────────────────

        private async Task RunOcrAsync()
        {
            if (_isRunning) return;

            if (_currentImage == null)
            {
                UpdateStatus("먼저 이미지를 불러오세요.");
                return;
            }

            IOcrEngine engine = _engines[Math.Max(0, comboEngine.SelectedIndex)];
            if (!engine.IsAvailable)
            {
                XtraMessageBox.Show(this, engine.UnavailableReason ?? "선택한 엔진을 사용할 수 없습니다.",
                    "엔진 사용 불가", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isRunning = true;
            SetBusy(true);
            UpdateStatus($"{engine.DisplayName} 로 인식 중...");

            try
            {
                // 백그라운드 스레드가 안전하게 읽도록 복제본을 넘긴다.
                using var clone = new Bitmap(_currentImage);
                OcrResult result = await engine.RecognizeAsync(clone);

                // await 도중 폼이 닫혔으면 파괴된 컨트롤 접근을 피한다.
                if (IsDisposed) return;

                memoText.Text = result.Text;

                if (string.IsNullOrWhiteSpace(result.Text))
                {
                    UpdateStatus("인식된 텍스트가 없습니다. 이미지 품질이나 언어를 확인하세요.");
                }
                else
                {
                    string confidence = result.Confidence.HasValue
                        ? $" · 신뢰도 {result.Confidence.Value * 100:0.0}%"
                        : string.Empty;
                    UpdateStatus(
                        $"완료 · {result.EngineName} · {result.Elapsed.TotalSeconds:0.00}초 · " +
                        $"{result.CharCount}자 / {result.LineCount}줄{confidence}");
                }
            }
            catch (Exception ex)
            {
                if (!IsDisposed)
                {
                    ShowError("OCR 실행 중 오류가 발생했습니다.", ex);
                    UpdateStatus("오류로 중단됨.");
                }
            }
            finally
            {
                _isRunning = false;
                if (!IsDisposed)
                    SetBusy(false);
            }
        }

        // ── 결과 처리 ──────────────────────────────────────────────────────

        private void CopyText()
        {
            if (string.IsNullOrEmpty(memoText.Text))
            {
                UpdateStatus("복사할 텍스트가 없습니다.");
                return;
            }

            Clipboard.SetText(memoText.Text);
            UpdateStatus("텍스트를 클립보드에 복사했습니다.");
        }

        private void SaveText()
        {
            if (string.IsNullOrEmpty(memoText.Text))
            {
                UpdateStatus("저장할 텍스트가 없습니다.");
                return;
            }

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
                catch (Exception ex)
                {
                    ShowError("파일 저장 실패", ex);
                }
            }
        }

        private void ClearAll()
        {
            pictureEdit.Image = null;
            _currentImage?.Dispose();
            _currentImage = null;
            pictureEdit.Visible = false;
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
            comboEngine.Enabled = !busy;
        }

        private void UpdateStatus(string text) => labelStatus.Text = text;

        private void ShowError(string message, Exception ex) =>
            XtraMessageBox.Show(this, $"{message}\n\n{ex.Message}", "오류",
                MessageBoxButtons.OK, MessageBoxIcon.Error);

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            pictureEdit.Image = null;
            _currentImage?.Dispose();
            _currentImage = null;

            foreach (IOcrEngine engine in _engines)
                (engine as IDisposable)?.Dispose();

            base.OnFormClosed(e);
        }
    }
}
