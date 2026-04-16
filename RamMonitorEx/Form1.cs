using System;
using System.Drawing;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private DockPanel _dockPanel;
        private MenuStrip _menuStrip;
        private int _graphPaneSequence = 0;

        private LineGraphControl lineGraph;
        private Timer dataTimer;
        private Random random = new Random();
        private int dataCounter = 0;

        public Form1()
        {
            InitializeComponent();
            InitializeDockPanel();
            InitializeMenu();
            InitializeLineGraph();
            StartDataSimulation();
        }

        /// <summary>
        /// DockPanel の初期化
        /// </summary>
        private void InitializeDockPanel()
        {
            var vS2015LightTheme = new WeifenLuo.WinFormsUI.Docking.VS2015LightTheme();
            _dockPanel = new DockPanel
            {
                Dock = DockStyle.Fill,
                DocumentStyle = DocumentStyle.DockingWindow,
                Theme = vS2015LightTheme
            };
            this.Controls.Add(_dockPanel);
        }

        /// <summary>
        /// メニューの初期化
        /// </summary>
        private void InitializeMenu()
        {
            _menuStrip = new MenuStrip();

            // ファイルメニュー
            ToolStripMenuItem fileMenu = new ToolStripMenuItem("ファイル(&F)");

            // 新規追加メニュー
            ToolStripMenuItem newGraphItem = new ToolStripMenuItem("新規追加(&N)");
            newGraphItem.ShortcutKeys = Keys.Control | Keys.N;
            newGraphItem.Click += NewGraphItem_Click;

            fileMenu.DropDownItems.Add(newGraphItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());

            // RamMonitorView サンプル表示メニュー
            ToolStripMenuItem ramMonitorViewItem = new ToolStripMenuItem("RamMonitorView サンプル(&R)");
            ramMonitorViewItem.Click += (s, e) => 
            {
                Form2 form2 = new Form2();
                form2.Show();
            };
            fileMenu.DropDownItems.Add(ramMonitorViewItem);
            fileMenu.DropDownItems.Add(new ToolStripSeparator());

            // 終了メニュー
            ToolStripMenuItem exitItem = new ToolStripMenuItem("終了(&X)");
            exitItem.Click += (s, e) => this.Close();
            fileMenu.DropDownItems.Add(exitItem);

            _menuStrip.Items.Add(fileMenu);

            this.MainMenuStrip = _menuStrip;
            this.Controls.Add(_menuStrip);
        }

        /// <summary>
        /// 新規グラフペイン追加メニュークリック
        /// </summary>
        private void NewGraphItem_Click(object sender, EventArgs e)
        {
            AddNewGraphPane();
        }

        /// <summary>
        /// 新しいグラフペインを追加
        /// </summary>
        private void AddNewGraphPane()
        {
            _graphPaneSequence++;
            string title = $"グラフ {_graphPaneSequence}";
            
            GraphPane pane = new GraphPane(title);
            pane.Show(_dockPanel, DockState.Document);
        }

        private void InitializeLineGraph()
        {
            // タスクマネージャスタイルのグラフコントロール作成
            lineGraph = new LineGraphControl
            {
                Location = new Point(20, 20),
                Size = new Size(580, 280),
                MaxPoints = 60, // 60秒分のデータ
                AutoScaleY = false,
                YMin = 0,
                YMax = 100,
                LineWidth = 1.5f,
                ShowGrid = true,
                GridColor = Color.FromArgb(40, 40, 40),
                RedrawIntervalMs = 100, // 0.1秒毎に更新
                BackColor = Color.FromArgb(13, 13, 13),
                FillArea = false, // 塗りつぶし無効（線のみ）
                PlotPadding = new Padding(5, 20, 5, 25)
            };

            this.Controls.Add(lineGraph);

            // 3つの系列を作成（異なる色）
            GraphSeries series1 = new GraphSeries("CPU", Color.FromArgb(0, 180, 200)); // 青緑
            GraphSeries series2 = new GraphSeries("メモリ", Color.FromArgb(255, 140, 0)); // オレンジ
            GraphSeries series3 = new GraphSeries("ディスク", Color.FromArgb(150, 200, 80)); // 黄緑

            lineGraph.AddSeries(series1);
            lineGraph.AddSeries(series2);
            lineGraph.AddSeries(series3);
        }

        private void StartDataSimulation()
        {
            // 0.1秒ごとにデータ追加
            dataTimer = new Timer();
            dataTimer.Interval = 100; // 0.1秒間隔
            dataTimer.Tick += DataTimer_Tick;
            dataTimer.Start();
        }

        private void DataTimer_Tick(object sender, EventArgs e)
        {
            // すべての系列にデータを追加（動的に追加された系列も含む）
            foreach (var series in lineGraph.Series)
            {
                float value;

                // 系列名に応じたデータパターンを生成
                if (series.Name.Contains("CPU"))
                {
                    // CPU使用率（波形パターン）
                    if (dataCounter < 150)
                    {
                        value = (float)(random.NextDouble() * 15 + 10);
                    }
                    else if (dataCounter >= 150 && dataCounter < 300)
                    {
                        value = (float)(random.NextDouble() * 25 + 40);
                    }
                    else
                    {
                        value = (float)(random.NextDouble() * 20 + 25);
                    }
                }
                else if (series.Name.Contains("メモリ"))
                {
                    // メモリ使用率（緩やかに上昇）
                    value = (float)(40 + Math.Sin(dataCounter * 0.015) * 20 + random.NextDouble() * 10);
                }
                else if (series.Name.Contains("ディスク"))
                {
                    // ディスク使用率（スパイク的な動き）
                    if (dataCounter % 100 < 20)
                    {
                        value = (float)(random.NextDouble() * 30 + 60); // スパイク
                    }
                    else
                    {
                        value = (float)(random.NextDouble() * 15 + 5); // 低い値
                    }
                }
                else
                {
                    // その他の系列（ランダム値）
                    value = (float)(random.NextDouble() * 50 + 25);
                }

                series.AddValue(value);
            }

            dataCounter++;
            lineGraph.RequestRedraw();
        }
    }
}
