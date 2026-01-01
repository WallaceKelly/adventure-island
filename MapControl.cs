using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

using PixelPoint = System.Drawing.Point;
using AreaPoint = System.Drawing.Point;

namespace AdventureIsland
{
    public partial class MapControl : Control
    {
        float numPixelsPerArea = 50;
        public float NumPixelsPerArea
        {
            get { return numPixelsPerArea; }
            set
            {
                if (value > 20 && value < 400)
                {
                    numPixelsPerArea = value;
                    Invalidate();
                }
            }
        }

        float numPixelsInMargin = 3;

        public MapControl()
        {
            InitializeComponent();
            BackColor = Color.Black;
            DoubleBuffered = true;
        }

        WorldInfo worldInfo = null;
        public WorldInfo WorldInfo
        {
            get { return worldInfo; }
            set { worldInfo = value; }
        }

        UserInfo userInfo = null;
        public UserInfo UserInfo
        {
            get { return userInfo; }
            set
            {
                if (value != null && userInfo != value)
                {
                    if(userInfo != null)
                        userInfo.Move -= OnMove;
                    userInfo = value;
                    userInfo.Move += OnMove;
                }
            }
        }

        public void OnMove(AreaInfo areaInfo)
        {
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            Invalidate();
            base.OnSizeChanged(e);
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Brush b = new SolidBrush(Color.LightGreen);
            if (worldInfo == null || DesignMode)
            {
                pe.Graphics.FillRectangle(b, 5, 5, Width - 10, Height - 10);
            }
            else
            {
                DrawAreas(pe.Graphics, userInfo.CurrentArea);
                HighlightCurrentArea(pe.Graphics);
            }

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        protected virtual void DrawAreas(Graphics g, AreaInfo startingArea)
        {
            List<AreaInfo> drawnAreas = new List<AreaInfo>();
            DrawAreaAndSurrounding(g, userInfo.CurrentArea, new AreaPoint(0, 0), drawnAreas);
        }

        void DrawAreaAndSurrounding(Graphics g, AreaInfo areaInfo, AreaPoint areaPoint, List<AreaInfo> drawnAreas)
        {
            if (areaInfo != null && !drawnAreas.Contains(areaInfo))
            {
                drawnAreas.Add(areaInfo);

                PixelPoint pixelPoint = CalculatePixelPoint(areaPoint);
                DrawArea(g, areaInfo, pixelPoint);

                // north
                AreaPoint p = new AreaPoint(areaPoint.X, areaPoint.Y + 1);
                DrawAreaAndSurrounding(g, worldInfo.AreaManager.GetArea(areaInfo, AreaDirection.N), p, drawnAreas);
                // south
                p = new AreaPoint(areaPoint.X, areaPoint.Y - 1);
                DrawAreaAndSurrounding(g, worldInfo.AreaManager.GetArea(areaInfo, AreaDirection.S), p, drawnAreas);
                // east
                p = new AreaPoint(areaPoint.X + 1, areaPoint.Y);
                DrawAreaAndSurrounding(g, worldInfo.AreaManager.GetArea(areaInfo, AreaDirection.E), p, drawnAreas);
                // west
                p = new AreaPoint(areaPoint.X - 1, areaPoint.Y);
                DrawAreaAndSurrounding(g, worldInfo.AreaManager.GetArea(areaInfo, AreaDirection.W), p, drawnAreas);
            }
        }

        protected virtual void DrawArea(Graphics g, AreaInfo areaInfo, PixelPoint pixelPoint)
        {
            Brush b = userInfo.VisitedAreas.Contains(areaInfo.Name) ?
                new SolidBrush(Color.LightGreen) :
                new SolidBrush(Color.Gray);
            g.FillRectangle(b,
                pixelPoint.X + numPixelsInMargin,
                pixelPoint.Y + numPixelsInMargin,
                numPixelsPerArea - 2 * numPixelsInMargin,
                numPixelsPerArea - 2 * numPixelsInMargin);
            if (userInfo.VisitedAreas.Contains(areaInfo.Name))
            {
                Region prevClip = g.Clip;
                Font font = new Font("Arial Narrow", 8);
                Brush brush = new SolidBrush(Color.Black);
                g.Clip = new Region(new RectangleF(
                    pixelPoint.X + numPixelsInMargin + 1,
                    pixelPoint.Y + numPixelsInMargin + 1,
                    numPixelsPerArea - 2 * numPixelsInMargin - 2,
                    numPixelsPerArea - 2 * numPixelsInMargin - 2));
                g.DrawString(
                    areaInfo.Name.ToString(),
                    font,
                    brush,
                    pixelPoint.X + 2,
                    pixelPoint.Y + 2);
                if (areaInfo.Discoverer != null)
                {
                    SizeF sizeF = g.MeasureString(areaInfo.Name.ToString(), font);
                    g.DrawString(
                        String.Format("({0})", areaInfo.Discoverer),
                        font,
                        brush,
                        pixelPoint.X + 2,
                        pixelPoint.Y + 2 + sizeF.Height + 2);
                }
                g.Clip = prevClip;
            }
        }

        void HighlightCurrentArea(Graphics g)
        {
            PixelPoint center = CalculatePixelPoint(new AreaPoint(0, 0));
            Pen p = new Pen(Color.Yellow, numPixelsInMargin);
            g.DrawRectangle(p, center.X, center.Y, numPixelsPerArea, numPixelsPerArea);
        }

        protected virtual PixelPoint CalculatePixelPoint(AreaPoint areaPoint)
        {
            PixelPoint p = new PixelPoint();
            p.X = Width / 2 + (int)(areaPoint.X * numPixelsPerArea) - (int)numPixelsPerArea / 2;
            p.Y = Height / 2 - (int)(areaPoint.Y * numPixelsPerArea) - (int)numPixelsPerArea / 2;
            return p;
        }
    }
}
