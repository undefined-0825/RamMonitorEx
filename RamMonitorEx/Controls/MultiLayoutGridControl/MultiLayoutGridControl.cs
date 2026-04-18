using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RamMonitorEx.Controls.MultiLayoutGrid
{
    /// <summary>
    /// 高速描画可能なマルチレイアウトグリッドコントロール
    /// </summary>
    public class MultiLayoutGridControl : UserControl
    {
        private List<GridRow> _rows = new List<GridRow>();
        private Bitmap? _backBuffer;
        private bool _needsRedraw = true;
        private ContextMenuStrip? _contextMenu;

        // グリッド線設定
        private bool _showGridLines = true;
        private Color _gridLineColor = Color.Gray; // 薄いグレー

        // リサイズ用
        private bool _isResizing = false;
        private GridCell? _resizingCell;
        private int _resizeStartX;
        private int _resizeStartWidth;

        public MultiLayoutGridControl()
        {
            // ダブルバッファリングを有効化
            this.SetStyle(ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint |
                         ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.ResizeRedraw, true);
            this.UpdateStyles();

            this.BackColor = Color.Black;
            this.ForeColor = Color.White;
            this.Font = new Font(this.Font.FontFamily, this.Font.Size, FontStyle.Regular);
            this.Size = new Size(600, 400);

            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            ToolStripMenuItem propertiesItem = new ToolStripMenuItem("プロパティ(&P)...");
            propertiesItem.Click += (s, e) =>
            {
                ShowPropertiesDialog();
            };
            _contextMenu.Items.Add(propertiesItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem gridLinesItem = new ToolStripMenuItem("グリッド線を表示(&G)");
            gridLinesItem.CheckOnClick = true;
            gridLinesItem.Checked = _showGridLines;
            gridLinesItem.CheckedChanged += (s, e) =>
            {
                ShowGridLines = gridLinesItem.Checked;
            };
            _contextMenu.Items.Add(gridLinesItem);

            _contextMenu.Items.Add(new ToolStripSeparator());

            ToolStripMenuItem bgColorItem = new ToolStripMenuItem("背景色(&B)...");
            bgColorItem.Click += (s, e) =>
            {
                using (ColorDialog dialog = new ColorDialog())
                {
                    dialog.Color = this.BackColor;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.BackColor = dialog.Color;
                        RequestRedraw();
                    }
                }
            };
            _contextMenu.Items.Add(bgColorItem);

            ToolStripMenuItem fgColorItem = new ToolStripMenuItem("文字色(&F)...");
            fgColorItem.Click += (s, e) =>
            {
                using (ColorDialog dialog = new ColorDialog())
                {
                    dialog.Color = this.ForeColor;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.ForeColor = dialog.Color;
                        RequestRedraw();
                    }
                }
            };
            _contextMenu.Items.Add(fgColorItem);

            ToolStripMenuItem gridColorItem = new ToolStripMenuItem("グリッド線の色(&L)...");
            gridColorItem.Click += (s, e) =>
            {
                using (ColorDialog dialog = new ColorDialog())
                {
                    dialog.Color = this.GridLineColor;
                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        this.GridLineColor = dialog.Color;
                    }
                }
            };
            _contextMenu.Items.Add(gridColorItem);

            this.ContextMenuStrip = _contextMenu;
        }

        private void ShowPropertiesDialog()
        {
            Form? parentForm = this.FindForm();
            if (parentForm == null) return;

            // プロパティダイアログを表示
            var propertiesForm = new RamMonitorEx.Forms.MultiLayoutGridPropertiesForm(this);
            propertiesForm.ShowDialog(parentForm);
            RequestRedraw();
        }

        /// <summary>
        /// 行のリスト
        /// </summary>
        public List<GridRow> Rows => _rows;

        /// <summary>
        /// グリッド線を表示するか
        /// </summary>
        public bool ShowGridLines
        {
            get => _showGridLines;
            set
            {
                _showGridLines = value;
                RequestRedraw();
            }
        }

        /// <summary>
        /// グリッド線の色
        /// </summary>
        public Color GridLineColor
        {
            get => _gridLineColor;
            set
            {
                _gridLineColor = value;
                RequestRedraw();
            }
        }

        /// <summary>
        /// 再描画をリクエスト
        /// </summary>
        public void RequestRedraw()
        {
            _needsRedraw = true;
            Invalidate();
        }

        /// <summary>
        /// レイアウトを計算（各セルのBoundsを更新）
        /// </summary>
        private void CalculateLayout()
        {
            int y = 0;

            foreach (var row in _rows)
            {
                int x = 0;
                row.Bounds = new Rectangle(0, y, this.Width, row.Height);

                foreach (var cell in row.Cells)
                {
                    cell.Bounds = new Rectangle(x, y, cell.Width, row.Height);
                    x += cell.Width;
                }

                y += row.Height;
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_needsRedraw || _backBuffer == null || 
                _backBuffer.Width != this.Width || _backBuffer.Height != this.Height)
            {
                // バックバッファを再作成
                _backBuffer?.Dispose();
                _backBuffer = new Bitmap(Math.Max(1, this.Width), Math.Max(1, this.Height));

                using (Graphics g = Graphics.FromImage(_backBuffer))
                {
                    DrawGrid(g);
                }

                _needsRedraw = false;
            }

            // バックバッファを画面に転送
            e.Graphics.DrawImage(_backBuffer, 0, 0);
        }

        private void DrawGrid(Graphics g)
        {
            // 背景塗りつぶし
            g.Clear(this.BackColor);

            // レイアウト計算
            CalculateLayout();

            // 各行・各セルを描画
            foreach (var row in _rows)
            {
                foreach (var cell in row.Cells)
                {
                    DrawCell(g, cell);
                }
            }

            // グリッド線を描画
            if (_showGridLines)
            {
                DrawGridLines(g);
            }
        }

        private void DrawGridLines(Graphics g)
        {
            using (Pen pen = new Pen(_gridLineColor, 1))
            {
                // 横線（行の境界）
                int y = 0;
                foreach (var row in _rows)
                {
                    y += row.Height;
                    if (y < this.Height)
                    {
                        g.DrawLine(pen, 0, y, this.Width, y);
                    }
                }

                // 縦線（セルの境界）
                foreach (var row in _rows)
                {
                    int x = 0;
                    foreach (var cell in row.Cells)
                    {
                        x += cell.Width;
                        if (x < this.Width)
                        {
                            g.DrawLine(pen, x, row.Bounds.Top, x, row.Bounds.Bottom);
                        }
                    }
                }
            }
        }

        private void DrawCell(Graphics g, GridCell cell)
        {
            // 文字色を決定
            Color textColor = cell.ForeColor ?? this.ForeColor;

            // 表示テキストを取得
            string text = cell.GetDisplayText();

            if (!string.IsNullOrEmpty(text))
            {
                // TextFormatFlagsを設定
                TextFormatFlags flags = GetTextFormatFlags(cell.HorizontalAlignment, cell.VerticalAlignment);

                // TextRendererで高速描画
                TextRenderer.DrawText(g, text, this.Font, cell.Bounds, textColor, flags);
            }
        }

        private TextFormatFlags GetTextFormatFlags(ContentAlignment horizontal, ContentAlignment vertical)
        {
            TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak;

            // 水平配置
            switch (horizontal)
            {
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.TopLeft:
                case ContentAlignment.BottomLeft:
                    flags |= TextFormatFlags.Left;
                    break;
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.TopCenter:
                case ContentAlignment.BottomCenter:
                    flags |= TextFormatFlags.HorizontalCenter;
                    break;
                case ContentAlignment.MiddleRight:
                case ContentAlignment.TopRight:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Right;
                    break;
            }

            // 垂直配置
            switch (vertical)
            {
                case ContentAlignment.TopLeft:
                case ContentAlignment.TopCenter:
                case ContentAlignment.TopRight:
                    flags |= TextFormatFlags.Top;
                    break;
                case ContentAlignment.MiddleLeft:
                case ContentAlignment.MiddleCenter:
                case ContentAlignment.MiddleRight:
                    flags |= TextFormatFlags.VerticalCenter;
                    break;
                case ContentAlignment.BottomLeft:
                case ContentAlignment.BottomCenter:
                case ContentAlignment.BottomRight:
                    flags |= TextFormatFlags.Bottom;
                    break;
            }

            return flags;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                // セル右端のヒットテスト
                GridCell? cell = GetCellAtRightEdge(e.Location);
                if (cell != null)
                {
                    _isResizing = true;
                    _resizingCell = cell;
                    _resizeStartX = e.X;
                    _resizeStartWidth = cell.Width;
                    this.Cursor = Cursors.SizeWE;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_isResizing && _resizingCell != null)
            {
                // リサイズ中
                int delta = e.X - _resizeStartX;
                _resizingCell.Width = Math.Max(30, _resizeStartWidth + delta);
                RequestRedraw();
            }
            else
            {
                // カーソル変更
                GridCell? cell = GetCellAtRightEdge(e.Location);
                this.Cursor = cell != null ? Cursors.SizeWE : Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_isResizing)
            {
                _isResizing = false;
                _resizingCell = null;
                this.Cursor = Cursors.Default;
            }
        }

        private GridCell? GetCellAtRightEdge(Point location)
        {
            const int hitWidth = 5;

            foreach (var row in _rows)
            {
                foreach (var cell in row.Cells)
                {
                    int rightEdge = cell.Bounds.Right;
                    if (location.Y >= cell.Bounds.Top && location.Y <= cell.Bounds.Bottom &&
                        location.X >= rightEdge - hitWidth && location.X <= rightEdge + hitWidth)
                    {
                        return cell;
                    }
                }
            }

            return null;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _backBuffer?.Dispose();
                _contextMenu?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
