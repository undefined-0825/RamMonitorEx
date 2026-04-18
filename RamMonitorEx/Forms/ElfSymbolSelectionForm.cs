using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WindowsFormsApp1.ReadElf;

namespace RamMonitorEx.Forms
{
    /// <summary>
    /// ELFシンボル一覧を表示し、選択結果を返却するダイアログ
    /// </summary>
    public class ElfSymbolSelectionForm : Form
    {
        private readonly ReadElfParser _parser = new ReadElfParser();
        private readonly DataSet _dataSet = new DataSet("ElfSymbolsDataSet");
        private readonly DataTable _symbolTable = new DataTable("Symbols");
        private readonly BindingSource _bindingSource = new BindingSource();

        private TextBox _filePathTextBox = null!;
        private Button _browseButton = null!;
        private TextBox _filterTextBox = null!;
        private LinkLabel _selectAllLinkLabel = null!;
        private LinkLabel _unselectAllLinkLabel = null!;
        private DataGridView _grid = null!;
        private Label _statusLabel = null!;
        private Button _okButton = null!;
        private Button _cancelButton = null!;

        public List<ElfSymbolInfo> SelectedSymbols { get; } = new List<ElfSymbolInfo>();

        public string SelectedElfFilePath { get; private set; } = string.Empty;

        public ElfSymbolSelectionForm()
        {
            InitializeDataSet();
            InitializeComponents();
        }

        private void InitializeDataSet()
        {
            _symbolTable.Columns.Add("Select", typeof(bool));
            _symbolTable.Columns.Add("Name", typeof(string));
            _symbolTable.Columns.Add("Address", typeof(ulong));
            _symbolTable.Columns.Add("AddressHex", typeof(string));
            _symbolTable.Columns.Add("Size", typeof(ulong));
            _symbolTable.Columns.Add("SourceTable", typeof(string));

            _dataSet.Tables.Add(_symbolTable);

            DataView view = new DataView(_symbolTable);
            _bindingSource.DataSource = view;
        }

