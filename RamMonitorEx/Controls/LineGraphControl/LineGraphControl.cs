using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using RamMonitorEx.Forms;
using Timer = System.Windows.Forms.Timer;

namespace RamMonitorEx.Controls.LineGraph
{
    /// <summary>
    /// 複数系列の折れ線グラフを高速描画するUserControl
    /// </summary>
    public class LineGraphControl : UserControl
    {
        private List<GraphSeries> seriesList = new List<GraphSeries>();
        private Timer redrawTimer;
        private bool needsRedraw = false;
        private Bitmap? backBuffer; // オフスクリーンバッファ
        private ContextMenuStrip? contextMenu;
        
        // プロパティのバッキングフィールド
        private int maxPoints = 1000;
        private float yMin = 0f;
        private float yMax = 100f;
        private bool autoScaleY = true;
        private float lineWidth = 1.5f;
        private bool showGrid = true;
        private Color gridColor = Color.LightGray;
        private Padding plotPadding = new Padding(50, 20, 20, 40);
        private int redrawIntervalMs = 50;
        private bool fillArea = false;
        private int fillAlpha = 128;

        public LineGraphControl()
        {
            // ダブルバッファリングを有効化
            this.SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();
            
            this.BackColor = Color.FromArgb(13, 13, 13);
            this.Size = new Size(600, 400);

            // 再描画タイマーの初期化
            redrawTimer = new Timer();
            redrawTimer.Interval = redrawIntervalMs;
            redrawTimer.Tick += RedrawTimer_Tick;
            redrawTimer.Start();

            // コンテキストメニューの初期化
            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            contextMenu = new ContextMenuStrip();
            
            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("プロパティ(&P)");
            propertiesItem.Click += PropertiesItem_Click;
            contextMenu.Items.Add(propertiesItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem clearDataItem = new ToolStripMenuItem("データをクリア(&C)");
            clearDataItem.Click += ClearDataItem_Click;
            contextMenu.Items.Add(clearDataItem);

            this.ContextMenuStrip = contextMenu;
        }

        private void PropertiesItem_Click(object sender, EventArgs e)
        {
            using (GraphPropertiesForm form = new GraphPropertiesForm(this))
            {
                form.ShowDialog(this.FindForm());
            }
        }

        private void ClearDataItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "すべてのデータをクリアしますか？",
                "確認",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                ClearAllData();
            }
        }

        #region プロパティ

        /// <summary>
        /// 描画対象の系列一覧
        /// </summary>
        public IReadOnlyList<GraphSeries> Series => seriesList;

        /// <summary>
        /// 1系列あたりの最大保持点数
        /// </summary>
        public int MaxPoints
        {
            get => maxPoints;
            set
            {
                if (value > 0)
                {
                    maxPoints = value;
                    foreach (var series in seriesList)
                    {
                        series.SetMaxPoints(maxPoints);
                    }
                }
            }
        }

        /// <summary>
        /// 手動スケール時のY軸最小値
        /// </summary>
        public float YMin
        {
            get => yMin;
            set { yMin = value; RequestRedraw(); }
        }

        /// <summary>
        /// 手動スケール時のY軸最大値
        /// </summary>
        public float YMax
        {
            get => yMax;
            set { yMax = value; RequestRedraw(); }
        }

        /// <summary>
        /// Y軸を自動スケーリングするか
        /// </summary>
        public bool AutoScaleY
        {
            get => autoScaleY;
            set { autoScaleY = value; RequestRedraw(); }
        }

        /// <summary>
        /// 折れ線の太さ（全系列共通）
        /// </summary>
        public float LineWidth
        {
            get => lineWidth;
            set { lineWidth = value; RequestRedraw(); }
        }

        /// <summary>
        /// グリッド表示有無
        /// </summary>
        public bool ShowGrid
        {
            get => showGrid;
            set { showGrid = value; RequestRedraw(); }
        }

        /// <summary>
        /// グリッド色
        /// </summary>
        public Color GridColor
        {
            get => gridColor;
            set { gridColor = value; RequestRedraw(); }
        }

        /// <summary>
        /// 描画領域の内側余白
        /// </summary>
        public Padding PlotPadding
        {
            get => plotPadding;
            set { plotPadding = value; RequestRedraw(); }
        }

        /// <summary>
        /// 再描画周期(ms)
        /// </summary>
        public int RedrawIntervalMs
        {
            get => redrawIntervalMs;
            set
            {
                if (value > 0)
                {
                    redrawIntervalMs = value;
                    redrawTimer.Interval = redrawIntervalMs;
                }
            }
        }

