using System;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Serialization;

namespace WindowsFormsApp1.Serialization
{
    /// <summary>
    /// ワークスペース全体の設定を保持するルートクラス
    /// </summary>
    [XmlRoot("Workspace")]
    public class WorkspaceConfig
    {
        /// <summary>
        /// ワークスペース名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 保存日時
        /// </summary>
        public DateTime SavedDate { get; set; }

        /// <summary>
        /// DockPanelのレイアウト設定
        /// </summary>
        public string DockPanelLayout { get; set; } = string.Empty;

        /// <summary>
        /// グラフペインのリスト
        /// </summary>
        [XmlArray("GraphPanes")]
        [XmlArrayItem("GraphPane")]
        public List<GraphPaneConfig> GraphPanes { get; set; } = new List<GraphPaneConfig>();

        /// <summary>
        /// RAMモニタペインのリスト
        /// </summary>
        [XmlArray("RamMonitorPanes")]
        [XmlArrayItem("RamMonitorPane")]
        public List<RamMonitorPaneConfig> RamMonitorPanes { get; set; } = new List<RamMonitorPaneConfig>();

        /// <summary>
        /// マルチレイアウトグリッドペインのリスト
        /// </summary>
        [XmlArray("MultiLayoutGridPanes")]
        [XmlArrayItem("MultiLayoutGridPane")]
        public List<MultiLayoutGridPaneConfig> MultiLayoutGridPanes { get; set; } = new List<MultiLayoutGridPaneConfig>();

        /// <summary>
        /// ELFシンボルペインのリスト
        /// </summary>
        [XmlArray("ElfSymbolPanes")]
        [XmlArrayItem("ElfSymbolPane")]
        public List<ElfSymbolPaneConfig> ElfSymbolPanes { get; set; } = new List<ElfSymbolPaneConfig>();
    }

    /// <summary>
    /// グラフペインの設定
    /// </summary>
    public class GraphPaneConfig
    {
        /// <summary>
        /// ペイン名（一意の識別子）
        /// </summary>
        [XmlAttribute]
        public string PaneName { get; set; } = string.Empty;

        /// <summary>
        /// グラフコントロールの設定
        /// </summary>
        public LineGraphConfig GraphConfig { get; set; } = new LineGraphConfig();
    }

    /// <summary>
    /// 折れ線グラフコントロールの設定
    /// </summary>
    public class LineGraphConfig
    {
        public int MaxPoints { get; set; }
        public bool AutoScaleY { get; set; }
        public double YMin { get; set; }
        public double YMax { get; set; }
        public float LineWidth { get; set; }
        public bool ShowGrid { get; set; }
        public bool FillArea { get; set; }
        public int RedrawIntervalMs { get; set; }

        // 色の設定（ARGB形式で保存）
        public int BackColorArgb { get; set; }
        public int GridColorArgb { get; set; }

        // パディング
        public int PlotPaddingLeft { get; set; }
        public int PlotPaddingTop { get; set; }
        public int PlotPaddingRight { get; set; }
        public int PlotPaddingBottom { get; set; }

        /// <summary>
        /// 系列のリスト
        /// </summary>
        [XmlArray("Series")]
        [XmlArrayItem("GraphSeries")]
        public List<GraphSeriesConfig> Series { get; set; } = new List<GraphSeriesConfig>();

        // 色のヘルパープロパティ（シリアライズ対象外）
        [XmlIgnore]
        public Color BackColor
        {
            get => Color.FromArgb(BackColorArgb);
            set => BackColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color GridColor
        {
            get => Color.FromArgb(GridColorArgb);
            set => GridColorArgb = value.ToArgb();
        }
    }

    /// <summary>
    /// グラフ系列の設定
    /// </summary>
    public class GraphSeriesConfig
    {
        [XmlAttribute]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute]
        public int ColorArgb { get; set; }

        [XmlAttribute]
        public bool Visible { get; set; }

        [XmlIgnore]
        public Color Color
        {
            get => Color.FromArgb(ColorArgb);
            set => ColorArgb = value.ToArgb();
        }
    }

    /// <summary>
    /// RAMモニタペインの設定
    /// </summary>
    public class RamMonitorPaneConfig
    {
        /// <summary>
        /// ペイン名（一意の識別子）
        /// </summary>
        [XmlAttribute]
        public string PaneName { get; set; } = string.Empty;

        /// <summary>
        /// RAMモニタビューの設定
        /// </summary>
        public RamMonitorViewConfig ViewConfig { get; set; } = new RamMonitorViewConfig();
    }

    /// <summary>
    /// RAMモニタビューの設定
    /// </summary>
    public class RamMonitorViewConfig
    {
        // レイアウト設定
        public int LabelColumnWidth { get; set; }
        public int ValueColumnWidth { get; set; }
        public int UnitColumnWidth { get; set; }
        public int RowHeight { get; set; }
        public bool HeaderVisible { get; set; }
        public bool GridLineVisible { get; set; }

        // 表示設定
        public int ValueAlignment { get; set; } // ValueTextAlignment の int 値
        public int BackColorArgb { get; set; }
        public int ForeColorArgb { get; set; }
        public int LabelForeColorArgb { get; set; }
        public int ValueForeColorArgb { get; set; }
        public int UnitForeColorArgb { get; set; }
        public int CommentForeColorArgb { get; set; }
        public int CommentBackColorArgb { get; set; }
        public int GridLineColorArgb { get; set; }

        // 動作設定
        public bool ColumnResizeEnabled { get; set; }
        public bool PanEnabled { get; set; }

