using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using WeifenLuo.WinFormsUI.Docking;
using RamMonitorEx.Controls.RamMonitorView;
using RamMonitorEx.Controls.LineGraph;
using RamMonitorEx.Controls.MultiLayoutGrid;
using RamMonitorEx.Forms;
using WindowsFormsApp1.Serialization;
using WindowsFormsApp1.Docking;
using Timer = System.Windows.Forms.Timer;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private DockPanel _dockPanel;
        private MenuStrip _menuStrip;
        private WorkspaceSerializer _workspaceSerializer;
        private string? _currentWorkspaceFile;
        private WorkspaceConfig? _loadingWorkspaceConfig; // 読み込み中の設定を保持

        private LineGraphControl lineGraph;
        private Timer dataTimer;
        private Random random = new Random();
        private int dataCounter = 0;

        private bool _runtimeInitialized;

        public Form1()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (_runtimeInitialized)
            {
                return;
            }

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            _runtimeInitialized = true;
            _workspaceSerializer = new WorkspaceSerializer();
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

            // 新規RAMモニタ
            ToolStripMenuItem newRamMonitorItem = new ToolStripMenuItem("新規RAMモニタ(&R)...");
            newRamMonitorItem.ShortcutKeys = Keys.Control | Keys.M;
            newRamMonitorItem.Click += NewRamMonitorWithNameItem_Click;
            fileMenu.DropDownItems.Add(newRamMonitorItem);

            // 新規グラフ
            ToolStripMenuItem newGraphItem = new ToolStripMenuItem("新規グラフ(&G)...");
            newGraphItem.ShortcutKeys = Keys.Control | Keys.N;
            newGraphItem.Click += NewGraphWithNameItem_Click;
            fileMenu.DropDownItems.Add(newGraphItem);

            // 新規マルチレイアウトグリッド
            ToolStripMenuItem newMultiGridItem = new ToolStripMenuItem("新規マルチレイアウトグリッド(&L)...");
            newMultiGridItem.Click += NewMultiLayoutGridWithNameItem_Click;
            fileMenu.DropDownItems.Add(newMultiGridItem);

            fileMenu.DropDownItems.Add(new ToolStripSeparator());

            // ワークスペースの保存・読み込み
            ToolStripMenuItem saveAsItem = new ToolStripMenuItem("名前を付けて保存(&S)...");
            saveAsItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
            saveAsItem.Click += SaveAsItem_Click;
            fileMenu.DropDownItems.Add(saveAsItem);

            ToolStripMenuItem saveItem = new ToolStripMenuItem("上書き保存(&O)");
            saveItem.ShortcutKeys = Keys.Control | Keys.S;
            saveItem.Click += SaveItem_Click;
            fileMenu.DropDownItems.Add(saveItem);

            ToolStripMenuItem loadItem = new ToolStripMenuItem("開く(&L)...");
            loadItem.ShortcutKeys = Keys.Control | Keys.O;
            loadItem.Click += LoadItem_Click;
            fileMenu.DropDownItems.Add(loadItem);

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
        /// 名前を指定して新規グラフペイン追加メニュークリック
        /// </summary>
        private void NewGraphWithNameItem_Click(object sender, EventArgs e)
        {
            AddNewGraphPaneWithName();
        }

        /// <summary>
        /// 名前を指定して新規RAMモニタペイン追加メニュークリック
        /// </summary>
        private void NewRamMonitorWithNameItem_Click(object sender, EventArgs e)
        {
            AddNewRamMonitorPaneWithName();
        }

        /// <summary>
        /// 名前を指定して新規マルチレイアウトグリッドペイン追加メニュークリック
        /// </summary>
        private void NewMultiLayoutGridWithNameItem_Click(object sender, EventArgs e)
        {
            AddNewMultiLayoutGridPaneWithName();
        }

        /// <summary>
        /// 名前を指定して新しいグラフペインを追加
        /// </summary>
        private void AddNewGraphPaneWithName()
        {
            string defaultName = PaneNameManager.Instance.RegisterNewName("折れ線グラフパネル");
            PaneNameManager.Instance.UnregisterName(defaultName); // 一旦解除

            using (PaneNameDialog dialog = new PaneNameDialog(defaultName, "グラフパネル名を入力してください:"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (PaneNameManager.Instance.RegisterCustomName(dialog.PaneName))
                    {
                        GraphPane pane = new GraphPane(dialog.PaneName);
                        pane.Show(_dockPanel, DockState.Document);
                    }
                    else
                    {
                        MessageBox.Show("パネル名の登録に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// 名前を指定して新しいRAMモニタペインを追加
        /// </summary>
        private void AddNewRamMonitorPaneWithName()
        {
            string defaultName = PaneNameManager.Instance.RegisterNewName("RAMモニタパネル");
            PaneNameManager.Instance.UnregisterName(defaultName); // 一旦解除

            using (PaneNameDialog dialog = new PaneNameDialog(defaultName, "RAMモニタパネル名を入力してください:"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (PaneNameManager.Instance.RegisterCustomName(dialog.PaneName))
                    {
                        RamMonitorViewPane pane = new RamMonitorViewPane(dialog.PaneName);
                        pane.Show(_dockPanel, DockState.Document);
                    }
                    else
                    {
                        MessageBox.Show("パネル名の登録に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        /// <summary>
        /// 名前を指定して新しいマルチレイアウトグリッドペインを追加
        /// </summary>
        private void AddNewMultiLayoutGridPaneWithName()
        {
            string defaultName = PaneNameManager.Instance.RegisterNewName("マルチレイアウトグリッドパネル");
            PaneNameManager.Instance.UnregisterName(defaultName); // 一旦解除

            using (PaneNameDialog dialog = new PaneNameDialog(defaultName, "マルチレイアウトグリッドパネル名を入力してください:"))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (PaneNameManager.Instance.RegisterCustomName(dialog.PaneName))
                    {
                        MultiLayoutGridPane pane = new MultiLayoutGridPane(dialog.PaneName);
                        pane.Show(_dockPanel, DockState.Document);
                    }
                    else
                    {
                        MessageBox.Show("パネル名の登録に失敗しました。", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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

        // ========== ワークスペースの保存・読み込み ==========

        private void SaveAsItem_Click(object sender, EventArgs e)
        {
            SaveWorkspaceAs();
        }

        private void SaveItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(_currentWorkspaceFile))
            {
                SaveWorkspaceAs();
            }
            else
            {
                SaveWorkspace(_currentWorkspaceFile);
            }
        }

        private void LoadItem_Click(object sender, EventArgs e)
        {
            LoadWorkspace();
        }

        private void SaveWorkspaceAs()
        {
            using (SaveFileDialog dialog = new SaveFileDialog())
            {
                dialog.Filter = "ワークスペースファイル (*.xml)|*.xml|すべてのファイル (*.*)|*.*";
                dialog.DefaultExt = "xml";
                dialog.AddExtension = true;

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    SaveWorkspace(dialog.FileName);
                    _currentWorkspaceFile = dialog.FileName;
                }
            }
        }

        private void SaveWorkspace(string filePath)
        {
            try
            {
                WorkspaceConfig config = new WorkspaceConfig
                {
                    Name = Path.GetFileNameWithoutExtension(filePath)
                };

                config.DockPanelLayout = _workspaceSerializer.SaveDockPanelLayout(_dockPanel);

                foreach (var content in _dockPanel.Contents)
                {
                    if (content is GraphPane graphPane)
                    {
                        config.GraphPanes.Add(SaveGraphPaneConfig(graphPane));
                    }
                    else if (content is RamMonitorViewPane ramPane)
                    {
                        config.RamMonitorPanes.Add(SaveRamMonitorPaneConfig(ramPane));
                    }
                    else if (content is MultiLayoutGridPane multiPane)
                    {
                        config.MultiLayoutGridPanes.Add(SaveMultiLayoutGridPaneConfig(multiPane));
                    }
                }

                _workspaceSerializer.Save(config, filePath);
                MessageBox.Show($"ワークスペースを保存しました。\n{filePath}", "保存完了",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存中にエラーが発生しました。\n{ex.Message}", "エラー",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private MultiLayoutGridPaneConfig SaveMultiLayoutGridPaneConfig(MultiLayoutGridPane pane)
        {
            var config = new MultiLayoutGridPaneConfig
            {
                PaneName = pane.PaneName,
                GridConfig = new MultiLayoutGridConfig()
            };

            var grid = pane.GridControl;
            if (grid != null)
            {
                config.GridConfig.ShowGridLines = grid.ShowGridLines;
                config.GridConfig.GridLineColor = grid.GridLineColor;
                config.GridConfig.BackColor = grid.BackColor;
                config.GridConfig.ForeColor = grid.ForeColor;

                for (int rowIndex = 0; rowIndex < grid.Rows.Count; rowIndex++)
                {
                    var row = grid.Rows[rowIndex];
                    var rowConfig = new MultiLayoutGridRowConfig
                    {
                        RowIndex = rowIndex,
                        Height = row.Height
                    };

                    for (int cellIndex = 0; cellIndex < row.Cells.Count; cellIndex++)
                    {
                        var cell = row.Cells[cellIndex];
                        rowConfig.Cells.Add(new MultiLayoutGridCellConfig
                        {
                            CellIndex = cellIndex,
                            Text = cell.Text,
                            IsVariable = cell.ValueProvider != null,
                            Width = cell.Width,
                            HorizontalAlignment = (int)cell.HorizontalAlignment,
                            VerticalAlignment = (int)cell.VerticalAlignment,
                            HasForeColor = cell.ForeColor.HasValue,
                            ForeColorArgb = cell.ForeColor?.ToArgb() ?? Color.White.ToArgb()
                        });
                    }

                    config.GridConfig.Rows.Add(rowConfig);
                }
            }

            return config;
        }

        private GraphPaneConfig SaveGraphPaneConfig(GraphPane pane)
        {
            var config = new GraphPaneConfig
            {
                PaneName = pane.PaneName,
                GraphConfig = new LineGraphConfig()
            };

            var graph = pane.GraphControl;
            if (graph != null)
            {
                config.GraphConfig.MaxPoints = graph.MaxPoints;
                config.GraphConfig.AutoScaleY = graph.AutoScaleY;
                config.GraphConfig.YMin = graph.YMin;
                config.GraphConfig.YMax = graph.YMax;
                config.GraphConfig.LineWidth = graph.LineWidth;
                config.GraphConfig.ShowGrid = graph.ShowGrid;
                config.GraphConfig.FillArea = graph.FillArea;
                config.GraphConfig.RedrawIntervalMs = graph.RedrawIntervalMs;
                config.GraphConfig.BackColor = graph.BackColor;
                config.GraphConfig.GridColor = graph.GridColor;

                var padding = graph.PlotPadding;
                config.GraphConfig.PlotPaddingLeft = padding.Left;
                config.GraphConfig.PlotPaddingTop = padding.Top;
                config.GraphConfig.PlotPaddingRight = padding.Right;
                config.GraphConfig.PlotPaddingBottom = padding.Bottom;

                foreach (var series in graph.SeriesList)
                {
                    config.GraphConfig.Series.Add(new GraphSeriesConfig
                        {
                            Name = series.Name,
                            Color = series.Color,
                            Visible = series.Visible
                        });
                }
            }

            return config;
        }

        private RamMonitorPaneConfig SaveRamMonitorPaneConfig(RamMonitorViewPane pane)
        {
            var config = new RamMonitorPaneConfig
            {
                PaneName = pane.PaneName,
                ViewConfig = new RamMonitorViewConfig()
            };

            var view = pane.RamMonitorView;
            if (view != null)
            {
                config.ViewConfig.LabelColumnWidth = view.LabelColumnWidth;
                config.ViewConfig.ValueColumnWidth = view.ValueColumnWidth;
                config.ViewConfig.UnitColumnWidth = view.UnitColumnWidth;
                config.ViewConfig.RowHeight = view.RowHeight;
                config.ViewConfig.HeaderVisible = view.HeaderVisible;
                config.ViewConfig.GridLineVisible = view.GridLineVisible;
                config.ViewConfig.ValueAlignment = (int)view.ValueAlignment;
                config.ViewConfig.BackColor = view.BackColor;
                config.ViewConfig.ForeColor = view.ForeColor;
                config.ViewConfig.LabelForeColor = view.LabelForeColor;
                config.ViewConfig.ValueForeColor = view.ValueForeColor;
                config.ViewConfig.UnitForeColor = view.UnitForeColor;
                config.ViewConfig.CommentForeColor = view.CommentForeColor;
                config.ViewConfig.CommentBackColor = view.CommentBackColor;
                config.ViewConfig.GridLineColor = view.GridLineColor;
                config.ViewConfig.ColumnResizeEnabled = view.ColumnResizeEnabled;
                config.ViewConfig.PanEnabled = view.PanEnabled;

                for (int i = 0; i < view.Rows.Count; i++)
                {
                    var row = view.Rows[i];
                    if (row is ValueDataRow dataRow)
                    {
                        config.ViewConfig.DataRows.Add(new DataRowConfig
                        {
                            RowIndex = i,
                            LabelText = dataRow.LabelText,
                            UnitText = dataRow.UnitText
                        });
                    }
                    else if (row is ValueCommentRow commentRow)
                    {
                        config.ViewConfig.CommentRows.Add(new CommentRowConfig
                        {
                            RowIndex = i,
                            CommentText = commentRow.CommentText
                        });
                    }
                }
            }

            return config;
        }

        private void LoadWorkspace()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "ワークスペースファイル (*.xml)|*.xml|すべてのファイル (*.*)|*.*";
                dialog.DefaultExt = "xml";

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        CloseAllPanes();
                        _loadingWorkspaceConfig = _workspaceSerializer.Load(dialog.FileName);
                        _workspaceSerializer.LoadDockPanelLayout(_dockPanel, _loadingWorkspaceConfig.DockPanelLayout,
                            GetContentFromPersistString);

                        _currentWorkspaceFile = dialog.FileName;
                        _loadingWorkspaceConfig = null;

                        MessageBox.Show($"ワークスペースを読み込みました。\n{dialog.FileName}", "読み込み完了",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        _loadingWorkspaceConfig = null;
                        MessageBox.Show($"読み込み中にエラーが発生しました。\n{ex.Message}", "エラー",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void CloseAllPanes()
        {
            var contents = _dockPanel.Contents.ToList();
            foreach (var content in contents)
            {
                if (content is GraphPane || content is RamMonitorViewPane || content is MultiLayoutGridPane)
                {
                    content.DockHandler.Close();
                }
            }
        }

        private IDockContent GetContentFromPersistString(string persistString)
        {
            string[] parts = persistString.Split('|');
            if (parts.Length != 2)
            {
                return null;
            }

            string type = parts[0];
            string paneName = parts[1];

            if (_loadingWorkspaceConfig == null)
            {
                return null;
            }

            if (type == "GraphPane")
            {
                var paneConfig = _loadingWorkspaceConfig.GraphPanes.Find(p => p.PaneName == paneName);
                if (paneConfig != null && PaneNameManager.Instance.RegisterCustomName(paneName))
                {
                    var pane = new GraphPane(paneName);
                    RestoreGraphPaneConfig(pane, paneConfig);
                    return pane;
                }
            }
            else if (type == "RamMonitorViewPane")
            {
                var paneConfig = _loadingWorkspaceConfig.RamMonitorPanes.Find(p => p.PaneName == paneName);
                if (paneConfig != null && PaneNameManager.Instance.RegisterCustomName(paneName))
                {
                    var pane = new RamMonitorViewPane(paneName);
                    RestoreRamMonitorPaneConfig(pane, paneConfig);
                    return pane;
                }
            }
            else if (type == "MultiLayoutGridPane")
            {
                var paneConfig = _loadingWorkspaceConfig.MultiLayoutGridPanes.Find(p => p.PaneName == paneName);
                if (PaneNameManager.Instance.RegisterCustomName(paneName))
                {
                    var pane = new MultiLayoutGridPane(paneName);
                    if (paneConfig != null)
                    {
                        RestoreMultiLayoutGridPaneConfig(pane, paneConfig);
                    }
                    return pane;
                }
            }

            return null;
        }

        private void RestoreMultiLayoutGridPaneConfig(MultiLayoutGridPane pane, MultiLayoutGridPaneConfig config)
        {
            var grid = pane.GridControl;
            if (grid == null) return;

            var cfg = config.GridConfig;
            grid.ShowGridLines = cfg.ShowGridLines;
            grid.GridLineColor = cfg.GridLineColor;
            grid.BackColor = cfg.BackColor;
            grid.ForeColor = cfg.ForeColor;

            grid.Rows.Clear();

            foreach (var rowCfg in cfg.Rows.OrderBy(r => r.RowIndex))
            {
                var row = new GridRow { Height = rowCfg.Height };
                foreach (var cellCfg in rowCfg.Cells.OrderBy(c => c.CellIndex))
                {
                    var cell = new GridCell
                    {
                        Text = cellCfg.Text,
                        Width = cellCfg.Width,
                        HorizontalAlignment = (ContentAlignment)cellCfg.HorizontalAlignment,
                        VerticalAlignment = (ContentAlignment)cellCfg.VerticalAlignment,
                        ForeColor = cellCfg.HasForeColor ? Color.FromArgb(cellCfg.ForeColorArgb) : null
                    };

                    if (cellCfg.IsVariable)
                    {
                        Random rand = new Random();
                        cell.ValueProvider = () => rand.Next(0, 100).ToString();
                    }

                    row.Cells.Add(cell);
                }
                grid.Rows.Add(row);
            }

            grid.RequestRedraw();
        }

        private void RestoreGraphPaneConfig(GraphPane pane, GraphPaneConfig config)
        {
            var graph = pane.GraphControl;
            if (graph == null) return;

            var cfg = config.GraphConfig;
            graph.MaxPoints = cfg.MaxPoints;
            graph.AutoScaleY = cfg.AutoScaleY;
            graph.YMin = (float)cfg.YMin;
            graph.YMax = (float)cfg.YMax;
            graph.LineWidth = cfg.LineWidth;
            graph.ShowGrid = cfg.ShowGrid;
            graph.FillArea = cfg.FillArea;
            graph.RedrawIntervalMs = cfg.RedrawIntervalMs;
            graph.BackColor = cfg.BackColor;
            graph.GridColor = cfg.GridColor;
            graph.PlotPadding = new Padding(cfg.PlotPaddingLeft, cfg.PlotPaddingTop,
                cfg.PlotPaddingRight, cfg.PlotPaddingBottom);

            // 既存の系列をクリア
            graph.ClearSeries();

            // 系列を復元
            foreach (var seriesConfig in cfg.Series)
            {
                var series = new GraphSeries(seriesConfig.Name, seriesConfig.Color)
                {
                    Visible = seriesConfig.Visible
                };
                graph.AddSeries(series);
            }
        }

        private void RestoreRamMonitorPaneConfig(RamMonitorViewPane pane, RamMonitorPaneConfig config)
        {
            var view = pane.RamMonitorView;
            if (view == null) return;

            var cfg = config.ViewConfig;

            // レイアウト設定を復元
            view.LabelColumnWidth = cfg.LabelColumnWidth;
            view.ValueColumnWidth = cfg.ValueColumnWidth;
            view.UnitColumnWidth = cfg.UnitColumnWidth;
            view.RowHeight = cfg.RowHeight;
            view.HeaderVisible = cfg.HeaderVisible;
            view.GridLineVisible = cfg.GridLineVisible;

            // 表示設定を復元
            view.ValueAlignment = (ValueTextAlignment)cfg.ValueAlignment;
            view.BackColor = cfg.BackColor;
            view.ForeColor = cfg.ForeColor;
            view.LabelForeColor = cfg.LabelForeColor;
            view.ValueForeColor = cfg.ValueForeColor;
            view.UnitForeColor = cfg.UnitForeColor;
            view.CommentForeColor = cfg.CommentForeColor;
            view.CommentBackColor = cfg.CommentBackColor;
            view.GridLineColor = cfg.GridLineColor;

            // 動作設定を復元
            view.ColumnResizeEnabled = cfg.ColumnResizeEnabled;
            view.PanEnabled = cfg.PanEnabled;

            // 既存の行をクリア
            view.ClearRows();

            // 行データを復元（行インデックス順にソート）
            var allRows = new List<(int index, IValueViewRow row)>();

            foreach (var dataRowConfig in cfg.DataRows)
            {
                allRows.Add((dataRowConfig.RowIndex, new ValueDataRow(
                    dataRowConfig.LabelText,
                    "0", // ValueTextは復元時は初期値
                    dataRowConfig.UnitText
                )));
            }

            foreach (var commentRowConfig in cfg.CommentRows)
            {
                allRows.Add((commentRowConfig.RowIndex, new ValueCommentRow(
                    commentRowConfig.CommentText
                )));
            }

            // インデックス順に並べ替えて追加
            foreach (var (index, row) in allRows.OrderBy(x => x.index))
            {
                if (row is ValueDataRow dataRow)
                {
                    view.AddDataRow(dataRow.LabelText, dataRow.ValueText, dataRow.UnitText);
                }
                else if (row is ValueCommentRow commentRow)
                {
                    view.AddCommentRow(commentRow.CommentText);
                }
            }
        }
    }
}