        /// <summary>
        /// 系列のリスト（読み取り専用）
        /// </summary>
        public IReadOnlyList<GraphSeries> SeriesList => seriesList.AsReadOnly();

        /// <summary>
        /// エリアを塗りつぶすか（タスクマネージャスタイル）
        /// </summary>
        public bool FillArea
        {
            get => fillArea;
            set { fillArea = value; RequestRedraw(); }
        }

        /// <summary>
        /// 塗りつぶしの透明度 (0-255)
        /// </summary>
        public int FillAlpha
        {
            get => fillAlpha;
            set 
            { 
                fillAlpha = Math.Max(0, Math.Min(255, value));
                RequestRedraw();
            }
        }

        #endregion

        #region 系列管理

        /// <summary>
        /// 系列を追加
        /// </summary>
        public void AddSeries(GraphSeries series)
        {
            if (series != null)
            {
                series.SetMaxPoints(maxPoints);
                seriesList.Add(series);
                RequestRedraw();
            }
        }

        /// <summary>
        /// 系列を削除
        /// </summary>
        public void RemoveSeries(GraphSeries series)
        {
            if (seriesList.Remove(series))
            {
                RequestRedraw();
            }
        }

        /// <summary>
        /// すべての系列をクリア
        /// </summary>
        public void ClearSeries()
        {
            seriesList.Clear();
            RequestRedraw();
        }

        /// <summary>
        /// すべての系列のデータをクリア
        /// </summary>
        public void ClearAllData()
        {
            foreach (var series in seriesList)
            {
                series.Clear();
            }
            RequestRedraw();
        }

        #endregion

        #region 描画制御

        /// <summary>
        /// 再描画をリクエスト（実際の描画はタイマーで間引かれる）
        /// </summary>
        public void RequestRedraw()
        {
            needsRedraw = true;
        }

        private void RedrawTimer_Tick(object? sender, EventArgs e)
        {
            if (needsRedraw)
            {
                needsRedraw = false;
                Invalidate();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // バックバッファが未作成またはサイズが変わった場合は再作成
            if (backBuffer == null || backBuffer.Width != Width || backBuffer.Height != Height)
            {
                backBuffer?.Dispose();
                backBuffer = new Bitmap(Width, Height);
            }

            // バックバッファに描画
            using (Graphics g = Graphics.FromImage(backBuffer))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighSpeed;

                // 背景クリア
                g.Clear(this.BackColor);

                // 描画領域の計算
                Rectangle plotRect = new Rectangle(
                    plotPadding.Left,
                    plotPadding.Top,
                    Width - plotPadding.Left - plotPadding.Right,
                    Height - plotPadding.Top - plotPadding.Bottom
                );

                if (plotRect.Width > 0 && plotRect.Height > 0)
                {
                    // クリッピング領域を設定
                    g.SetClip(plotRect);

                    // グリッド描画
                    if (showGrid)
                    {
                        DrawGrid(g, plotRect);
                    }

                    // 各系列の折れ線を描画
                    DrawAllSeries(g, plotRect);

                    // クリッピング解除
                    g.ResetClip();

                    // 軸と枠線を描画
                    DrawAxes(g, plotRect);
                }
            }

            // バックバッファを画面に転送
            e.Graphics.DrawImageUnscaled(backBuffer, 0, 0);
        }

        private void DrawGrid(Graphics g, Rectangle plotRect)
        {
            using (Pen gridPen = new Pen(gridColor, 1))
            {
                gridPen.DashStyle = DashStyle.Solid;

                // 縦線（5分割）
                for (int i = 0; i <= 5; i++)
                {
                    float x = plotRect.Left + (plotRect.Width / 5f) * i;
                    g.DrawLine(gridPen, x, plotRect.Top, x, plotRect.Bottom);
                }

                // 横線（5分割）
                for (int i = 0; i <= 5; i++)
                {
                    float y = plotRect.Top + (plotRect.Height / 5f) * i;
                    g.DrawLine(gridPen, plotRect.Left, y, plotRect.Right, y);
                }
            }
        }

