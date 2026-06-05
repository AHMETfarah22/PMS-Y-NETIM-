using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace PmsSystem.Components
{
    public class RoundedPanel : Panel
    {
        public int BorderRadius { get; set; } = 15;
        public Color BorderColor { get; set; } = Color.Transparent;
        public int BorderSize { get; set; } = 0;

        public RoundedPanel()
        {
            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            // Arka planı temizle
            if (this.Parent != null)
            {
                using (var brush = new SolidBrush(this.Parent.BackColor))
                {
                    e.Graphics.FillRectangle(brush, this.ClientRectangle);
                }
            }

            Rectangle rect = new Rectangle(0, 0, this.Width - 1, this.Height - 1);
            
            // --- LUXURY SHADOW EFFECT ---
            using (var shadowPath = GetRoundedPath(new Rectangle(2, 2, this.Width - 3, this.Height - 3), BorderRadius)) {
                using (var shadowBrush = new SolidBrush(Color.FromArgb(12, Color.Black))) {
                    e.Graphics.FillPath(shadowBrush, shadowPath);
                }
            }

            using (var path = GetRoundedPath(rect, BorderRadius))
            {
                using (var brush = new SolidBrush(this.BackColor))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // Default subtle border if none specified
                if (BorderSize == 0) {
                    using (var pen = new Pen(Color.FromArgb(226, 232, 240), 1)) {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
                else if (BorderSize > 0 && BorderColor != Color.Transparent)
                {
                    using (var pen = new Pen(BorderColor, BorderSize))
                    {
                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        private GraphicsPath GetRoundedPath(Rectangle rect, int radius)
        {
            var path = new GraphicsPath();
            if (radius <= 0)
            {
                path.AddRectangle(rect);
                return path;
            }

            int diam = radius * 2;
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, diam, diam, 180, 90);
            path.AddArc(rect.Right - diam, rect.Y, diam, diam, 270, 90);
            path.AddArc(rect.Right - diam, rect.Bottom - diam, diam, diam, 0, 90);
            path.AddArc(rect.X, rect.Bottom - diam, diam, diam, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}