        private void InitializeComponents()
        {
            Text = "ELFシンボル選択";
            StartPosition = FormStartPosition.CenterParent;
            MinimumSize = new Size(900, 600);
            Size = new Size(1000, 700);

            Label fileLabel = new Label
            {
                Text = "ELFファイル:",
                AutoSize = true,
                Location = new Point(12, 15)
            };

            _filePathTextBox = new TextBox
            {
                Location = new Point(90, 12),
                Width = 760,
                ReadOnly = true
            };

            _browseButton = new Button
            {
                Text = "参照...",
                Location = new Point(860, 10),
                Size = new Size(110, 26)
            };
            _browseButton.Click += BrowseButton_Click;

            Label filterLabel = new Label
            {
                Text = "シンボル名絞り込み:",
                AutoSize = true,
                Location = new Point(12, 48)
            };

            _filterTextBox = new TextBox
            {
                Location = new Point(130, 45),
                Width = 260
            };
            _filterTextBox.TextChanged += FilterTextBox_TextChanged;

            _selectAllLinkLabel = new LinkLabel
            {
                Text = "全て選択",
                AutoSize = true,
                Location = new Point(420, 48)
            };
            _selectAllLinkLabel.LinkClicked += SelectAllLinkLabel_LinkClicked;

            _unselectAllLinkLabel = new LinkLabel
            {
                Text = "全て選択解除",
                AutoSize = true,
                Location = new Point(490, 48)
            };
            _unselectAllLinkLabel.LinkClicked += UnselectAllLinkLabel_LinkClicked;

            _grid = new DataGridView
            {
                Location = new Point(12, 78),
                Size = new Size(958, 520),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                DataSource = _bindingSource
            };

            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "Select",
                HeaderText = "選択",
                Width = 60,
                Name = "SelectColumn"
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "シンボル名",
                Width = 340,
                Name = "NameColumn"
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "AddressHex",
                HeaderText = "アドレス",
                Width = 170,
                Name = "AddressColumn"
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Size",
                HeaderText = "サイズ",
                Width = 100,
                Name = "SizeColumn"
            });

            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SourceTable",
                HeaderText = "ソーステーブル",
                Width = 180,
                Name = "SourceTableColumn"
            });

            _statusLabel = new Label
            {
                Text = "ファイルを選択してください。",
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(12, 606),
                Size = new Size(620, 30),
                Anchor = AnchorStyles.Left | AnchorStyles.Bottom
            };

            _okButton = new Button
            {
                Text = "OK",
                Size = new Size(100, 32),
                Location = new Point(760, 604),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.OK
            };
            _okButton.Click += OkButton_Click;

            _cancelButton = new Button
            {
                Text = "キャンセル",
                Size = new Size(100, 32),
                Location = new Point(870, 604),
                Anchor = AnchorStyles.Right | AnchorStyles.Bottom,
                DialogResult = DialogResult.Cancel
            };

            Controls.Add(fileLabel);
            Controls.Add(_filePathTextBox);
            Controls.Add(_browseButton);
            Controls.Add(filterLabel);
            Controls.Add(_filterTextBox);
            Controls.Add(_selectAllLinkLabel);
            Controls.Add(_unselectAllLinkLabel);
            Controls.Add(_grid);
            Controls.Add(_statusLabel);
            Controls.Add(_okButton);
            Controls.Add(_cancelButton);

            AcceptButton = _okButton;
            CancelButton = _cancelButton;
        }

        private void BrowseButton_Click(object? sender, EventArgs e)
        {
            using OpenFileDialog dialog = new OpenFileDialog
            {
                Filter = "ELFファイル (*.elf;*.o;*.so)|*.elf;*.o;*.so|すべてのファイル (*.*)|*.*",
                Title = "ELFファイルを選択"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            LoadElf(dialog.FileName);
        }

        private void LoadElf(string filePath)
        {
            try
            {
                ReadElfResult result = _parser.Parse(filePath);

                _symbolTable.Rows.Clear();
                foreach (ElfSymbolInfo symbol in result.Symbols)
                {
                    DataRow row = _symbolTable.NewRow();
                    row["Select"] = false;
                    row["Name"] = symbol.Name;
                    row["Address"] = symbol.Address;
                    row["AddressHex"] = $"0x{symbol.Address:X}";
                    row["Size"] = symbol.Size;
                    row["SourceTable"] = symbol.SourceTable;
                    _symbolTable.Rows.Add(row);
                }

                SelectedElfFilePath = filePath;
                _filePathTextBox.Text = filePath;
                _statusLabel.Text = $"読み込み成功: {result.Symbols.Count} 件";
            }
            catch (Exception ex)
            {
                _symbolTable.Rows.Clear();
                SelectedElfFilePath = string.Empty;
                _filePathTextBox.Text = filePath;
                _statusLabel.Text = "読み込み失敗";
                MessageBox.Show(this, $"ELF解析に失敗しました。\n{ex.Message}", "エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FilterTextBox_TextChanged(object? sender, EventArgs e)
        {
            string filterText = _filterTextBox.Text.Trim();
            DataView view = (DataView)_bindingSource.DataSource!;

            if (string.IsNullOrEmpty(filterText))
            {
                view.RowFilter = string.Empty;
                return;
            }

            string escaped = EscapeForRowFilter(filterText);
            view.RowFilter = $"Name LIKE '%{escaped}%'";
        }

        private static string EscapeForRowFilter(string value)
        {
            return value
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");
        }

        private void SelectAllLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (DataRow row in _symbolTable.Rows)
            {
                row["Select"] = true;
            }
        }

        private void UnselectAllLinkLabel_LinkClicked(object? sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (DataRow row in _symbolTable.Rows)
            {
                row["Select"] = false;
            }
        }

        private void OkButton_Click(object? sender, EventArgs e)
        {
            _grid.EndEdit();
            BindingContext[_bindingSource]?.EndCurrentEdit();

            SelectedSymbols.Clear();

            foreach (DataRow row in _symbolTable.Rows)
            {
                bool isSelected = row.Field<bool>("Select");
                if (!isSelected)
                {
                    continue;
                }

                SelectedSymbols.Add(new ElfSymbolInfo
                {
                    Name = row.Field<string>("Name") ?? string.Empty,
                    Address = row.Field<ulong>("Address"),
                    Size = row.Field<ulong>("Size"),
                    SourceTable = row.Field<string>("SourceTable") ?? string.Empty
                });
            }

            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
