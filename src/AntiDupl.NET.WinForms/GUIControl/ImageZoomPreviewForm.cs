using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace AntiDupl.NET.WinForms
{
    /// <summary>Displays a magnified crop of the image currently under the mouse.</summary>
    internal sealed class ImageZoomPreviewForm : Form
    {
        private const int WM_NCHITTEST = 0x0084;
        private static readonly IntPtr HTTRANSPARENT = new IntPtr(-1);

        private Bitmap m_image;
        private Rectangle m_sourceRectangle;

        public ImageZoomPreviewForm()
        {
            // A borderless, non-activating window leaves the full area for the image.
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            TopMost = true;
            StartPosition = FormStartPosition.Manual;
            ClientSize = new Size(480, 360);
            BackColor = Color.Black;
            Padding = new Padding(1);
            DoubleBuffered = true;
        }

        protected override bool ShowWithoutActivation { get { return true; } }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;
                parameters.ExStyle |= 0x08000000; // WS_EX_NOACTIVATE
                return parameters;
            }
        }

        /// <summary>
        /// Lets the window underneath receive hit testing, so the magnifier does not
        /// trigger MouseLeave on the PictureBoxPanel being magnified.
        /// </summary>
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_NCHITTEST)
            {
                m.Result = HTTRANSPARENT;
                return;
            }

            base.WndProc(ref m);
        }

        public void UpdatePreview(Bitmap image, Rectangle imageBounds, Point imagePoint,
            int zoomPercent, int previewWidth, int previewHeight, Point cursorScreenPosition)
        {
            if (image == null || image.Width <= 0 || image.Height <= 0 ||
                imageBounds.Width <= 0 || imageBounds.Height <= 0)
                return;

            if (zoomPercent != 150 && zoomPercent != 200 &&
                zoomPercent != 300 && zoomPercent != 400 &&
                zoomPercent != 600)
                zoomPercent = 200;

            Rectangle workingArea = Screen.FromPoint(cursorScreenPosition).WorkingArea;
            previewWidth = Math.Max(240, Math.Min(previewWidth, Math.Max(240, workingArea.Width - 16)));
            previewHeight = Math.Max(180, Math.Min(previewHeight, Math.Max(180, workingArea.Height - 16)));
            if (ClientSize.Width != previewWidth || ClientSize.Height != previewHeight)
                ClientSize = new Size(previewWidth, previewHeight);

            m_image = image;
            double scale = Math.Max(1.0, zoomPercent / 100.0);
            int viewWidth = Math.Max(1, ClientSize.Width - Padding.Horizontal);
            int viewHeight = Math.Max(1, ClientSize.Height - Padding.Vertical);
            int sourceWidth = Math.Max(1, (int)Math.Round(viewWidth * image.Width / (double)imageBounds.Width / scale));
            int sourceHeight = Math.Max(1, (int)Math.Round(viewHeight * image.Height / (double)imageBounds.Height / scale));
            sourceWidth = Math.Min(sourceWidth, image.Width);
            sourceHeight = Math.Min(sourceHeight, image.Height);

            int centerX = (int)Math.Round((imagePoint.X - imageBounds.Left) * image.Width / (double)imageBounds.Width);
            int centerY = (int)Math.Round((imagePoint.Y - imageBounds.Top) * image.Height / (double)imageBounds.Height);
            int left = Math.Max(0, Math.Min(image.Width - sourceWidth, centerX - sourceWidth / 2));
            int top = Math.Max(0, Math.Min(image.Height - sourceHeight, centerY - sourceHeight / 2));
            m_sourceRectangle = new Rectangle(left, top, sourceWidth, sourceHeight);

            PositionNearCursor(cursorScreenPosition);
            Invalidate();
            if (!Visible)
                Show();
        }

        public void ClearPreview()
        {
            m_image = null;
            m_sourceRectangle = Rectangle.Empty;
        }

        private void PositionNearCursor(Point cursor)
        {
            Rectangle area = Screen.FromPoint(cursor).WorkingArea;
            int x = cursor.X + 20;
            int y = cursor.Y + 20;
            if (x + Width > area.Right)
                x = cursor.X - Width - 20;
            if (y + Height > area.Bottom)
                y = cursor.Y - Height - 20;
            x = Math.Max(area.Left, Math.Min(area.Right - Width, x));
            y = Math.Max(area.Top, Math.Min(area.Bottom - Height, y));
            Location = new Point(x, y);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.Clear(Color.Black);
            if (m_image != null && m_sourceRectangle.Width > 0 && m_sourceRectangle.Height > 0)
            {
                Rectangle destination = new Rectangle(Padding.Left, Padding.Top,
                    ClientSize.Width - Padding.Horizontal, ClientSize.Height - Padding.Vertical);
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                e.Graphics.DrawImage(m_image, destination, m_sourceRectangle, GraphicsUnit.Pixel);
            }
            using (Pen border = new Pen(Color.White))
                e.Graphics.DrawRectangle(border, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }
    }
}
