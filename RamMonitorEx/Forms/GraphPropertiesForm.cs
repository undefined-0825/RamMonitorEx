using System;
using System.Drawing;
using System.Windows.Forms;
using RamMonitorEx.Controls;

namespace RamMonitorEx.Forms
{
    /// <summary>
    /// グラフプロパティ編集フォーム
    /// </summary>
    public class GraphPropertiesForm : Form
    {
        private LineGraphControl targetGraph;
        private TabControl tabControl;
        
        // 全般タブ
        private TabPage generalTab;
        private Label backColorLabel;
        private Button backColorButton;
        private Label gridColorLabel;
        private Button gridColorButton;
        private CheckBox showGridCheckBox;
        
        // 系列タブ
        private TabPage seriesTab;
        private ListBox seriesListBox;
        private Button addButton;
        private Button removeButton;
        
        // プロパティ編集コントロール
        private Panel propertyPanel;
        private Label nameLabel;
        private TextBox nameTextBox;
        private Label colorLabel;
        private Button colorButton;
        private CheckBox visibleCheckBox;
        
        // 共通
        private Button okButton;
        private Button cancelButton;
        private ColorDialog colorDialog;

        public GraphPropertiesForm(LineGraphControl graph)
        {
            targetGraph = graph;
            InitializeComponent();
            LoadGeneralSettings();
            LoadSeriesList();
        }

        private void InitializeComponent()
        {
            this.Text = "グラフプロパティ";
            this.Size = new Size(520, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterParent;

            // タブコントロール
            tabControl = new TabControl
            {
                Location = new Point(10, 10),
                Size = new Size(490, 370)
            };
            this.Controls.Add(tabControl);

            // 全般タブ
            InitializeGeneralTab();
            
            // 系列タブ
            InitializeSeriesTab();

            // OKボタン
            okButton = new Button
            {
                Text = "OK",
                Location = new Point(330, 390),
                Size = new Size(80, 25),
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(okButton);

            // キャンセルボタン
            cancelButton = new Button
            {
                Text = "キャンセル",
                Location = new Point(420, 390),
                Size = new Size(80, 25),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(cancelButton);

            // カラーダイアログ
            colorDialog = new ColorDialog();
        }

        private void InitializeGeneralTab()
        {
            generalTab = new TabPage("全般");
            tabControl.TabPages.Add(generalTab);

            // 背景色
            backColorLabel = new Label
            {
                Text = "背景色:",
                Location = new Point(20, 30),
                Size = new Size(100, 20)
            };
            generalTab.Controls.Add(backColorLabel);

            backColorButton = new Button
            {
                Location = new Point(130, 27),
                Size = new Size(200, 25),
                Text = "背景色を選択..."
            };
            backColorButton.Click += BackColorButton_Click;
            generalTab.Controls.Add(backColorButton);

            // グリッド線の色
            gridColorLabel = new Label
            {
                Text = "グリッド線の色:",
                Location = new Point(20, 70),
                Size = new Size(100, 20)
            };
            generalTab.Controls.Add(gridColorLabel);

            gridColorButton = new Button
            {
                Location = new Point(130, 67),
                Size = new Size(200, 25),
                Text = "グリッド色を選択..."
            };
            gridColorButton.Click += GridColorButton_Click;
            generalTab.Controls.Add(gridColorButton);

            // グリッド表示
            showGridCheckBox = new CheckBox
            {
                Text = "グリッド線を表示",
                Location = new Point(130, 110),
                Size = new Size(200, 20),
                Checked = true
            };
            showGridCheckBox.CheckedChanged += ShowGridCheckBox_CheckedChanged;
            generalTab.Controls.Add(showGridCheckBox);
        }

        private void InitializeSeriesTab()
        {
            seriesTab = new TabPage("系列");
            tabControl.TabPages.Add(seriesTab);

            // 系列リストボックス
            Label listLabel = new Label
            {
                Text = "折れ線一覧:",
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };
            seriesTab.Controls.Add(listLabel);

            seriesListBox = new ListBox
            {
                Location = new Point(10, 35),
                Size = new Size(200, 230),
                DisplayMember = "Name"
            };
            seriesListBox.SelectedIndexChanged += SeriesListBox_SelectedIndexChanged;
            seriesTab.Controls.Add(seriesListBox);

            // 追加ボタン
            addButton = new Button
            {
                Text = "追加",
                Location = new Point(10, 275),
                Size = new Size(95, 25)
            };
            addButton.Click += AddButton_Click;
            seriesTab.Controls.Add(addButton);

            // 削除ボタン
            removeButton = new Button
            {
                Text = "削除",
                Location = new Point(115, 275),
                Size = new Size(95, 25)
            };
            removeButton.Click += RemoveButton_Click;
            seriesTab.Controls.Add(removeButton);

            // プロパティパネル
            propertyPanel = new Panel
            {
                Location = new Point(220, 35),
                Size = new Size(250, 230),
                BorderStyle = BorderStyle.FixedSingle,
                Enabled = false
            };

            // 名前
            nameLabel = new Label
            {
                Text = "名前:",
                Location = new Point(10, 15),
                Size = new Size(80, 20)
            };
            propertyPanel.Controls.Add(nameLabel);

            nameTextBox = new TextBox
            {
                Location = new Point(90, 12),
                Size = new Size(140, 20)
            };
            nameTextBox.TextChanged += NameTextBox_TextChanged;
            propertyPanel.Controls.Add(nameTextBox);

            // 色
            colorLabel = new Label
            {
                Text = "色:",
                Location = new Point(10, 50),
                Size = new Size(80, 20)
            };
            propertyPanel.Controls.Add(colorLabel);

            colorButton = new Button
            {
                Location = new Point(90, 47),
                Size = new Size(140, 25),
                Text = "色を選択..."
            };
            colorButton.Click += ColorButton_Click;
            propertyPanel.Controls.Add(colorButton);

            // 表示/非表示
            visibleCheckBox = new CheckBox
            {
                Text = "表示",
                Location = new Point(90, 85),
                Size = new Size(140, 20),
                Checked = true
            };
            visibleCheckBox.CheckedChanged += VisibleCheckBox_CheckedChanged;
            propertyPanel.Controls.Add(visibleCheckBox);

            seriesTab.Controls.Add(propertyPanel);
        }

        private void LoadGeneralSettings()
        {
            backColorButton.BackColor = targetGraph.BackColor;
            gridColorButton.BackColor = targetGraph.GridColor;
            showGridCheckBox.Checked = targetGraph.ShowGrid;
        }

        private void LoadSeriesList()
        {
            seriesListBox.Items.Clear();
            foreach (var series in targetGraph.Series)
            {
                seriesListBox.Items.Add(series);
            }
        }

        #region 全般タブイベント

        private void BackColorButton_Click(object sender, EventArgs e)
        {
            colorDialog.Color = targetGraph.BackColor;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                targetGraph.BackColor = colorDialog.Color;
                backColorButton.BackColor = colorDialog.Color;
                targetGraph.RequestRedraw();
            }
        }

        private void GridColorButton_Click(object sender, EventArgs e)
        {
            colorDialog.Color = targetGraph.GridColor;
            
            if (colorDialog.ShowDialog() == DialogResult.OK)
            {
                targetGraph.GridColor = colorDialog.Color;
                gridColorButton.BackColor = colorDialog.Color;
                targetGraph.RequestRedraw();
            }
        }

        private void ShowGridCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            targetGraph.ShowGrid = showGridCheckBox.Checked;
            targetGraph.RequestRedraw();
        }

        #endregion

        #region 系列タブイベント

        private void SeriesListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (seriesListBox.SelectedItem != null)
            {
                GraphSeries selected = (GraphSeries)seriesListBox.SelectedItem;
                propertyPanel.Enabled = true;
                nameTextBox.Text = selected.Name;
                colorButton.BackColor = selected.Color;
                visibleCheckBox.Checked = selected.Visible;
            }
            else
            {
                propertyPanel.Enabled = false;
            }
        }

