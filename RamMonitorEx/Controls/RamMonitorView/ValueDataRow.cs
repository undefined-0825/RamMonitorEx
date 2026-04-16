namespace RamMonitorEx.Controls.RamMonitorView
{
    public class ValueDataRow : IValueViewRow
    {
        public ValueRowType RowType => ValueRowType.Data;
        public string LabelText { get; set; }
        public string ValueText { get; set; }
        public string UnitText { get; set; }

        public ValueDataRow(string labelText, string valueText, string unitText)
        {
            LabelText = labelText;
            ValueText = valueText;
            UnitText = unitText;
        }
    }
}
