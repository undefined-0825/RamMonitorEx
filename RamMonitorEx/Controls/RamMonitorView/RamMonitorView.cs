using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace RamMonitorEx.Controls.RamMonitorView
{
    public class RamMonitorView : UserControl
    {
        private readonly List<IValueViewRow> _rows = new List<IValueViewRow>();
        private readonly ValueViewColumnLayout _columnLayout = new ValueViewColumnLayout();

        private int _rowHeight = 25;
        private int _headerHeight = 25;
        private bool _headerVisible = true;
        private bool _gridLineVisible = true;
        private bool _columnResizeEnabled = true;
        private bool _panEnabled = true;

        private ValueTextAlignment _valueAlignment = ValueTextAlignment.Right;
        private Color _gridLineColor = Color.Gray;
        private Color _commentForeColor = Color.Gray;
        private Color _commentBackColor = Color.Black;

        private Color _labelForeColor = Color.White;
        private Color _valueForeColor = Color.White;
        private Color _unitForeColor = Color.White;

        private int _panOffsetX = 0;
        private int _panOffsetY = 0;
        private Point _lastMousePosition;
        private bool _isPanning = false;

        private int _resizingColumnIndex = -1;
        private int _resizeStartX = 0;
        private int _resizeStartWidth = 0;

        private ContextMenuStrip _contextMenu;
        private ToolStripMenuItem _gridLineMenuItem;
        private ToolStripMenuItem _headerMenuItem;

        public event EventHandler? ColumnWidthChanged;
        public event EventHandler<int>? RowClicked;
        public event EventHandler<int>? RowDoubleClicked;
        public event EventHandler? PanPositionChanged;

        public int LabelColumnWidth
        {
            get => _columnLayout.LabelWidth;
            set
            {
                _columnLayout.LabelWidth = value;
                Invalidate();
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int ValueColumnWidth
        {
            get => _columnLayout.ValueWidth;
            set
            {
                _columnLayout.ValueWidth = value;
                Invalidate();
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int UnitColumnWidth
        {
            get => _columnLayout.UnitWidth;
            set
            {
                _columnLayout.UnitWidth = value;
                Invalidate();
                ColumnWidthChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public int RowHeight
        {
            get => _rowHeight;
            set
            {
                _rowHeight = Math.Max(value, 10);
                Invalidate();
            }
        }

        public bool HeaderVisible
        {
            get => _headerVisible;
            set
            {
                _headerVisible = value;
                Invalidate();
            }
        }

        public bool GridLineVisible
        {
            get => _gridLineVisible;
            set
            {
                _gridLineVisible = value;
                Invalidate();
            }
        }

        public ValueTextAlignment ValueAlignment
        {
            get => _valueAlignment;
            set
            {
                _valueAlignment = value;
                Invalidate();
            }
        }

        public Color GridLineColor
        {
            get => _gridLineColor;
            set
            {
                _gridLineColor = value;
                Invalidate();
            }
        }

        public Color CommentForeColor
        {
            get => _commentForeColor;
            set
            {
                _commentForeColor = value;
                Invalidate();
            }
        }

        public Color CommentBackColor
        {
            get => _commentBackColor;
            set
            {
                _commentBackColor = value;
                Invalidate();
            }
        }

        public Color LabelForeColor
        {
            get => _labelForeColor;
            set
            {
                _labelForeColor = value;
                Invalidate();
            }
        }

        public Color ValueForeColor
        {
            get => _valueForeColor;
            set
            {
                _valueForeColor = value;
                Invalidate();
            }
        }

        public Color UnitForeColor
        {
            get => _unitForeColor;
            set
            {
                _unitForeColor = value;
                Invalidate();
            }
        }

        public bool ColumnResizeEnabled
        {
            get => _columnResizeEnabled;
            set => _columnResizeEnabled = value;
        }

        public bool PanEnabled
        {
            get => _panEnabled;
            set => _panEnabled = value;
        }

        public IReadOnlyList<IValueViewRow> Rows => _rows.AsReadOnly();

        public RamMonitorView()
        {
            DoubleBuffered = true;
            BackColor = Color.Black;
            ForeColor = Color.White;

            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.UserPaint, true);

            InitializeContextMenu();
        }

        private void InitializeContextMenu()
        {
            _contextMenu = new ContextMenuStrip();

            _gridLineMenuItem = new ToolStripMenuItem("グリッド線を表示")
            {
                CheckOnClick = true,
                Checked = _gridLineVisible
            };
            _gridLineMenuItem.Click += GridLineMenuItem_Click;

            _headerMenuItem = new ToolStripMenuItem("ヘッダーを表示")
            {
                CheckOnClick = true,
                Checked = _headerVisible
            };
            _headerMenuItem.Click += HeaderMenuItem_Click;

            _contextMenu.Items.Add(_gridLineMenuItem);
            _contextMenu.Items.Add(_headerMenuItem);
            _contextMenu.Items.Add(new ToolStripSeparator());

            // 全体メニュー
            ToolStripMenuItem globalMenu = new ToolStripMenuItem("全体");

            ToolStripMenuItem bgColorItem = new ToolStripMenuItem("背景色");
            bgColorItem.Click += BackgroundColorMenuItem_Click;
            globalMenu.DropDownItems.Add(bgColorItem);

            ToolStripMenuItem fgColorItem = new ToolStripMenuItem("文字色");
            fgColorItem.Click += ForegroundColorMenuItem_Click;
            globalMenu.DropDownItems.Add(fgColorItem);

            _contextMenu.Items.Add(globalMenu);

            // セル文字色メニュー
            ToolStripMenuItem cellColorMenu = new ToolStripMenuItem("セル文字色");

            ToolStripMenuItem labelColorItem = new ToolStripMenuItem("ラベル列");
            labelColorItem.Click += LabelColorMenuItem_Click;
            cellColorMenu.DropDownItems.Add(labelColorItem);

            ToolStripMenuItem valueColorItem = new ToolStripMenuItem("値列");
            valueColorItem.Click += ValueColorMenuItem_Click;
            cellColorMenu.DropDownItems.Add(valueColorItem);

            ToolStripMenuItem unitColorItem = new ToolStripMenuItem("単位列");
            unitColorItem.Click += UnitColorMenuItem_Click;
            cellColorMenu.DropDownItems.Add(unitColorItem);

            cellColorMenu.DropDownItems.Add(new ToolStripSeparator());

            ToolStripMenuItem commentColorItem = new ToolStripMenuItem("コメント行");
            commentColorItem.Click += CommentColorMenuItem_Click;
            cellColorMenu.DropDownItems.Add(commentColorItem);

            _contextMenu.Items.Add(cellColorMenu);

            this.ContextMenuStrip = _contextMenu;
        }

        private void GridLineMenuItem_Click(object? sender, EventArgs e)
        {
            GridLineVisible = _gridLineMenuItem.Checked;
        }

        private void HeaderMenuItem_Click(object? sender, EventArgs e)
        {
            HeaderVisible = _headerMenuItem.Checked;
        }

        private void BackgroundColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = BackColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    BackColor = dialog.Color;
                }
            }
        }

        private void ForegroundColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = ForeColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ForeColor = dialog.Color;
                }
            }
        }

        private void LabelColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = _labelForeColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    LabelForeColor = dialog.Color;
                }
            }
        }

        private void ValueColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = _valueForeColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ValueForeColor = dialog.Color;
                }
            }
        }

        private void UnitColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = _unitForeColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    UnitForeColor = dialog.Color;
                }
            }
        }

        private void CommentColorMenuItem_Click(object? sender, EventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                dialog.Color = _commentForeColor;
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    CommentForeColor = dialog.Color;
                }
            }
        }

        public void AddDataRow(string label, string value, string unit)
        {
            _rows.Add(new ValueDataRow(label, value, unit));
            Invalidate();
        }

        public void AddCommentRow(string comment)
        {
            _rows.Add(new ValueCommentRow(comment));
            Invalidate();
        }

        public void UpdateValue(int rowIndex, string value)
        {
            if (rowIndex >= 0 && rowIndex < _rows.Count && _rows[rowIndex] is ValueDataRow dataRow)
            {
                dataRow.ValueText = value;
                InvalidateRow(rowIndex);
            }
        }

        public void UpdateDataRow(int rowIndex, string label, string value, string unit)
        {
            if (rowIndex >= 0 && rowIndex < _rows.Count && _rows[rowIndex] is ValueDataRow dataRow)
            {
                dataRow.LabelText = label;
                dataRow.ValueText = value;
                dataRow.UnitText = unit;
                InvalidateRow(rowIndex);
            }
        }

        public void RemoveRow(int rowIndex)
        {
            if (rowIndex >= 0 && rowIndex < _rows.Count)
            {
                _rows.RemoveAt(rowIndex);
                Invalidate();
            }
        }

        public void ClearRows()
        {
            _rows.Clear();
            Invalidate();
        }

        private void InvalidateRow(int rowIndex)
        {
            int y = GetRowY(rowIndex);
            Rectangle rowRect = new Rectangle(0, y, Width, _rowHeight);
            Invalidate(rowRect);
        }

        private int GetRowY(int rowIndex)
        {
            int headerOffset = _headerVisible ? _headerHeight : 0;
            return headerOffset + rowIndex * _rowHeight - _panOffsetY;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(BackColor);

            if (_headerVisible)
            {
                DrawHeader(g);
            }

            int headerOffset = _headerVisible ? _headerHeight : 0;
            int firstVisibleRow = Math.Max(0, _panOffsetY / _rowHeight);
            int lastVisibleRow = Math.Min(_rows.Count - 1, (e.ClipRectangle.Bottom + _panOffsetY - headerOffset) / _rowHeight);

            for (int i = firstVisibleRow; i <= lastVisibleRow && i < _rows.Count; i++)
            {
                DrawRow(g, i);
            }
        }

        private void DrawHeader(Graphics g)
        {
            int x = -_panOffsetX;
            int y = -_panOffsetY;
            Rectangle headerRect = new Rectangle(x, y, _columnLayout.TotalWidth, _headerHeight);

            using (Brush headerBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(headerBrush, headerRect);
            }

            using (Pen gridPen = new Pen(_gridLineColor))
            using (Brush textBrush = new SolidBrush(ForeColor))
            {
                string[] headers = { "Label", "Value", "Unit" };
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                for (int i = 0; i < 3; i++)
                {
                    int colX = x + _columnLayout.GetColumnX(i);
                    int colWidth = _columnLayout.GetColumnWidth(i);
                    Rectangle cellRect = new Rectangle(colX + 5, y, colWidth - 10, _headerHeight);
                    g.DrawString(headers[i], Font, textBrush, cellRect, sf);

                    if (_gridLineVisible && i < 2)
                    {
                        int lineX = colX + colWidth;
                        g.DrawLine(gridPen, lineX, y, lineX, y + _headerHeight);
                    }
                }

                if (_gridLineVisible)
                {
                    g.DrawLine(gridPen, x, y + _headerHeight - 1, x + _columnLayout.TotalWidth, y + _headerHeight - 1);
                }
            }
        }

        private void DrawRow(Graphics g, int rowIndex)
        {
            int headerOffset = _headerVisible ? _headerHeight : 0;
            int y = headerOffset + rowIndex * _rowHeight - _panOffsetY;
            int x = -_panOffsetX;

            IValueViewRow row = _rows[rowIndex];

            if (row is ValueDataRow dataRow)
            {
                DrawDataRow(g, dataRow, x, y);
            }
            else if (row is ValueCommentRow commentRow)
            {
                DrawCommentRow(g, commentRow, x, y);
            }
        }

        private void DrawDataRow(Graphics g, ValueDataRow row, int x, int y)
        {
            using (Pen gridPen = new Pen(_gridLineColor))
            {
                string[] texts = { row.LabelText, row.ValueText, row.UnitText };
                StringAlignment[] alignments = { StringAlignment.Near, GetValueStringAlignment(), StringAlignment.Near };
                Color[] colors = { _labelForeColor, _valueForeColor, _unitForeColor };

                for (int i = 0; i < 3; i++)
                {
                    int colX = x + _columnLayout.GetColumnX(i);
                    int colWidth = _columnLayout.GetColumnWidth(i);

                    Rectangle cellRect = new Rectangle(colX + 5, y, colWidth - 10, _rowHeight);
                    StringFormat sf = new StringFormat
                    {
                        Alignment = alignments[i],
                        LineAlignment = StringAlignment.Center
                    };

                    using (Brush textBrush = new SolidBrush(colors[i]))
                    {
                        g.DrawString(texts[i], Font, textBrush, cellRect, sf);
                    }

                    if (_gridLineVisible && i < 2)
                    {
                        int lineX = colX + colWidth;
                        g.DrawLine(gridPen, lineX, y, lineX, y + _rowHeight);
                    }
                }

                if (_gridLineVisible)
                {
                    g.DrawLine(gridPen, x, y + _rowHeight - 1, x + _columnLayout.TotalWidth, y + _rowHeight - 1);
                }
            }
        }

        private void DrawCommentRow(Graphics g, ValueCommentRow row, int x, int y)
        {
            Rectangle rowRect = new Rectangle(x, y, _columnLayout.TotalWidth, _rowHeight);

            using (Brush bgBrush = new SolidBrush(BackColor))
            {
                g.FillRectangle(bgBrush, rowRect);
            }

            using (Brush textBrush = new SolidBrush(_commentForeColor))
            using (Pen gridPen = new Pen(_gridLineColor))
            {
                Rectangle textRect = new Rectangle(x + 5, y, _columnLayout.TotalWidth - 10, _rowHeight);
                StringFormat sf = new StringFormat
                {
                    Alignment = StringAlignment.Near,
                    LineAlignment = StringAlignment.Center
                };

                g.DrawString(row.CommentText, Font, textBrush, textRect, sf);

                if (_gridLineVisible)
                {
                    g.DrawLine(gridPen, x, y + _rowHeight - 1, x + _columnLayout.TotalWidth, y + _rowHeight - 1);
                }
            }
        }

        private StringAlignment GetValueStringAlignment()
        {
            return _valueAlignment switch
            {
                ValueTextAlignment.Left => StringAlignment.Near,
                ValueTextAlignment.Center => StringAlignment.Center,
                ValueTextAlignment.Right => StringAlignment.Far,
                _ => StringAlignment.Far
            };
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e.Button == MouseButtons.Left)
            {
                int adjustedX = e.X + _panOffsetX;

                if (_columnResizeEnabled && _columnLayout.IsNearColumnBorder(adjustedX))
                {
                    _resizingColumnIndex = _columnLayout.GetResizingColumnIndex(adjustedX);
                    if (_resizingColumnIndex >= 0)
                    {
                        _resizeStartX = e.X;
                        _resizeStartWidth = _columnLayout.GetColumnWidth(_resizingColumnIndex);
                        Cursor = Cursors.VSplit;
                        return;
                    }
                }

                if (_panEnabled)
                {
                    _isPanning = true;
                    _lastMousePosition = e.Location;
                    Cursor = Cursors.Hand;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (_resizingColumnIndex >= 0)
            {
                int delta = e.X - _resizeStartX;
                int newWidth = _resizeStartWidth + delta;

                if (_resizingColumnIndex == 0)
                {
                    LabelColumnWidth = newWidth;
                }
                else if (_resizingColumnIndex == 1)
                {
                    ValueColumnWidth = newWidth;
                }
                return;
            }

            if (_isPanning)
            {
                int deltaX = _lastMousePosition.X - e.X;
                int deltaY = _lastMousePosition.Y - e.Y;

                _panOffsetX = Math.Max(0, _panOffsetX + deltaX);
                _panOffsetY = Math.Max(0, _panOffsetY + deltaY);

                _lastMousePosition = e.Location;
                Invalidate();
                PanPositionChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            int adjustedX = e.X + _panOffsetX;
            if (_columnResizeEnabled && _columnLayout.IsNearColumnBorder(adjustedX))
            {
                Cursor = Cursors.VSplit;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (_resizingColumnIndex >= 0)
            {
                _resizingColumnIndex = -1;
                Cursor = Cursors.Default;
            }

            if (_isPanning)
            {
                _isPanning = false;
                Cursor = Cursors.Default;
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (e.Button == MouseButtons.Left && !_isPanning && _resizingColumnIndex < 0)
            {
                int headerOffset = _headerVisible ? _headerHeight : 0;
                int adjustedY = e.Y + _panOffsetY - headerOffset;
                int rowIndex = adjustedY / _rowHeight;

                if (rowIndex >= 0 && rowIndex < _rows.Count)
                {
                    RowClicked?.Invoke(this, rowIndex);
                }
            }
        }

        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            base.OnMouseDoubleClick(e);

            if (e.Button == MouseButtons.Left)
            {
                int headerOffset = _headerVisible ? _headerHeight : 0;
                int adjustedY = e.Y + _panOffsetY - headerOffset;
                int rowIndex = adjustedY / _rowHeight;

                if (rowIndex >= 0 && rowIndex < _rows.Count)
                {
                    RowDoubleClicked?.Invoke(this, rowIndex);
                }
            }
        }
    }
}