        private void DrawAxes(Graphics g, Rectangle plotRect)
        {
            using (Pen axisPen = new Pen(Color.FromArgb(60, 60, 60), 1))
            using (Font font = new Font("Segoe UI", 8))
            using (Brush textBrush = new SolidBrush(Color.FromArgb(180, 180, 180)))
            {
                // 枠線
                g.DrawRectangle(axisPen, plotRect);

                // Y軸スケール値の計算
                float displayYMin, displayYMax;
                CalculateYScale(out displayYMin, out displayYMax);

                // Y軸ラベル（左下に最小値、左上に最大値のみ）
                // 左下
                string labelMin = displayYMin.ToString("F0");
                SizeF labelMinSize = g.MeasureString(labelMin, font);
                g.DrawString(labelMin, font, textBrush, 
                    5, 
                    plotRect.Bottom - labelMinSize.Height);

                // 左上
                string labelMax = displayYMax.ToString("F0") + "%";
                SizeF labelMaxSize = g.MeasureString(labelMax, font);
                g.DrawString(labelMax, font, textBrush, 
                    5, 
                    plotRect.Top);

                // X軸ラベル（60秒で固定表示）
                string labelLeft = "60 秒";
                SizeF labelLeftSize = g.MeasureString(labelLeft, font);
                g.DrawString(labelLeft, font, textBrush,
                    plotRect.Left,
                    plotRect.Bottom + 5);

                string labelRight = "0";
                SizeF labelRightSize = g.MeasureString(labelRight, font);
                g.DrawString(labelRight, font, textBrush,
                    plotRect.Right - labelRightSize.Width,
                    plotRect.Bottom + 5);
            }
        }

        private void DrawAllSeries(Graphics g, Rectangle plotRect)
        {
            float displayYMin, displayYMax;
            CalculateYScale(out displayYMin, out displayYMax);

            if (displayYMax <= displayYMin)
                return;

            foreach (var series in seriesList)
            {
                if (!series.Visible || series.Values.Count < 2)
                    continue;

                DrawSeries(g, plotRect, series, displayYMin, displayYMax);
            }
        }

        private void DrawSeries(Graphics g, Rectangle plotRect, GraphSeries series, float yMin, float yMax)
        {
            int pointCount = series.Values.Count;
            if (pointCount < 2)
                return;

            // データポイントを画面座標に変換（X軸は最大点数で固定、右から左へ）
            PointF[] screenPoints = new PointF[pointCount];
            
            for (int i = 0; i < pointCount; i++)
            {
                // 右端が最新データになるように、右から左へ描画
                float x = plotRect.Right - (float)i / (maxPoints - 1) * plotRect.Width;
                float normalizedY = (series.Values[pointCount - 1 - i] - yMin) / (yMax - yMin);
                normalizedY = Math.Max(0, Math.Min(1, normalizedY));
                float y = plotRect.Bottom - normalizedY * plotRect.Height;
                
                screenPoints[i] = new PointF(x, y);
            }

            // エリア塗りつぶし（高速化のため単色塗りつぶし）
            if (fillArea)
            {
                // 塗りつぶし用のポイント配列を作成（底辺を追加）
                PointF[] fillPoints = new PointF[pointCount + 2];
                Array.Copy(screenPoints, fillPoints, pointCount);
                
                // 底辺の2点を追加
                fillPoints[pointCount] = new PointF(screenPoints[pointCount - 1].X, plotRect.Bottom);
                fillPoints[pointCount + 1] = new PointF(screenPoints[0].X, plotRect.Bottom);

                // 単色で塗りつぶし（グラデーションなし）
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(fillAlpha, series.Color)))
                {
                    g.FillPolygon(brush, fillPoints);
                }
            }

            // 折れ線を描画
            using (Pen linePen = new Pen(series.Color, lineWidth))
            {
                linePen.LineJoin = LineJoin.Round;
                g.DrawLines(linePen, screenPoints);
            }
        }

        private void CalculateYScale(out float min, out float max)
        {
            if (autoScaleY)
            {
                min = float.MaxValue;
                max = float.MinValue;

                foreach (var series in seriesList)
                {
                    if (series.Visible && series.Values.Count > 0)
                    {
                        float seriesMin = series.Values.Min();
                        float seriesMax = series.Values.Max();
                        
                        if (seriesMin < min) min = seriesMin;
                        if (seriesMax > max) max = seriesMax;
                    }
                }

                // データがない場合のデフォルト値
                if (min == float.MaxValue || max == float.MinValue)
                {
                    min = 0f;
                    max = 100f;
                }
                
                // 最小値と最大値が同じ場合は範囲を広げる
                if (Math.Abs(max - min) < 0.001f)
                {
                    min -= 10f;
                    max += 10f;
                }
            }
            else
            {
                min = yMin;
                max = yMax;
            }
        }

        #endregion

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            // ダブルバッファリング時は背景描画をスキップしてちらつきを防止
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                backBuffer?.Dispose();
                redrawTimer?.Stop();
                redrawTimer?.Dispose();
                contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}