        private void AddButton_Click(object sender, EventArgs e)
        {
            // 新しい系列を追加
            int count = targetGraph.Series.Count + 1;
            Random rand = new Random(DateTime.Now.Millisecond);
            Color newColor = Color.FromArgb(
                rand.Next(100, 256),
                rand.Next(100, 256),
                rand.Next(100, 256)
            );
            
            GraphSeries newSeries = new GraphSeries($"系列{count}", newColor);
            
            // 既存系列と同じデータポイント数になるように初期データを追加
            int existingDataCount = 0;
            if (targetGraph.Series.Count > 0)
            {
                existingDataCount = targetGraph.Series[0].Values.Count;
            }
            
            // 既存のデータポイント数分、ダミーデータを追加
            for (int i = 0; i < existingDataCount; i++)
            {
                newSeries.AddValue((float)(rand.NextDouble() * 50 + 25));
            }
            
            targetGraph.AddSeries(newSeries);
            
            LoadSeriesList();
            seriesListBox.SelectedIndex = seriesListBox.Items.Count - 1;
        }

        private void RemoveButton_Click(object sender, EventArgs e)
        {
            if (seriesListBox.SelectedItem != null)
            {
                GraphSeries selected = (GraphSeries)seriesListBox.SelectedItem;
                
                DialogResult result = MessageBox.Show(
                    $"'{selected.Name}' を削除しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question
                );

                if (result == DialogResult.Yes)
                {
                    targetGraph.RemoveSeries(selected);
                    LoadSeriesList();
                }
            }
        }

        private void NameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (seriesListBox.SelectedItem != null)
            {
                GraphSeries selected = (GraphSeries)seriesListBox.SelectedItem;
                selected.Name = nameTextBox.Text;
                seriesListBox.Refresh();
            }
        }

        private void ColorButton_Click(object sender, EventArgs e)
        {
            if (seriesListBox.SelectedItem != null)
            {
                GraphSeries selected = (GraphSeries)seriesListBox.SelectedItem;
                colorDialog.Color = selected.Color;
                
                if (colorDialog.ShowDialog() == DialogResult.OK)
                {
                    selected.Color = colorDialog.Color;
                    colorButton.BackColor = colorDialog.Color;
                    targetGraph.RequestRedraw();
                }
            }
        }

        private void VisibleCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (seriesListBox.SelectedItem != null)
            {
                GraphSeries selected = (GraphSeries)seriesListBox.SelectedItem;
                selected.Visible = visibleCheckBox.Checked;
                targetGraph.RequestRedraw();
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                colorDialog?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}