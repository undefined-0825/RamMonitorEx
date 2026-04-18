using System;
using System.Drawing;
using System.Windows.Forms;
using RamMonitorEx.Controls.MultiLayoutGrid;

namespace RamMonitorEx.Forms
{
    /// <summary>
    /// セル（列）のプロパティを設定するフォーム
    /// </summary>
    public class CellPropertiesForm : Form
    {
        private GridCell _cell;
        private TextBox _textTextBox;
        private RadioButton _fixedRadio;
        private RadioButton _variableRadio;
        private NumericUpDown _widthNumeric;
        private ComboBox _hAlignCombo;
        private ComboBox _vAlignCombo;
        private CheckBox _customColorCheckBox;
        private Panel _colorPanel;
        private Button _colorButton;
        private Button _okButton;
        private Button _cancelButton;

        public CellPropertiesForm(GridCell cell)
        {
            _cell = cell ?? throw new ArgumentNullException(nameof(cell));

            InitializeComponents();
            LoadSettings();
        }

        private void InitializeComponents()
        {
            this.Text = "列（セル）プロパティ";
            this.Size = new Size(450, 450);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int yPos = 10;

            // テキスト
            Label textLabel = new Label
            {
                Text = "テキスト:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            this.Controls.Add(textLabel);

            _textTextBox = new TextBox
            {
                Location = new Point(120, yPos),
                Size = new Size(300, 20)
            };
            this.Controls.Add(_textTextBox);

            yPos += 30;

            // 値のタイプ
            Label typeLabel = new Label
            {
                Text = "値のタイプ:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            this.Controls.Add(typeLabel);

            _fixedRadio = new RadioButton
            {
                Text = "固定文字列",
                Location = new Point(120, yPos),
                Size = new Size(100, 20),
                Checked = true
            };
            _fixedRadio.CheckedChanged += TypeRadio_CheckedChanged;
            this.Controls.Add(_fixedRadio);

            _variableRadio = new RadioButton
            {
                Text = "可変値（実行時取得）",
                Location = new Point(230, yPos),
                Size = new Size(180, 20)
            };
            _variableRadio.CheckedChanged += TypeRadio_CheckedChanged;
            this.Controls.Add(_variableRadio);

            yPos += 40;

            Label noteLabel = new Label
            {
                Text = "※可変値の場合、表示テキストは実行時に動的に更新されます",
                Location = new Point(10, yPos),
                Size = new Size(400, 20),
                ForeColor = Color.Gray,
                Font = new Font(this.Font.FontFamily, 8)
            };
            this.Controls.Add(noteLabel);

            yPos += 30;

            // 幅
            Label widthLabel = new Label
            {
                Text = "幅 (px):",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            this.Controls.Add(widthLabel);

            _widthNumeric = new NumericUpDown
            {
                Location = new Point(120, yPos),
                Size = new Size(100, 20),
                Minimum = 30,
                Maximum = 500,
                Value = 100
            };
            this.Controls.Add(_widthNumeric);

            yPos += 35;

            // 水平配置
            Label hAlignLabel = new Label
            {
                Text = "水平配置:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            this.Controls.Add(hAlignLabel);

            _hAlignCombo = new ComboBox
            {
                Location = new Point(120, yPos),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _hAlignCombo.Items.AddRange(new object[]
            {
                new AlignmentItem { Text = "左揃え", Value = ContentAlignment.MiddleLeft },
                new AlignmentItem { Text = "中央揃え", Value = ContentAlignment.MiddleCenter },
                new AlignmentItem { Text = "右揃え", Value = ContentAlignment.MiddleRight }
            });
            _hAlignCombo.DisplayMember = "Text";
            _hAlignCombo.SelectedIndex = 0;
            this.Controls.Add(_hAlignCombo);

            yPos += 35;

            // 垂直配置
            Label vAlignLabel = new Label
            {
                Text = "垂直配置:",
                Location = new Point(10, yPos),
                Size = new Size(100, 20)
            };
            this.Controls.Add(vAlignLabel);

            _vAlignCombo = new ComboBox
            {
                Location = new Point(120, yPos),
                Size = new Size(150, 20),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            _vAlignCombo.Items.AddRange(new object[]
            {
                new AlignmentItem { Text = "上揃え", Value = ContentAlignment.TopCenter },
                new AlignmentItem { Text = "中央揃え", Value = ContentAlignment.MiddleCenter },
                new AlignmentItem { Text = "下揃え", Value = ContentAlignment.BottomCenter }
            });
            _vAlignCombo.DisplayMember = "Text";
            _vAlignCombo.SelectedIndex = 1;
            this.Controls.Add(_vAlignCombo);

            yPos += 35;

            // カスタム文字色
            _customColorCheckBox = new CheckBox
            {
                Text = "カスタム文字色を使用:",
                Location = new Point(10, yPos),
                Size = new Size(180, 20)
            };
            _customColorCheckBox.CheckedChanged += CustomColorCheckBox_CheckedChanged;
            this.Controls.Add(_customColorCheckBox);

            yPos += 30;

            _colorPanel = new Panel
            {
                Location = new Point(30, yPos),
                Size = new Size(50, 30),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(_colorPanel);

            _colorButton = new Button
            {
                Text = "色を選択...",
                Location = new Point(90, yPos),
                Size = new Size(100, 30)
            };
            _colorButton.Click += ColorButton_Click;
            this.Controls.Add(_colorButton);

            yPos += 50;

            // OKキャンセルボタン
            _okButton = new Button
            {
                Text = "OK",
                Location = new Point(230, yPos),
                Size = new Size(90, 30),
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;
            this.Controls.Add(_okButton);

            _cancelButton = new Button
            {
                Text = "キャンセル",
                Location = new Point(330, yPos),
                Size = new Size(90, 30),
                DialogResult = DialogResult.Cancel
            };
            this.Controls.Add(_cancelButton);

            this.AcceptButton = _okButton;
            this.CancelButton = _cancelButton;

            UpdateColorControlsState();
        }

        private void LoadSettings()
        {
            _textTextBox.Text = _cell.Text;
            _widthNumeric.Value = _cell.Width;

            _fixedRadio.Checked = _cell.ValueProvider == null;
            _variableRadio.Checked = _cell.ValueProvider != null;

            // 水平配置
            for (int i = 0; i < _hAlignCombo.Items.Count; i++)
            {
                if ((_hAlignCombo.Items[i] as AlignmentItem)?.Value == _cell.HorizontalAlignment)
                {
                    _hAlignCombo.SelectedIndex = i;
                    break;
                }
            }

            // 垂直配置
            for (int i = 0; i < _vAlignCombo.Items.Count; i++)
            {
                if ((_vAlignCombo.Items[i] as AlignmentItem)?.Value == _cell.VerticalAlignment)
                {
                    _vAlignCombo.SelectedIndex = i;
                    break;
                }
            }

            // カスタム色
            if (_cell.ForeColor.HasValue)
            {
                _customColorCheckBox.Checked = true;
                _colorPanel.BackColor = _cell.ForeColor.Value;
            }

            UpdateColorControlsState();
        }

        private void TypeRadio_CheckedChanged(object? sender, EventArgs e)
        {
            // 可変値の場合、テキストは表示用のプレースホルダー
            if (_variableRadio.Checked)
            {
                _textTextBox.Enabled = false;
            }
            else
            {
                _textTextBox.Enabled = true;
            }
        }

        private void CustomColorCheckBox_CheckedChanged(object? sender, EventArgs e)
        {
            UpdateColorControlsState();
        }

        private void UpdateColorControlsState()
        {
            bool enabled = _customColorCheckBox.Checked;
            _colorPanel.Enabled = enabled;
            _colorButton.Enabled = enabled;
        }

        private void ColorButton_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = _colorPanel.BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _colorPanel.BackColor = dialog.Color;
                }
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            // 設定を適用
            _cell.Text = _textTextBox.Text;
            _cell.Width = (int)_widthNumeric.Value;

            if (_hAlignCombo.SelectedItem is AlignmentItem hAlign)
            {
                _cell.HorizontalAlignment = hAlign.Value;
            }

            if (_vAlignCombo.SelectedItem is AlignmentItem vAlign)
            {
                _cell.VerticalAlignment = vAlign.Value;
            }

            // 可変値の設定
            if (_variableRadio.Checked)
            {
                // 既存のValueProviderを保持（新規の場合はダミー関数を設定）
                if (_cell.ValueProvider == null)
                {
                    Random rand = new Random();
                    _cell.ValueProvider = () => rand.Next(0, 100).ToString();
                }
            }
            else
            {
                _cell.ValueProvider = null;
            }

            // カスタム色
            if (_customColorCheckBox.Checked)
            {
                _cell.ForeColor = _colorPanel.BackColor;
            }
            else
            {
                _cell.ForeColor = null;
            }
        }

        private class AlignmentItem
        {
            public string Text { get; set; } = string.Empty;
            public ContentAlignment Value { get; set; }
        }
    }
}
