using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls.RamMonitorView;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsApp1
{
    /// <summary>
    /// RAMモニタ表示用のドッキング可能なペイン
    /// </summary>
    public class RamMonitorViewPane : DockContent
    {
        private Panel? _containerPanel;
        private RamMonitorView? _ramMonitorView;
        private Timer? _updateTimer;
        private Random _random = new Random();
        private int _updateCounter = 0;
        private string _paneName;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="title">ペインのタイトル</param>
        public RamMonitorViewPane(string title)
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
            SetupSampleData();
            StartDataUpdate();
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
            // 配置用の親パネル
            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(5)
            };
            this.Controls.Add(_containerPanel);

            // RAMモニタビューコントロール
            _ramMonitorView = new RamMonitorView
            {
                Dock = DockStyle.Fill,
                GridLineVisible = true,
                GridLineColor = Color.Gray,
                HeaderVisible = true,
                ValueAlignment = ValueTextAlignment.Right,
                ColumnResizeEnabled = true,
                PanEnabled = true
            };

            _containerPanel.Controls.Add(_ramMonitorView);
        }

        /// <summary>
        /// サンプルデータのセットアップ
        /// </summary>
        private void SetupSampleData()
        {
            if (_ramMonitorView == null) return;

            // データ行の追加
            _ramMonitorView.AddDataRow("CPU使用率", "0", "%");
            _ramMonitorView.AddDataRow("メモリ使用率", "0", "%");
            _ramMonitorView.AddDataRow("ディスク使用率", "0", "%");
            
            // コメント行の追加
            _ramMonitorView.AddCommentRow("--- システム情報 ---");
            
            _ramMonitorView.AddDataRow("温度", "0", "℃");
            _ramMonitorView.AddDataRow("回転数", "0", "rpm");
            _ramMonitorView.AddDataRow("電圧", "0", "V");
            
            // コメント行の追加
            _ramMonitorView.AddCommentRow("※ 値は1秒ごとに更新されます");
            
            _ramMonitorView.AddDataRow("ネットワーク送信", "0", "MB/s");
            _ramMonitorView.AddDataRow("ネットワーク受信", "0", "MB/s");
        }

        /// <summary>
        /// データ更新タイマーの開始
        /// </summary>
        private void StartDataUpdate()
        {
            _updateTimer = new Timer
            {
                Interval = 1000 // 1秒ごとに更新
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        /// <summary>
        /// タイマーティック時のデータ更新
        /// </summary>
        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            if (_ramMonitorView == null) return;

            _updateCounter++;

            // 行インデックス 0, 1, 2, 4, 5, 6, 8, 9 がデータ行
            int[] dataRowIndices = { 0, 1, 2, 4, 5, 6, 8, 9 };

            foreach (int index in dataRowIndices)
            {
                string value = GenerateRandomValue(index);
                _ramMonitorView.UpdateValue(index, value);
            }
        }

        /// <summary>
        /// ランダム値の生成
        /// </summary>
        private string GenerateRandomValue(int rowIndex)
        {
            return rowIndex switch
            {
                0 => _random.Next(10, 100).ToString(), // CPU使用率
                1 => _random.Next(30, 80).ToString(),  // メモリ使用率
                2 => _random.Next(5, 50).ToString(),   // ディスク使用率
                4 => _random.Next(30, 70).ToString(),  // 温度
                5 => _random.Next(800, 2000).ToString(), // 回転数
                6 => (_random.NextDouble() * 2 + 11).ToString("F2"), // 電圧
                8 => (_random.NextDouble() * 10).ToString("F1"), // ネットワーク送信
                9 => (_random.NextDouble() * 20).ToString("F1"), // ネットワーク受信
                _ => "0"
            };
        }

        /// <summary>
        /// RamMonitorViewコントロールへのアクセス（外部からデータ追加用）
        /// </summary>
        public RamMonitorView? RamMonitorView => _ramMonitorView;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // パネル名の登録を解除
                PaneNameManager.Instance.UnregisterName(_paneName);

                _updateTimer?.Stop();
                _updateTimer?.Dispose();
                _ramMonitorView?.Dispose();
                _containerPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
