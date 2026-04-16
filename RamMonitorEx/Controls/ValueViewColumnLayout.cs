namespace RamMonitorEx.Controls
{
    public class ValueViewColumnLayout
    {
        private const int MinColumnWidth = 30;

        private int _labelWidth;
        private int _valueWidth;
        private int _unitWidth;

        public int LabelWidth
        {
            get => _labelWidth;
            set => _labelWidth = Math.Max(value, MinColumnWidth);
        }

        public int ValueWidth
        {
            get => _valueWidth;
            set => _valueWidth = Math.Max(value, MinColumnWidth);
        }

        public int UnitWidth
        {
            get => _unitWidth;
            set => _unitWidth = Math.Max(value, MinColumnWidth);
        }

        public int TotalWidth => LabelWidth + ValueWidth + UnitWidth;

        public ValueViewColumnLayout()
        {
            _labelWidth = 150;
            _valueWidth = 100;
            _unitWidth = 60;
        }

        public ValueViewColumnLayout(int labelWidth, int valueWidth, int unitWidth)
        {
            LabelWidth = labelWidth;
            ValueWidth = valueWidth;
            UnitWidth = unitWidth;
        }

        public int GetColumnX(int columnIndex)
        {
            return columnIndex switch
            {
                0 => 0,
                1 => LabelWidth,
                2 => LabelWidth + ValueWidth,
                _ => 0
            };
        }

        public int GetColumnWidth(int columnIndex)
        {
            return columnIndex switch
            {
                0 => LabelWidth,
                1 => ValueWidth,
                2 => UnitWidth,
                _ => 0
            };
        }

        public int GetColumnIndexFromX(int x)
        {
            if (x < LabelWidth)
                return 0;
            if (x < LabelWidth + ValueWidth)
                return 1;
            return 2;
        }

        public bool IsNearColumnBorder(int x, int tolerance = 5)
        {
            return Math.Abs(x - LabelWidth) <= tolerance ||
                   Math.Abs(x - (LabelWidth + ValueWidth)) <= tolerance;
        }

        public int GetResizingColumnIndex(int x, int tolerance = 5)
        {
            if (Math.Abs(x - LabelWidth) <= tolerance)
                return 0;
            if (Math.Abs(x - (LabelWidth + ValueWidth)) <= tolerance)
                return 1;
            return -1;
        }
    }
}
