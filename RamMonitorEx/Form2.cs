using System;
using System.Drawing;
using System.Windows.Forms;
using RamMonitorEx.Controls;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsApp1
{
    public partial class Form2 : Form
    {
        private RamMonitorView ramMonitorView;
        private Timer updateTimer;
        private Random random = new Random();
        private int updateCounter = 0;

        public Form2()
        {
            InitializeComponent();
            InitializeRamMonitorView();
            SetupSampleData();
            StartDataUpdate();
        }

        private void InitializeRamMonitorView()
        {
            ramMonitorView = new RamMonitorView
            {
                Location = new Point(10, 10),
                Size = new Size(400, 400),
                BackColor = Color.White,
                ForeColor = Color.Black,
                GridLineVisible = true,
                GridLineColor = Color.Gray,
                HeaderVisible = true,
                ValueAlignment = ValueTextAlignment.Right,
                ColumnResizeEnabled = true,
                PanEnabled = true
            };

            ramMonitorView.RowClicked += RamMonitorView_RowClicked;
            ramMonitorView.RowDoubleClicked += RamMonitorView_RowDoubleClicked;
            ramMonitorView.ColumnWidthChanged += RamMonitorView_ColumnWidthChanged;

            this.Controls.Add(ramMonitorView);
        }

        private void SetupSampleData()
        {
            // データ行の追加
            ramMonitorView.AddDataRow("CPU使用率", "0", "%");
            ramMonitorView.AddDataRow("メモリ使用率", "0", "%");
            ramMonitorView.AddDataRow("ディスク使用率", "0", "%");
            
            // コメント行の追加
            ramMonitorView.AddCommentRow("--- システム情報 ---");
            
            ramMonitorView.AddDataRow("温度", "0", "℃");
            ramMonitorView.AddDataRow("回転数", "0", "rpm");
            ramMonitorView.AddDataRow("電圧", "0", "V");
            
            // コメント行の追加
            ramMonitorView.AddCommentRow("※ 値は1秒ごとに更新されます");
            
            ramMonitorView.AddDataRow("ネットワーク送信", "0", "MB/s");
            ramMonitorView.AddDataRow("ネットワーク受信", "0", "MB/s");
        }

        private void StartDataUpdate()
        {
            updateTimer = new Timer
            {
                Interval = 1000 // 1秒ごとに更新
            };
            updateTimer.Tick += UpdateTimer_Tick;
            updateTimer.Start();
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            updateCounter++;

            // 行インデックス 0, 1, 2, 4, 5, 6, 8, 9 がデータ行
            int[] dataRowIndices = { 0, 1, 2, 4, 5, 6, 8, 9 };

            foreach (int index in dataRowIndices)
            {
                string value = GenerateRandomValue(index);
                ramMonitorView.UpdateValue(index, value);
            }

            // フォームのタイトルに更新回数を表示
            this.Text = $"RamMonitorView Sample - 更新回数: {updateCounter}";
        }

        private string GenerateRandomValue(int rowIndex)
        {
            return rowIndex switch
            {
                0 => random.Next(10, 100).ToString(), // CPU使用率
                1 => random.Next(30, 80).ToString(),  // メモリ使用率
                2 => random.Next(5, 50).ToString(),   // ディスク使用率
                4 => random.Next(30, 70).ToString(),  // 温度
                5 => random.Next(800, 2000).ToString(), // 回転数
                6 => (random.NextDouble() * 2 + 11).ToString("F2"), // 電圧
                8 => (random.NextDouble() * 10).ToString("F1"), // ネットワーク送信
                9 => (random.NextDouble() * 20).ToString("F1"), // ネットワーク受信
                _ => "0"
            };
        }

        private void RamMonitorView_RowClicked(object? sender, int rowIndex)
        {
            if (ramMonitorView.Rows[rowIndex] is ValueDataRow dataRow)
            {
                MessageBox.Show($"行 {rowIndex} がクリックされました\n\n" +
                              $"ラベル: {dataRow.LabelText}\n" +
                              $"値: {dataRow.ValueText}\n" +
                              $"単位: {dataRow.UnitText}",
                              "行クリック",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
            else if (ramMonitorView.Rows[rowIndex] is ValueCommentRow commentRow)
            {
                MessageBox.Show($"コメント行 {rowIndex} がクリックされました\n\n" +
                              $"コメント: {commentRow.CommentText}",
                              "行クリック",
                              MessageBoxButtons.OK,
                              MessageBoxIcon.Information);
            }
        }

        private void RamMonitorView_RowDoubleClicked(object? sender, int rowIndex)
        {
            MessageBox.Show($"行 {rowIndex} がダブルクリックされました",
                          "行ダブルクリック",
                          MessageBoxButtons.OK,
                          MessageBoxIcon.Information);
        }

        private void RamMonitorView_ColumnWidthChanged(object? sender, EventArgs e)
        {
            // 列幅が変更された時の処理（必要に応じて実装）
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            updateTimer?.Stop();
            updateTimer?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
