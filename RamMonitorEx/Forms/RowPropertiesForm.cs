using System;
using System.Drawing;
using System.Windows.Forms;
using RamMonitorEx.Controls.MultiLayoutGrid;

namespace RamMonitorEx.Forms
{
    /// <summary>
    /// 行のプロパティ（列の設定）を行うフォーム
    /// </summary>
    public class RowPropertiesForm : Form
    {
        private GridRow _row;
        private ListBox _cellListBox;
        private Button _addCellButton;
        private Button _removeCellButton;
        private Button _editCellButton;
        private Button _moveLeftButton;
        private Button _moveRightButton;
        private Button _okButton;
        private Button _cancelButton;

        public RowPropertiesForm(GridRow row)
        {
            _row = row ?? throw new ArgumentNullException(nameof(row));

            InitializeComponents();
            LoadCells();
        }

        private void InitializeComponents()
        {
            this.Text = "行プロパティ - 列の設定";
            this.Size = new Size(500, 400);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // セルリストボックス
            Label cellLabel = new Label
            {
                Text = "列（セル）:",
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };
            this.Controls.Add(cellLabel);

            _cellListBox = new ListBox
            {
                Location = new Point(10, 35),
                Size = new Size(350, 250),
                DisplayMember = "Display"
            };
            _cellListBox.SelectedIndexChanged += CellListBox_SelectedIndexChanged;
            this.Controls.Add(_cellListBox);

            // セル操作ボタン
            _addCellButton = new Button
            {
                Text = "列を追加",
                Location = new Point(370, 35),
                Size = new Size(100, 30)
            };
            _addCellButton.Click += AddCellButton_Click;
            this.Controls.Add(_addCellButton);

            _removeCellButton = new Button
            {
                Text = "列を削除",
                Location = new Point(370, 70),
                Size = new Size(100, 30)
            };
            _removeCellButton.Click += RemoveCellButton_Click;
            this.Controls.Add(_removeCellButton);

            _editCellButton = new Button
            {
                Text = "列を編集",
                Location = new Point(370, 105),
                Size = new Size(100, 30)
            };
            _editCellButton.Click += EditCellButton_Click;
            this.Controls.Add(_editCellButton);

            _moveLeftButton = new Button
            {
                Text = "←左へ",
                Location = new Point(370, 150),
                Size = new Size(100, 30)
            };
            _moveLeftButton.Click += MoveLeftButton_Click;
            this.Controls.Add(_moveLeftButton);

            _moveRightButton = new Button
            {
                Text = "→右へ",
                Location = new Point(370, 185),
                Size = new Size(100, 30)
            };
            _moveRightButton.Click += MoveRightButton_Click;
            this.Controls.Add(_moveRightButton);

            // OKキャンセルボタン
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(270, 310),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "キャンセル",
                Location = new Point(370, 310),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadCells()
        {
            _cellListBox.Items.Clear();

            for (int i = 0; i < _row.Cells.Count; i++)
            {
                var cell = _row.Cells[i];
                string type = cell.ValueProvider != null ? "可変" : "固定";
                string colorInfo = cell.ForeColor.HasValue ? $", 色: {cell.ForeColor.Value.Name}" : "";
                _cellListBox.Items.Add(new CellListItem
                {
                    Index = i,
                    Cell = cell,
                    Display = $"列 {i + 1}: [{type}] {cell.Text} (幅: {cell.Width}px{colorInfo})"
                });
            }

            UpdateButtonStates();
        }

        private void CellListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _cellListBox.SelectedIndex >= 0;
            _removeCellButton.Enabled = hasSelection;
            _editCellButton.Enabled = hasSelection;
            _moveLeftButton.Enabled = hasSelection && _cellListBox.SelectedIndex > 0;
            _moveRightButton.Enabled = hasSelection && _cellListBox.SelectedIndex < _cellListBox.Items.Count - 1;
        }

        private void AddCellButton_Click(object? sender, EventArgs e)
        {
            GridCell newCell = new GridCell
            {
                Text = "新しい列",
                Width = 100,
                HorizontalAlignment = ContentAlignment.MiddleLeft
            };

            using (CellPropertiesForm form = new CellPropertiesForm(newCell))
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    _row.Cells.Add(newCell);
                    LoadCells();
                    _cellListBox.SelectedIndex = _cellListBox.Items.Count - 1;
                }
            }
        }

        private void RemoveCellButton_Click(object? sender, EventArgs e)
        {
            if (_cellListBox.SelectedItem is CellListItem item)
            {
                DialogResult result = MessageBox.Show(
                    $"列 {item.Index + 1} を削除しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _row.Cells.RemoveAt(item.Index);
                    LoadCells();
                }
            }
        }

        private void EditCellButton_Click(object? sender, EventArgs e)
        {
            if (_cellListBox.SelectedItem is CellListItem item)
            {
                using (CellPropertiesForm form = new CellPropertiesForm(item.Cell))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadCells();
                        _cellListBox.SelectedIndex = item.Index;
                    }
                }
            }
        }

        private void MoveLeftButton_Click(object? sender, EventArgs e)
        {
            int index = _cellListBox.SelectedIndex;
            if (index > 0)
            {
                var cell = _row.Cells[index];
                _row.Cells.RemoveAt(index);
                _row.Cells.Insert(index - 1, cell);
                LoadCells();
                _cellListBox.SelectedIndex = index - 1;
            }
        }

        private void MoveRightButton_Click(object? sender, EventArgs e)
        {
            int index = _cellListBox.SelectedIndex;
            if (index < _row.Cells.Count - 1)
            {
                var cell = _row.Cells[index];
                _row.Cells.RemoveAt(index);
                _row.Cells.Insert(index + 1, cell);
                LoadCells();
                _cellListBox.SelectedIndex = index + 1;
            }
        }

        private class CellListItem
        {
            public int Index { get; set; }
            public GridCell Cell { get; set; } = null!;
            public string Display { get; set; } = string.Empty;
        }
    }
}
