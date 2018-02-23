using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBlack.Core.Collections.Generic;

namespace LiveSplit.OriDE.GPS
{
    public partial class MapPanel : UserControl
    {
        private Image backgroundImage;
        public float zoom;

        private PointF swampTeleporter = new PointF(493.719818f, -74.31961f);
        private PointF gladesTeleporter = new PointF(109.90181f, -257.681549f);

        private PointF swampTeleporterOnMap = new PointF(4523, 2867);
        private PointF gladesTeleporterOnMap = new PointF(3438, 3384);

        private CircularBuffer<PointF> trace;
        private PointF offset;
        private PointF? lastDragLocation = null;

        private PointF centerLocationInGame;
        public PointF CenterLocationInGame
        {
            get
            {
                return centerLocationInGame;
            }
            set
            {
                var last = centerLocationInGame;
                if (last != value) {
                    centerLocationInGame = value;
                    trace.Enqueue(value);
                    Invalidate();
                }
            }
        }

        public MapPanel()
        {
            InitializeComponent();
            backgroundImage = Properties.Resources.Map;
            zoom = 1.0f;
            this.DoubleBuffered = true;
            trace = new CircularBuffer<PointF>(100);
            this.MouseWheel += Panel_MouseWheel;
            this.MouseMove += Panel_MouseMove;
            this.MouseUp += Panel_MouseUp;
        }

        private void Panel_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                lastDragLocation = null;
            }
        }

        private void Panel_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                PointF currentDragLocation = e.Location;
                if (lastDragLocation.HasValue) {
                    offset = PointF.Subtract(offset, new SizeF(PointF.Subtract(currentDragLocation, new SizeF(lastDragLocation.Value))));
                    Invalidate();
                }
                lastDragLocation = currentDragLocation;
            }
        }

        private void Panel_MouseWheel(object sender, MouseEventArgs e)
        {
            var minZoom = 0.04f;
            var maxZoom = 3.0f;
            zoom = Math.Max(Math.Min(maxZoom, zoom + 0.001f * e.Delta), minZoom);
            Invalidate();
        }

        private PointF centerLocationOnMap()
        {
            return toMapCoords(CenterLocationInGame);
        }

        private PointF toMapCoords(PointF gameCoords)
        {
            var map1 = gladesTeleporterOnMap;
            var map2 = swampTeleporterOnMap;
            var game1 = gladesTeleporter;
            var game2 = swampTeleporter;

            var gameLeftSide = game2.X - ((map2.X / (map2.X - map1.X)) * (game2.X - game1.X));
            var gameTopSide = game2.Y - ((map2.Y / (map2.Y - map1.Y)) * (game2.Y - game1.Y));

            var scaleX = (swampTeleporter.X - gladesTeleporter.X) / (swampTeleporterOnMap.X - gladesTeleporterOnMap.X);
            var scaleY = (swampTeleporter.Y - gladesTeleporter.Y) / (swampTeleporterOnMap.Y - gladesTeleporterOnMap.Y);
            var mapX = (gameCoords.X - gameLeftSide) / scaleX;
            var mapY = (gameCoords.Y - gameTopSide) / scaleY;
            return new PointF(mapX, mapY);
        }

        private PointF toWindowCoords(PointF gameCoord)
        {
            var mapCoord = toMapCoords(gameCoord);
            var centerMap = PointF.Add(centerLocationOnMap(), new SizeF(offset));
            var relativeToCenter = PointF.Subtract(mapCoord, new SizeF(centerMap));
            // Apply zoom
            var zoomed = new PointF(relativeToCenter.X * zoom, relativeToCenter.Y * zoom);

            return new PointF(zoomed.X + this.ClientSize.Width / 2, zoomed.Y + this.ClientSize.Height / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Pen p = new Pen(System.Drawing.Color.Red, 4);

            PointF? lastPoint = null;
            foreach (var point in trace)
            {
                if (lastPoint.HasValue)
                {
                    e.Graphics.DrawLine(p, toWindowCoords(lastPoint.Value), toWindowCoords(point));
                }
                lastPoint = point;
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            var rc = new Rectangle(this.ClientSize.Width - backgroundImage.Width,
                this.ClientSize.Height - backgroundImage.Height,
                backgroundImage.Width,
                backgroundImage.Height);
            //e.Graphics.DrawImage(backgroundImage, rc);
            var extractWidth = this.ClientSize.Width * (1 / zoom);
            var extractHeight = this.ClientSize.Height * (1 / zoom);
            var window = new SizeF(extractWidth, extractHeight);
            var centerLocation = PointF.Add(centerLocationOnMap(), new SizeF(offset));


            var sourceRect = new RectangleF(PointF.Subtract(centerLocation, new SizeF(window.Width / 2, window.Height / 2)), window); // - extractSize / 2, centerLocation.Y - extractHeight / 2)
            var destRect = new RectangleF(0, 0, this.ClientSize.Width, this.ClientSize.Height);

            //var sourceRect = new RectangleF(0, 0, backgroundImage.Width, backgroundImage.Height);
            //var destRect = new RectangleF(0, 0, this.ClientSize.Width, this.ClientSize.Width / aspectRatio);
            var black = new SolidBrush(Color.Black);
            e.Graphics.FillRectangle(black, destRect);
            e.Graphics.DrawImage(backgroundImage, destRect, sourceRect, GraphicsUnit.Pixel);
        }
    }
}
