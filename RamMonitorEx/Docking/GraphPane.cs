using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls.LineGraph;

namespace WindowsFormsApp1
{
    /// <summary>
    /// 折れ線グラフ表示用のドッキング可能なペイン
    /// </summary>
    public class GraphPane : DockContent
    {
        private Panel? _containerPanel;
        private LineGraphControl? _graphControl;
        private string _paneName;
        private System.Windows.Forms.Timer? _updateTimer;
        private Random _random = new Random();
        private int _dataCounter = 0;

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

            _paneName = title;
            this.Text = title;
            this.TabText = title;
            this.ToolTipText = title;
            this.HideOnClose = false;

            InitializeControls();
        }

        /// <summary>
        /// パネル名（一意の識別子）
        /// </summary>
        public string PaneName => _paneName;

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

            // データ更新タイマーを開始
            StartDataSimulation();
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
        /// データシミュレーションを開始
        /// </summary>
        private void StartDataSimulation()
        {
            _updateTimer = new System.Windows.Forms.Timer();
            _updateTimer.Interval = 100; // 0.1秒間隔
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        /// <summary>
        /// タイマーイベント - データを更新
        /// </summary>
        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_graphControl == null) return;

            // すべての系列にデータを追加
            foreach (var series in _graphControl.Series)
            {
                float value;

                // 系列名に応じたデータパターンを生成
                if (series.Name.Contains("データ1"))
                {
                    // 波形パターン
                    if (_dataCounter < 150)
                    {
                        value = (float)(_random.NextDouble() * 15 + 10);
                    }
                    else if (_dataCounter >= 150 && _dataCounter < 300)
                    {
                        value = (float)(_random.NextDouble() * 25 + 40);
                    }
                    else
                    {
                        value = (float)(_random.NextDouble() * 20 + 25);
                    }
                }
                else if (series.Name.Contains("データ2"))
                {
                    // 緩やかな変化
                    value = (float)(40 + Math.Sin(_dataCounter * 0.015) * 20 + _random.NextDouble() * 10);
                }
                else
                {
                    // デフォルトパターン
                    value = (float)(_random.NextDouble() * 50 + 25);
                }

                series.AddValue(value);
            }

            _dataCounter++;
            if (_dataCounter >= 600) _dataCounter = 0; // カウンターリセット
        }

        /// <summary>
        /// グラフコントロールへのアクセス（外部からデータ追加用）
        /// </summary>
        public LineGraphControl GraphControl => _graphControl;

        /// <summary>
        /// DockPanelの永続化用の文字列を取得
        /// </summary>
        protected override string GetPersistString()
        {
            return $"GraphPane|{_paneName}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // タイマーを停止・破棄
                if (_updateTimer != null)
                {
                    _updateTimer.Stop();
                    _updateTimer.Dispose();
                    _updateTimer = null;
                }

                // パネル名の登録を解除
                PaneNameManager.Instance.UnregisterName(_paneName);

                _graphControl?.Dispose();
                _containerPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}