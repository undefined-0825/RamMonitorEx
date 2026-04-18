using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls.MultiLayoutGrid;

namespace WindowsFormsApp1.Docking
{
    /// <summary>
    /// マルチレイアウトグリッド表示用のドッキング可能なペイン
    /// </summary>
    public class MultiLayoutGridPane : DockContent
    {
        private Panel? _containerPanel;
        private MultiLayoutGridControl? _gridControl;
        private string _paneName;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="title">ペインのタイトル</param>
        public MultiLayoutGridPane(string title)
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

            // マルチレイアウトグリッドコントロール
            _gridControl = new MultiLayoutGridControl
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Black,
                ForeColor = Color.White
            };

            _containerPanel.Controls.Add(_gridControl);

            // デモデータを追加
            AddDemoData();
        }

        /// <summary>
        /// デモ用のデータを追加
        /// </summary>
        private void AddDemoData()
        {
            if (_gridControl == null) return;

            Random rand = new Random();

            // ヘッダー行
            GridRow headerRow = new GridRow { Height = 35 };
            headerRow.Cells.Add(new GridCell { Text = "項目", Width = 150, HorizontalAlignment = ContentAlignment.MiddleLeft, ForeColor = Color.Cyan });
            headerRow.Cells.Add(new GridCell { Text = "値", Width = 100, HorizontalAlignment = ContentAlignment.MiddleRight, ForeColor = Color.Cyan });
            headerRow.Cells.Add(new GridCell { Text = "単位", Width = 80, HorizontalAlignment = ContentAlignment.MiddleLeft, ForeColor = Color.Cyan });
            _gridControl.Rows.Add(headerRow);

            // データ行（動的値）
            for (int i = 0; i < 5; i++)
            {
                GridRow dataRow = new GridRow { Height = 30 };
                dataRow.Cells.Add(new GridCell { Text = $"データ{i + 1}", Width = 150, HorizontalAlignment = ContentAlignment.MiddleLeft });
                dataRow.Cells.Add(new GridCell { ValueProvider = () => rand.Next(0, 100).ToString(), Width = 100, HorizontalAlignment = ContentAlignment.MiddleRight, ForeColor = Color.LightGreen });
                dataRow.Cells.Add(new GridCell { Text = "%", Width = 80, HorizontalAlignment = ContentAlignment.MiddleLeft });
                _gridControl.Rows.Add(dataRow);
            }

            // 更新タイマー（デモ用）
            System.Windows.Forms.Timer updateTimer = new System.Windows.Forms.Timer();
            updateTimer.Interval = 1000; // 1秒ごと
            updateTimer.Tick += (s, e) => _gridControl?.RequestRedraw();
            updateTimer.Start();

            _gridControl.RequestRedraw();
        }

        /// <summary>
        /// グリッドコントロールへのアクセス
        /// </summary>
        public MultiLayoutGridControl? GridControl => _gridControl;

        /// <summary>
        /// DockPanelの永続化用の文字列を取得
        /// </summary>
        protected override string GetPersistString()
        {
            return $"MultiLayoutGridPane|{_paneName}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // パネル名の登録を解除
                PaneNameManager.Instance.UnregisterName(_paneName);

                _gridControl?.Dispose();
                _containerPanel?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
