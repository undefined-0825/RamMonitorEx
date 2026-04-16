using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls;

namespace WindowsFormsApp1
{
    /// <summary>
    /// 折れ線グラフ表示用のドッキング可能なペイン
    /// </summary>
    public class GraphPane : DockContent
    {
        private Panel? _containerPanel;
        private LineGraphControl? _graphControl;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="title">ペインのタイトル</param>
        public GraphPane(string title)
        {
            if (string.IsNullOrEmpty(title))
            {
                throw new ArgumentNullException(nameof(title));
            }

            this.Text = title;
            this.TabText = title;
            this.ToolTipText = title;
            this.HideOnClose = false;

            InitializeControls();
        }

        /// <summary>
        /// コントロールの初期化
        /// </summary>
        private void InitializeControls()
        {
            // 余白用の親パネル
            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            this.Controls.Add(_containerPanel);

            // 折れ線グラフコントロール
            _graphControl = new LineGraphControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(13, 13, 13),
                MaxPoints = 60,
                AutoScaleY = false,
                YMin = 0,
                YMax = 100,
                LineWidth = 1.5f,
                ShowGrid = true,
                GridColor = Color.FromArgb(40, 40, 40),
                RedrawIntervalMs = 100,
                FillArea = false,
                PlotPadding = new Padding(5, 20, 5, 25)
            };

            _containerPanel.Controls.Add(_graphControl);

            // 初期系列を追加（デモ用）
            AddDemoSeries();
        }

        /// <summary>
        /// デモ用の系列を追加
        /// </summary>
        private void AddDemoSeries()
        {
            Random rand = new Random(DateTime.Now.Millisecond);
            
            GraphSeries series1 = new GraphSeries("データ1", Color.FromArgb(0, 180, 200));
            GraphSeries series2 = new GraphSeries("データ2", Color.FromArgb(255, 140, 0));
            
            // 初期ダミーデータを追加
            for (int i = 0; i < 60; i++)
            {
                series1.AddValue((float)(rand.NextDouble() * 50 + 25));
                series2.AddValue((float)(rand.NextDouble() * 40 + 30));
            }
            
            _graphControl.AddSeries(series1);
            _graphControl.AddSeries(series2);
        }

        /// <summary>
        /// グラフコントロールへのアクセス（外部からデータ追加用）
        /// </summary>
        public LineGraphControl GraphControl => _graphControl;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _graphControl?.Dispose();
                _containerPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}