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
        private RectangleF m_destinationRectangle;

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

            if (zoomPercent != 100 && zoomPercent != 150 && zoomPercent != 200 &&
                zoomPercent != 300 && zoomPercent != 400 && zoomPercent != 600 &&
                zoomPercent != 800)
                zoomPercent = 200;

            Rectangle workingArea = Screen.FromPoint(cursorScreenPosition).WorkingArea;
            previewWidth = Math.Max(240, Math.Min(previewWidth, Math.Max(240, workingArea.Width - 16)));
            previewHeight = Math.Max(180, Math.Min(previewHeight, Math.Max(180, workingArea.Height - 16)));
            if (ClientSize.Width != previewWidth || ClientSize.Height != previewHeight)
                ClientSize = new Size(previewWidth, previewHeight);

            Rectangle view = new Rectangle(Padding.Left, Padding.Top,
                Math.Max(1, ClientSize.Width - Padding.Horizontal),
                Math.Max(1, ClientSize.Height - Padding.Vertical));

            // Derive the scale of the source image in the main preview. Use one
            // uniform scale so the floating preview always preserves the image's
            // natural aspect ratio, even when the main panel uses non-proportional fit.
            double scaleX = imageBounds.Width / (double)image.Width;
            double scaleY = imageBounds.Height / (double)image.Height;
            double displayedPixelScale = Math.Min(scaleX, scaleY);
            if (displayedPixelScale <= 0.0 || double.IsNaN(displayedPixelScale) ||
                double.IsInfinity(displayedPixelScale))
                return;

            double requestedScale = displayedPixelScale * zoomPercent / 100.0;
            if (requestedScale <= 0.0 || double.IsNaN(requestedScale) ||
                double.IsInfinity(requestedScale))
                return;

            // The preview viewport is only a canvas. Fit the crop to it without
            // stretching: if the bitmap is too small to fill the canvas at the
            // selected magnification, show the whole available image with black
            // margins instead of silently increasing the effective zoom.
            double cropFraction = Math.Min(1.0,
                Math.Min(view.Width / (requestedScale * image.Width),
                         view.Height / (requestedScale * image.Height)));
            int sourceWidth = Math.Max(1, Math.Min(image.Width,
                (int)Math.Round(image.Width * cropFraction)));
            int sourceHeight = Math.Max(1, Math.Min(image.Height,
                (int)Math.Round(image.Height * cropFraction)));

            int centerX = (int)Math.Round((imagePoint.X - imageBounds.Left) *
                image.Width / (double)imageBounds.Width);
            int centerY = (int)Math.Round((imagePoint.Y - imageBounds.Top) *
                image.Height / (double)imageBounds.Height);
            centerX = Math.Max(0, Math.Min(image.Width - 1, centerX));
            centerY = Math.Max(0, Math.Min(image.Height - 1, centerY));

            int left = Math.Max(0, Math.Min(image.Width - sourceWidth,
                centerX - sourceWidth / 2));
            int top = Math.Max(0, Math.Min(image.Height - sourceHeight,
                centerY - sourceHeight / 2));
            m_sourceRectangle = new Rectangle(left, top, sourceWidth, sourceHeight);

            float drawWidth = (float)(sourceWidth * requestedScale);
            float drawHeight = (float)(sourceHeight * requestedScale);
            // Guard against sub-pixel rounding pushing the image beyond the canvas.
            drawWidth = Math.Min(drawWidth, view.Width);
            drawHeight = Math.Min(drawHeight, view.Height);
            float drawX = view.Left + (view.Width - drawWidth) / 2.0F;
            float drawY = view.Top + (view.Height - drawHeight) / 2.0F;
            m_destinationRectangle = new RectangleF(drawX, drawY, drawWidth, drawHeight);
            m_image = image;

            PositionNearCursor(cursorScreenPosition);
            Invalidate();
            if (!Visible)
                Show();
        }

        public void ClearPreview()
        {
            m_image = null;
            m_sourceRectangle = Rectangle.Empty;
            m_destinationRectangle = RectangleF.Empty;
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
            if (m_image != null && m_sourceRectangle.Width > 0 &&
                m_sourceRectangle.Height > 0 && m_destinationRectangle.Width > 0.0F &&
                m_destinationRectangle.Height > 0.0F)
            {
                Rectangle destination = Rectangle.Round(m_destinationRectangle);
                if (destination.Width > 0 && destination.Height > 0)
                {
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    e.Graphics.DrawImage(m_image, destination,
                        m_sourceRectangle, GraphicsUnit.Pixel);
                }
            }
            using (Pen border = new Pen(Color.White))
                e.Graphics.DrawRectangle(border, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
        }
    }
}