        // 色のヘルパープロパティ
        [XmlIgnore]
        public Color BackColor
        {
            get => Color.FromArgb(BackColorArgb);
            set => BackColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color ForeColor
        {
            get => Color.FromArgb(ForeColorArgb);
            set => ForeColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color LabelForeColor
        {
            get => Color.FromArgb(LabelForeColorArgb);
            set => LabelForeColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color ValueForeColor
        {
            get => Color.FromArgb(ValueForeColorArgb);
            set => ValueForeColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color UnitForeColor
        {
            get => Color.FromArgb(UnitForeColorArgb);
            set => UnitForeColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color CommentForeColor
        {
            get => Color.FromArgb(CommentForeColorArgb);
            set => CommentForeColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color CommentBackColor
        {
            get => Color.FromArgb(CommentBackColorArgb);
            set => CommentBackColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color GridLineColor
        {
            get => Color.FromArgb(GridLineColorArgb);
            set => GridLineColorArgb = value.ToArgb();
        }

        /// <summary>
        /// データ行のリスト
        /// </summary>
        [XmlArray("DataRows")]
        [XmlArrayItem("DataRow")]
        public List<DataRowConfig> DataRows { get; set; } = new List<DataRowConfig>();

        /// <summary>
        /// コメント行のリスト（行番号も保持）
        /// </summary>
        [XmlArray("CommentRows")]
        [XmlArrayItem("CommentRow")]
        public List<CommentRowConfig> CommentRows { get; set; } = new List<CommentRowConfig>();
    }

    /// <summary>
    /// データ行の設定
    /// </summary>
    public class DataRowConfig
    {
        [XmlAttribute]
        public int RowIndex { get; set; }

        [XmlAttribute]
        public string LabelText { get; set; } = string.Empty;

        [XmlAttribute]
        public string UnitText { get; set; } = string.Empty;
    }

    /// <summary>
    /// コメント行の設定
    /// </summary>
    public class CommentRowConfig
    {
        [XmlAttribute]
        public int RowIndex { get; set; }

        [XmlAttribute]
        public string CommentText { get; set; } = string.Empty;
    }

    /// <summary>
    /// マルチレイアウトグリッドペインの設定
    /// </summary>
    public class MultiLayoutGridPaneConfig
    {
        /// <summary>
        /// ペイン名（一意の識別子）
        /// </summary>
        [XmlAttribute]
        public string PaneName { get; set; } = string.Empty;

        /// <summary>
        /// マルチレイアウトグリッドの設定
        /// </summary>
        public MultiLayoutGridConfig GridConfig { get; set; } = new MultiLayoutGridConfig();
    }

    /// <summary>
    /// マルチレイアウトグリッドの設定
    /// </summary>
    public class MultiLayoutGridConfig
    {
        public bool ShowGridLines { get; set; }
        public int GridLineColorArgb { get; set; }
        public int BackColorArgb { get; set; }
        public int ForeColorArgb { get; set; }

        [XmlArray("Rows")]
        [XmlArrayItem("Row")]
        public List<MultiLayoutGridRowConfig> Rows { get; set; } = new List<MultiLayoutGridRowConfig>();

        [XmlIgnore]
        public Color GridLineColor
        {
            get => Color.FromArgb(GridLineColorArgb);
            set => GridLineColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color BackColor
        {
            get => Color.FromArgb(BackColorArgb);
            set => BackColorArgb = value.ToArgb();
        }

        [XmlIgnore]
        public Color ForeColor
        {
            get => Color.FromArgb(ForeColorArgb);
            set => ForeColorArgb = value.ToArgb();
        }
    }

    /// <summary>
    /// マルチレイアウトグリッド行の設定
    /// </summary>
    public class MultiLayoutGridRowConfig
    {
        [XmlAttribute]
        public int RowIndex { get; set; }

        [XmlAttribute]
        public int Height { get; set; }

        [XmlArray("Cells")]
        [XmlArrayItem("Cell")]
        public List<MultiLayoutGridCellConfig> Cells { get; set; } = new List<MultiLayoutGridCellConfig>();
    }

    /// <summary>
    /// マルチレイアウトグリッドセルの設定
    /// </summary>
    public class MultiLayoutGridCellConfig
    {
        [XmlAttribute]
        public int CellIndex { get; set; }

        [XmlAttribute]
        public string Text { get; set; } = string.Empty;

        [XmlAttribute]
        public bool IsVariable { get; set; }

        [XmlAttribute]
        public int Width { get; set; }

        [XmlAttribute]
        public int HorizontalAlignment { get; set; }

        [XmlAttribute]
        public int VerticalAlignment { get; set; }

        [XmlAttribute]
        public bool HasForeColor { get; set; }

        [XmlAttribute]
        public int ForeColorArgb { get; set; }
    }

    /// <summary>
    /// ELFシンボルペインの設定
    /// </summary>
    public class ElfSymbolPaneConfig
    {
        [XmlAttribute]
        public string PaneName { get; set; } = string.Empty;

        [XmlAttribute]
        public string SourceName { get; set; } = string.Empty;

        [XmlArray("Symbols")]
        [XmlArrayItem("Symbol")]
        public List<ElfSelectedSymbolConfig> Symbols { get; set; } = new List<ElfSelectedSymbolConfig>();
    }

    /// <summary>
    /// ELF選択シンボルの設定
    /// </summary>
    public class ElfSelectedSymbolConfig
    {
        [XmlAttribute]
        public string Name { get; set; } = string.Empty;

        [XmlAttribute]
        public ulong Address { get; set; }

        [XmlAttribute]
        public ulong Size { get; set; }

        [XmlAttribute]
        public string SourceTable { get; set; } = string.Empty;
    }
}
