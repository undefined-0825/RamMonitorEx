using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using RamMonitorEx.Controls.MultiLayoutGrid;

namespace RamMonitorEx.Forms
{
    /// <summary>
    /// マルチレイアウトグリッドのプロパティ設定フォーム
    /// </summary>
    public class MultiLayoutGridPropertiesForm : Form
    {
        private MultiLayoutGridControl _gridControl;
        private ListBox _rowListBox;
        private Button _addRowButton;
        private Button _removeRowButton;
        private Button _editRowButton;
        private Button _moveUpButton;
        private Button _moveDownButton;
        private NumericUpDown _rowHeightNumeric;
        private Button _okButton;
        private Button _cancelButton;

        public MultiLayoutGridPropertiesForm(MultiLayoutGridControl gridControl)
        {
            _gridControl = gridControl ?? throw new ArgumentNullException(nameof(gridControl));

            InitializeComponents();
            LoadRows();
        }

        private void InitializeComponents()
        {
            this.Text = "マルチレイアウトグリッド プロパティ";
            this.Size = new Size(500, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            // 行リストボックス
            Label rowLabel = new Label
            {
                Text = "行:",
                Location = new Point(10, 10),
                Size = new Size(100, 20)
            };
            this.Controls.Add(rowLabel);

            _rowListBox = new ListBox
            {
                Location = new Point(10, 35),
                Size = new Size(350, 250),
                DisplayMember = "Display"
            };
            _rowListBox.SelectedIndexChanged += RowListBox_SelectedIndexChanged;
            this.Controls.Add(_rowListBox);

            // 行操作ボタン
            _addRowButton = new Button
            {
                Text = "行を追加",
                Location = new Point(370, 35),
                Size = new Size(100, 30)
            };
            _addRowButton.Click += AddRowButton_Click;
            this.Controls.Add(_addRowButton);

            _removeRowButton = new Button
            {
                Text = "行を削除",
                Location = new Point(370, 70),
                Size = new Size(100, 30)
            };
            _removeRowButton.Click += RemoveRowButton_Click;
            this.Controls.Add(_removeRowButton);

            _editRowButton = new Button
            {
                Text = "行を編集",
                Location = new Point(370, 105),
                Size = new Size(100, 30)
            };
            _editRowButton.Click += EditRowButton_Click;
            this.Controls.Add(_editRowButton);

            _moveUpButton = new Button
            {
                Text = "↑上へ",
                Location = new Point(370, 150),
                Size = new Size(100, 30)
            };
            _moveUpButton.Click += MoveUpButton_Click;
            this.Controls.Add(_moveUpButton);

            _moveDownButton = new Button
            {
                Text = "↓下へ",
                Location = new Point(370, 185),
                Size = new Size(100, 30)
            };
            _moveDownButton.Click += MoveDownButton_Click;
            this.Controls.Add(_moveDownButton);

            // 行の高さ設定
            Label heightLabel = new Label
            {
                Text = "選択行の高さ:",
                Location = new Point(10, 295),
                Size = new Size(100, 20)
            };
            this.Controls.Add(heightLabel);

            _rowHeightNumeric = new NumericUpDown
            {
                Location = new Point(120, 293),
                Size = new Size(80, 20),
                Minimum = 10,
                Maximum = 200,
                Value = 30
            };
            _rowHeightNumeric.ValueChanged += RowHeightNumeric_ValueChanged;
            this.Controls.Add(_rowHeightNumeric);

            // OKキャンセルボタン
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(270, 370),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK
            };
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "キャンセル",
                Location = new Point(370, 370),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;
        }

        private void LoadRows()
        {
            _rowListBox.Items.Clear();

            for (int i = 0; i < _gridControl.Rows.Count; i++)
            {
                var row = _gridControl.Rows[i];
                _rowListBox.Items.Add(new RowListItem
                {
                    Index = i,
                    Row = row,
                    Display = $"行 {i + 1} (高さ: {row.Height}px, セル数: {row.Cells.Count})"
                });
            }

            UpdateButtonStates();
        }

        private void RowListBox_SelectedIndexChanged(object? sender, EventArgs e)
        {
            UpdateButtonStates();

            if (_rowListBox.SelectedItem is RowListItem item)
            {
                _rowHeightNumeric.Value = item.Row.Height;
            }
        }

        private void UpdateButtonStates()
        {
            bool hasSelection = _rowListBox.SelectedIndex >= 0;
            _removeRowButton.Enabled = hasSelection;
            _editRowButton.Enabled = hasSelection;
            _moveUpButton.Enabled = hasSelection && _rowListBox.SelectedIndex > 0;
            _moveDownButton.Enabled = hasSelection && _rowListBox.SelectedIndex < _rowListBox.Items.Count - 1;
        }

        private void AddRowButton_Click(object? sender, EventArgs e)
        {
            GridRow newRow = new GridRow { Height = 30 };
            newRow.Cells.Add(new GridCell { Text = "新しいセル", Width = 100 });

            _gridControl.Rows.Add(newRow);
            LoadRows();
            _rowListBox.SelectedIndex = _rowListBox.Items.Count - 1;
        }

        private void RemoveRowButton_Click(object? sender, EventArgs e)
        {
            if (_rowListBox.SelectedItem is RowListItem item)
            {
                DialogResult result = MessageBox.Show(
                    $"行 {item.Index + 1} を削除しますか？",
                    "確認",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    _gridControl.Rows.RemoveAt(item.Index);
                    LoadRows();
                }
            }
        }

        private void EditRowButton_Click(object? sender, EventArgs e)
        {
            if (_rowListBox.SelectedItem is RowListItem item)
            {
                using (RowPropertiesForm form = new RowPropertiesForm(item.Row))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadRows();
                        _rowListBox.SelectedIndex = item.Index;
                    }
                }
            }
        }

        private void MoveUpButton_Click(object? sender, EventArgs e)
        {
            int index = _rowListBox.SelectedIndex;
            if (index > 0)
            {
                var row = _gridControl.Rows[index];
                _gridControl.Rows.RemoveAt(index);
                _gridControl.Rows.Insert(index - 1, row);
                LoadRows();
                _rowListBox.SelectedIndex = index - 1;
            }
        }

        private void MoveDownButton_Click(object? sender, EventArgs e)
        {
            int index = _rowListBox.SelectedIndex;
            if (index < _gridControl.Rows.Count - 1)
            {
                var row = _gridControl.Rows[index];
                _gridControl.Rows.RemoveAt(index);
                _gridControl.Rows.Insert(index + 1, row);
                LoadRows();
                _rowListBox.SelectedIndex = index + 1;
            }
        }

        private void RowHeightNumeric_ValueChanged(object? sender, EventArgs e)
        {
            if (_rowListBox.SelectedItem is RowListItem item)
            {
                item.Row.Height = (int)_rowHeightNumeric.Value;
                LoadRows();
                _rowListBox.SelectedIndex = item.Index;
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (this.DialogResult == DialogResult.OK)
            {
                _gridControl.RequestRedraw();
            }
        }

        private class RowListItem
        {
            public int Index { get; set; }
            public GridRow Row { get; set; } = null!;
            public string Display { get; set; } = string.Empty;
        }
    }
}
