using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using image_ocr.Ocr;

namespace image_ocr.Controls
{
    /// <summary>
    /// 이미지를 비율 맞춰 표시하고, 드래그로 영역을 선택하며, OCR 단어 좌표를 하이라이트하는 캔버스.
    /// 표시 중인 Bitmap 의 소유권은 갖지 않는다(부모 Form 이 관리).
    /// </summary>
    public sealed class ImageCanvas : Control
    {
        private Bitmap? _image;
        private RectangleF _displayRect;
        private float _scale = 1f;

        private bool _selecting;
        private Point _selStart;
        private Point _selEnd;
        private Rectangle? _selectionImageRect;

        private IReadOnlyList<OcrWordBox>? _highlights;

        public event EventHandler? SelectionChanged;

        public ImageCanvas()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.UserPaint | ControlStyles.ResizeRedraw, true);
            BackColor = Color.FromArgb(37, 37, 38);
        }

        /// <summary>표시할 이미지. 설정 시 선택/하이라이트가 초기화된다.</summary>
        [Browsable(false)]
        public Bitmap? Image
        {
            get => _image;
            set
            {
                _image = value;
                _selectionImageRect = null;
                _highlights = null;
                RecomputeLayout();
                Invalidate();
            }
        }

        /// <summary>드래그로 선택된 영역(이미지 픽셀 좌표). 없으면 null.</summary>
        [Browsable(false)]
        public Rectangle? SelectionImageRect => _selectionImageRect;

        /// <summary>OCR 단어 좌표(이미지 픽셀). 설정하면 박스로 그려진다.</summary>
        [Browsable(false)]
        public IReadOnlyList<OcrWordBox>? Highlights
        {
            get => _highlights;
            set { _highlights = value; Invalidate(); }
        }

        public void ClearSelection()
        {
            if (_selectionImageRect == null) return;
            _selectionImageRect = null;
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RecomputeLayout();
            Invalidate();
        }

        private void RecomputeLayout()
        {
            if (_image == null || ClientSize.Width <= 0 || ClientSize.Height <= 0)
            {
                _displayRect = ClientRectangle;
                _scale = 1f;
                return;
            }
            float sw = (float)ClientSize.Width / _image.Width;
            float sh = (float)ClientSize.Height / _image.Height;
            _scale = Math.Min(sw, sh);
            float w = _image.Width * _scale;
            float h = _image.Height * _scale;
            _displayRect = new RectangleF((ClientSize.Width - w) / 2f, (ClientSize.Height - h) / 2f, w, h);
        }

        private PointF ClientToImage(Point p) =>
            new((p.X - _displayRect.X) / _scale, (p.Y - _displayRect.Y) / _scale);

        private RectangleF ImageToClient(Rectangle r) =>
            new(_displayRect.X + r.X * _scale, _displayRect.Y + r.Y * _scale, r.Width * _scale, r.Height * _scale);

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button != MouseButtons.Left || _image == null) return;
            _selecting = true;
            _selStart = _selEnd = e.Location;
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (_selecting)
            {
                _selEnd = e.Location;
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (!_selecting || _image == null) return;
            _selecting = false;

            var client = RectFromPoints(_selStart, _selEnd);
            if (client.Width < 5 || client.Height < 5)
            {
                // 작은 드래그/클릭 → 선택 해제.
                _selectionImageRect = null;
            }
            else
            {
                _selectionImageRect = ClampToImage(client);
            }
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private Rectangle ClampToImage(Rectangle client)
        {
            PointF a = ClientToImage(new Point(client.Left, client.Top));
            PointF b = ClientToImage(new Point(client.Right, client.Bottom));
            int x1 = (int)Math.Round(Math.Max(0, Math.Min(a.X, b.X)));
            int y1 = (int)Math.Round(Math.Max(0, Math.Min(a.Y, b.Y)));
            int x2 = (int)Math.Round(Math.Min(_image!.Width, Math.Max(a.X, b.X)));
            int y2 = (int)Math.Round(Math.Min(_image!.Height, Math.Max(a.Y, b.Y)));
            return Rectangle.FromLTRB(x1, y1, Math.Max(x1 + 1, x2), Math.Max(y1 + 1, y2));
        }

        private static Rectangle RectFromPoints(Point a, Point b) =>
            Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            if (_image == null) return;

            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(_image, _displayRect);

            // 하이라이트(인식된 단어 박스)
            if (_highlights is { Count: > 0 })
            {
                using var pen = new Pen(Color.FromArgb(230, 80, 200, 120), 1.5f);
                using var fill = new SolidBrush(Color.FromArgb(48, 80, 220, 120));
                foreach (OcrWordBox w in _highlights)
                {
                    RectangleF r = ImageToClient(w.Bounds);
                    g.FillRectangle(fill, r);
                    g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
                }
            }

            // 확정된 선택 영역
            if (_selectionImageRect is { } sel)
                DrawSelection(g, ImageToClient(sel));

            // 드래그 중인 선택 영역
            if (_selecting)
                DrawSelection(g, RectFromPoints(_selStart, _selEnd));
        }

        private static void DrawSelection(Graphics g, RectangleF r)
        {
            using var fill = new SolidBrush(Color.FromArgb(40, 90, 150, 255));
            g.FillRectangle(fill, r);
            using var pen = new Pen(Color.FromArgb(230, 90, 150, 255), 1.6f) { DashStyle = DashStyle.Dash };
            g.DrawRectangle(pen, r.X, r.Y, r.Width, r.Height);
        }
    }
}
