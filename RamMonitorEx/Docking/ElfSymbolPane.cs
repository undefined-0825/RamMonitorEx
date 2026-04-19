using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using WindowsFormsApp1.ReadElf;

namespace WindowsFormsApp1.Docking
{
    /// <summary>
    /// ELFシンボル一覧を表示する専用ドッキングペイン
    /// </summary>
    public class ElfSymbolPane : DockContent
    {
        private readonly string _paneName;
        private readonly string _sourceName;
        private Panel? _containerPanel;
        private DataGridView? _grid;
        private TextBox? _filterTextBox;
        private Label? _statusLabel;
        private readonly DataSet _dataSet = new DataSet("ElfSymbolPaneDataSet");
        private readonly DataTable _table = new DataTable("Symbols");
        private readonly BindingSource _bindingSource = new BindingSource();

        public ElfSymbolPane(string paneName, string sourceName, IEnumerable<ElfSymbolInfo> symbols)
        {
            if (string.IsNullOrWhiteSpace(paneName))
            {
                throw new ArgumentException("paneName is empty", nameof(paneName));
            }

            _paneName = paneName;
            _sourceName = sourceName;

            Text = paneName;
            TabText = paneName;
            ToolTipText = paneName;
            HideOnClose = false;

            InitializeData();
            InitializeControls();
            LoadSymbols(symbols);
        }

        public string PaneName => _paneName;
        public string SourceName => _sourceName;

        private void InitializeData()
        {
            _table.Columns.Add("Selected", typeof(bool));
            _table.Columns.Add("Name", typeof(string));
            _table.Columns.Add("Address", typeof(ulong));
            _table.Columns.Add("AddressHex", typeof(string));
            _table.Columns.Add("Size", typeof(ulong));
            _table.Columns.Add("SourceTable", typeof(string));
            _dataSet.Tables.Add(_table);
            _bindingSource.DataSource = new DataView(_table);
        }

        private void InitializeControls()
        {
            _containerPanel = new Panel
            {
                Dock = DockStyle.Fill,
                Padding = new Padding(6)
            };
            Controls.Add(_containerPanel);

            Label sourceLabel = new Label
            {
                Text = $"ELF: {_sourceName}",
                AutoSize = true,
                Location = new Point(6, 8)
            };
            _containerPanel.Controls.Add(sourceLabel);

            Label filterLabel = new Label
            {
                Text = "シンボル名絞り込み:",
                AutoSize = true,
                Location = new Point(6, 32)
            };
            _containerPanel.Controls.Add(filterLabel);

            _filterTextBox = new TextBox
            {
                Location = new Point(120, 28),
                Width = 280
            };
            _filterTextBox.TextChanged += FilterTextBox_TextChanged;
            _containerPanel.Controls.Add(_filterTextBox);

            _grid = new DataGridView
            {
                Location = new Point(6, 58),
                Size = new Size(760, 420),
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                AllowUserToResizeRows = false,
                RowHeadersVisible = false,
                DataSource = _bindingSource,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect
            };

            _grid.Columns.Add(new DataGridViewCheckBoxColumn
            {
                HeaderText = "選択",
                DataPropertyName = "Selected",
                Width = 60
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "シンボル名",
                DataPropertyName = "Name",
                Width = 260
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "アドレス",
                DataPropertyName = "AddressHex",
                Width = 160
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "サイズ",
                DataPropertyName = "Size",
                Width = 100
            });
            _grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "ソース",
                DataPropertyName = "SourceTable",
                Width = 150
            });

            _containerPanel.Controls.Add(_grid);

            _statusLabel = new Label
            {
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleLeft,
                Location = new Point(6, 484),
                Size = new Size(760, 24),
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            _containerPanel.Controls.Add(_statusLabel);
        }

        private void LoadSymbols(IEnumerable<ElfSymbolInfo> symbols)
        {
            _table.Rows.Clear();
            foreach (ElfSymbolInfo symbol in symbols)
            {
                DataRow row = _table.NewRow();
                row["Selected"] = true;
                row["Name"] = symbol.Name;
                row["Address"] = symbol.Address;
                row["AddressHex"] = $"0x{symbol.Address:X}";
                row["Size"] = symbol.Size;
                row["SourceTable"] = symbol.SourceTable;
                _table.Rows.Add(row);
            }

            if (_statusLabel != null)
            {
                _statusLabel.Text = $"件数: {_table.Rows.Count}";
            }
        }

        private void FilterTextBox_TextChanged(object? sender, EventArgs e)
        {
            if (_bindingSource.DataSource is not DataView view || _filterTextBox == null)
            {
                return;
            }

            string text = _filterTextBox.Text.Trim();
            if (string.IsNullOrEmpty(text))
            {
                view.RowFilter = string.Empty;
                return;
            }

            string escaped = text
                .Replace("'", "''")
                .Replace("[", "[[]")
                .Replace("%", "[%]")
                .Replace("*", "[*]");

            view.RowFilter = $"Name LIKE '%{escaped}%'";
        }

        public List<ElfSymbolInfo> GetSelectedSymbols()
        {
            return _table.AsEnumerable()
                .Where(r => r.Field<bool>("Selected"))
                .Select(r => new ElfSymbolInfo
                {
                    Name = r.Field<string>("Name") ?? string.Empty,
                    Address = r.Field<ulong>("Address"),
                    Size = r.Field<ulong>("Size"),
                    SourceTable = r.Field<string>("SourceTable") ?? string.Empty
                })
                .ToList();
        }

        protected override string GetPersistString()
        {
            return $"ElfSymbolPane|{_paneName}";
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PaneNameManager.Instance.UnregisterName(_paneName);
                _grid?.Dispose();
                _filterTextBox?.Dispose();
                _statusLabel?.Dispose();
                _containerPanel?.Dispose();
                _bindingSource.Dispose();
                _table.Dispose();
                _dataSet.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
