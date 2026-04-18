using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Drawing;
using WeifenLuo.WinFormsUI.Docking;

namespace WindowsFormsApp1.Serialization
{
    /// <summary>
    /// ワークスペースの保存・読み込みを管理するクラス
    /// </summary>
    public class WorkspaceSerializer
    {
        private readonly XmlSerializer _serializer;

        public WorkspaceSerializer()
        {
            _serializer = new XmlSerializer(typeof(WorkspaceConfig));
        }

        /// <summary>
        /// ワークスペースをXMLファイルに保存
        /// </summary>
        /// <param name="config">ワークスペース設定</param>
        /// <param name="filePath">保存先ファイルパス</param>
        public void Save(WorkspaceConfig config, string filePath)
        {
            try
            {
                config.SavedDate = DateTime.Now;

                XmlWriterSettings settings = new XmlWriterSettings
                {
                    Indent = true,
                    IndentChars = "  ",
                    Encoding = System.Text.Encoding.UTF8
                };

                using (XmlWriter writer = XmlWriter.Create(filePath, settings))
                {
                    // ルート要素を手動で書き込み
                    writer.WriteStartElement("Workspace");

                    // 基本情報
                    writer.WriteElementString("Name", config.Name);
                    writer.WriteElementString("SavedDate", config.SavedDate.ToString("o"));

                    // DockPanelLayoutをCDATAセクションで書き込み
                    writer.WriteStartElement("DockPanelLayout");
                    writer.WriteCData(config.DockPanelLayout);
                    writer.WriteEndElement();

                    // GraphPanes
                    writer.WriteStartElement("GraphPanes");
                    foreach (var pane in config.GraphPanes)
                    {
                        WriteGraphPane(writer, pane);
                    }
                    writer.WriteEndElement();

                    // RamMonitorPanes
                    writer.WriteStartElement("RamMonitorPanes");
                    foreach (var pane in config.RamMonitorPanes)
                    {
                        WriteRamMonitorPane(writer, pane);
                    }
                    writer.WriteEndElement();

                    // MultiLayoutGridPanes
                    writer.WriteStartElement("MultiLayoutGridPanes");
                    foreach (var pane in config.MultiLayoutGridPanes)
                    {
                        WriteMultiLayoutGridPane(writer, pane);
                    }
                    writer.WriteEndElement();

                    writer.WriteEndElement(); // Workspace
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ワークスペースの保存に失敗しました: {ex.Message}", ex);
            }
        }

        private void WriteGraphPane(XmlWriter writer, GraphPaneConfig pane)
        {
            writer.WriteStartElement("GraphPane");
            writer.WriteAttributeString("PaneName", pane.PaneName);

            // GraphConfig
            var cfg = pane.GraphConfig;
            writer.WriteStartElement("GraphConfig");
            writer.WriteAttributeString("MaxPoints", cfg.MaxPoints.ToString());
            writer.WriteAttributeString("AutoScaleY", cfg.AutoScaleY.ToString());
            writer.WriteAttributeString("YMin", cfg.YMin.ToString());
            writer.WriteAttributeString("YMax", cfg.YMax.ToString());
            writer.WriteAttributeString("LineWidth", cfg.LineWidth.ToString());
            writer.WriteAttributeString("ShowGrid", cfg.ShowGrid.ToString());
            writer.WriteAttributeString("FillArea", cfg.FillArea.ToString());
            writer.WriteAttributeString("RedrawIntervalMs", cfg.RedrawIntervalMs.ToString());
            writer.WriteAttributeString("BackColorArgb", cfg.BackColorArgb.ToString());
            writer.WriteAttributeString("GridColorArgb", cfg.GridColorArgb.ToString());
            writer.WriteAttributeString("PlotPaddingLeft", cfg.PlotPaddingLeft.ToString());
            writer.WriteAttributeString("PlotPaddingTop", cfg.PlotPaddingTop.ToString());
            writer.WriteAttributeString("PlotPaddingRight", cfg.PlotPaddingRight.ToString());
            writer.WriteAttributeString("PlotPaddingBottom", cfg.PlotPaddingBottom.ToString());

            // Series
            foreach (var series in cfg.Series)
            {
                writer.WriteStartElement("GraphSeries");
                writer.WriteAttributeString("Name", series.Name);
                writer.WriteAttributeString("ColorArgb", series.ColorArgb.ToString());
                writer.WriteAttributeString("Visible", series.Visible.ToString());
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // GraphConfig
            writer.WriteEndElement(); // GraphPane
        }

        private void WriteRamMonitorPane(XmlWriter writer, RamMonitorPaneConfig pane)
        {
            writer.WriteStartElement("RamMonitorPane");
            writer.WriteAttributeString("PaneName", pane.PaneName);

            // ViewConfig
            var cfg = pane.ViewConfig;
            writer.WriteStartElement("ViewConfig");
            writer.WriteAttributeString("LabelColumnWidth", cfg.LabelColumnWidth.ToString());
            writer.WriteAttributeString("ValueColumnWidth", cfg.ValueColumnWidth.ToString());
            writer.WriteAttributeString("UnitColumnWidth", cfg.UnitColumnWidth.ToString());
            writer.WriteAttributeString("RowHeight", cfg.RowHeight.ToString());
            writer.WriteAttributeString("HeaderVisible", cfg.HeaderVisible.ToString());
            writer.WriteAttributeString("GridLineVisible", cfg.GridLineVisible.ToString());
            writer.WriteAttributeString("ValueAlignment", cfg.ValueAlignment.ToString());
            writer.WriteAttributeString("BackColorArgb", cfg.BackColorArgb.ToString());
            writer.WriteAttributeString("ForeColorArgb", cfg.ForeColorArgb.ToString());
            writer.WriteAttributeString("LabelForeColorArgb", cfg.LabelForeColorArgb.ToString());
            writer.WriteAttributeString("ValueForeColorArgb", cfg.ValueForeColorArgb.ToString());
            writer.WriteAttributeString("UnitForeColorArgb", cfg.UnitForeColorArgb.ToString());
            writer.WriteAttributeString("CommentForeColorArgb", cfg.CommentForeColorArgb.ToString());
            writer.WriteAttributeString("CommentBackColorArgb", cfg.CommentBackColorArgb.ToString());
            writer.WriteAttributeString("GridLineColorArgb", cfg.GridLineColorArgb.ToString());
            writer.WriteAttributeString("ColumnResizeEnabled", cfg.ColumnResizeEnabled.ToString());
            writer.WriteAttributeString("PanEnabled", cfg.PanEnabled.ToString());

            // DataRows
            foreach (var row in cfg.DataRows)
            {
                writer.WriteStartElement("DataRow");
                writer.WriteAttributeString("RowIndex", row.RowIndex.ToString());
                writer.WriteAttributeString("LabelText", row.LabelText);
                writer.WriteAttributeString("UnitText", row.UnitText);
                writer.WriteEndElement();
            }

            // CommentRows
            foreach (var row in cfg.CommentRows)
            {
                writer.WriteStartElement("CommentRow");
                writer.WriteAttributeString("RowIndex", row.RowIndex.ToString());
                writer.WriteAttributeString("CommentText", row.CommentText);
                writer.WriteEndElement();
            }

            writer.WriteEndElement(); // ViewConfig
            writer.WriteEndElement(); // RamMonitorPane
        }

        private void WriteMultiLayoutGridPane(XmlWriter writer, MultiLayoutGridPaneConfig pane)
        {
            writer.WriteStartElement("MultiLayoutGridPane");
            writer.WriteAttributeString("PaneName", pane.PaneName);

            var cfg = pane.GridConfig;
            writer.WriteStartElement("GridConfig");
            writer.WriteAttributeString("ShowGridLines", cfg.ShowGridLines.ToString());
            writer.WriteAttributeString("GridLineColorArgb", cfg.GridLineColorArgb.ToString());
            writer.WriteAttributeString("BackColorArgb", cfg.BackColorArgb.ToString());
            writer.WriteAttributeString("ForeColorArgb", cfg.ForeColorArgb.ToString());

            foreach (var row in cfg.Rows)
            {
                writer.WriteStartElement("Row");
                writer.WriteAttributeString("RowIndex", row.RowIndex.ToString());
                writer.WriteAttributeString("Height", row.Height.ToString());

                foreach (var cell in row.Cells)
                {
                    writer.WriteStartElement("Cell");
                    writer.WriteAttributeString("CellIndex", cell.CellIndex.ToString());
                    writer.WriteAttributeString("Text", cell.Text);
                    writer.WriteAttributeString("IsVariable", cell.IsVariable.ToString());
                    writer.WriteAttributeString("Width", cell.Width.ToString());
                    writer.WriteAttributeString("HorizontalAlignment", cell.HorizontalAlignment.ToString());
                    writer.WriteAttributeString("VerticalAlignment", cell.VerticalAlignment.ToString());
                    writer.WriteAttributeString("HasForeColor", cell.HasForeColor.ToString());
                    writer.WriteAttributeString("ForeColorArgb", cell.ForeColorArgb.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        /// <summary>
        /// XMLファイルからワークスペースを読み込み
        /// </summary>
        /// <param name="filePath">読み込むファイルパス</param>
        /// <returns>ワークスペース設定</returns>
        public WorkspaceConfig Load(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    throw new FileNotFoundException($"ファイルが見つかりません: {filePath}");
                }

                WorkspaceConfig config = new WorkspaceConfig();

                using (XmlReader reader = XmlReader.Create(filePath))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            switch (reader.Name)
                            {
                                case "Name":
                                    config.Name = reader.ReadElementContentAsString();
                                    break;
                                case "SavedDate":
                                    config.SavedDate = DateTime.Parse(reader.ReadElementContentAsString());
                                    break;
                                case "DockPanelLayout":
                                    reader.Read();
                                    if (reader.NodeType == XmlNodeType.CDATA)
                                    {
                                        config.DockPanelLayout = reader.Value;
                                    }
                                    break;
                                case "GraphPane":
                                    config.GraphPanes.Add(ReadGraphPane(reader));
                                    break;
                                case "RamMonitorPane":
                                    config.RamMonitorPanes.Add(ReadRamMonitorPane(reader));
                                    break;
                                case "MultiLayoutGridPane":
                                    config.MultiLayoutGridPanes.Add(ReadMultiLayoutGridPane(reader));
                                    break;
                            }
                        }
                    }
                }

                return config;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ワークスペースの読み込みに失敗しました: {ex.Message}", ex);
            }
        }

        private GraphPaneConfig ReadGraphPane(XmlReader reader)
        {
            var config = new GraphPaneConfig();
            config.PaneName = reader.GetAttribute("PaneName") ?? string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "GraphConfig")
                {
                    var cfg = new LineGraphConfig();
                    cfg.MaxPoints = int.Parse(reader.GetAttribute("MaxPoints") ?? "60");
                    cfg.AutoScaleY = bool.Parse(reader.GetAttribute("AutoScaleY") ?? "true");
                    cfg.YMin = double.Parse(reader.GetAttribute("YMin") ?? "0");
                    cfg.YMax = double.Parse(reader.GetAttribute("YMax") ?? "100");
                    cfg.LineWidth = float.Parse(reader.GetAttribute("LineWidth") ?? "1.5");
                    cfg.ShowGrid = bool.Parse(reader.GetAttribute("ShowGrid") ?? "true");
                    cfg.FillArea = bool.Parse(reader.GetAttribute("FillArea") ?? "false");
                    cfg.RedrawIntervalMs = int.Parse(reader.GetAttribute("RedrawIntervalMs") ?? "100");
                    cfg.BackColorArgb = int.Parse(reader.GetAttribute("BackColorArgb") ?? "-16777216");
                    cfg.GridColorArgb = int.Parse(reader.GetAttribute("GridColorArgb") ?? "-8355712");
                    cfg.PlotPaddingLeft = int.Parse(reader.GetAttribute("PlotPaddingLeft") ?? "5");
                    cfg.PlotPaddingTop = int.Parse(reader.GetAttribute("PlotPaddingTop") ?? "20");
                    cfg.PlotPaddingRight = int.Parse(reader.GetAttribute("PlotPaddingRight") ?? "5");
                    cfg.PlotPaddingBottom = int.Parse(reader.GetAttribute("PlotPaddingBottom") ?? "25");

                    // Series読み込み
                    XmlReader subReader = reader.ReadSubtree();
                    while (subReader.Read())
                    {
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "GraphSeries")
                        {
                            var series = new GraphSeriesConfig();
                            series.Name = subReader.GetAttribute("Name") ?? string.Empty;
                            series.ColorArgb = int.Parse(subReader.GetAttribute("ColorArgb") ?? "-16777216");
                            series.Visible = bool.Parse(subReader.GetAttribute("Visible") ?? "true");
                            cfg.Series.Add(series);
                        }
                    }

                    config.GraphConfig = cfg;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "GraphPane")
                {
                    break;
                }
            }

            return config;
        }

        private RamMonitorPaneConfig ReadRamMonitorPane(XmlReader reader)
        {
            var config = new RamMonitorPaneConfig();
            config.PaneName = reader.GetAttribute("PaneName") ?? string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "ViewConfig")
                {
                    var cfg = new RamMonitorViewConfig();
                    cfg.LabelColumnWidth = int.Parse(reader.GetAttribute("LabelColumnWidth") ?? "150");
                    cfg.ValueColumnWidth = int.Parse(reader.GetAttribute("ValueColumnWidth") ?? "100");
                    cfg.UnitColumnWidth = int.Parse(reader.GetAttribute("UnitColumnWidth") ?? "60");
                    cfg.RowHeight = int.Parse(reader.GetAttribute("RowHeight") ?? "30");
                    cfg.HeaderVisible = bool.Parse(reader.GetAttribute("HeaderVisible") ?? "true");
                    cfg.GridLineVisible = bool.Parse(reader.GetAttribute("GridLineVisible") ?? "true");
                    cfg.ValueAlignment = int.Parse(reader.GetAttribute("ValueAlignment") ?? "2");
                    cfg.BackColorArgb = int.Parse(reader.GetAttribute("BackColorArgb") ?? "-16777216");
                    cfg.ForeColorArgb = int.Parse(reader.GetAttribute("ForeColorArgb") ?? "-1");
                    cfg.LabelForeColorArgb = int.Parse(reader.GetAttribute("LabelForeColorArgb") ?? "-1");
                    cfg.ValueForeColorArgb = int.Parse(reader.GetAttribute("ValueForeColorArgb") ?? "-1");
                    cfg.UnitForeColorArgb = int.Parse(reader.GetAttribute("UnitForeColorArgb") ?? "-1");
                    cfg.CommentForeColorArgb = int.Parse(reader.GetAttribute("CommentForeColorArgb") ?? "-1");
                    cfg.CommentBackColorArgb = int.Parse(reader.GetAttribute("CommentBackColorArgb") ?? "-16777216");
                    cfg.GridLineColorArgb = int.Parse(reader.GetAttribute("GridLineColorArgb") ?? "-8355712");
                    cfg.ColumnResizeEnabled = bool.Parse(reader.GetAttribute("ColumnResizeEnabled") ?? "true");
                    cfg.PanEnabled = bool.Parse(reader.GetAttribute("PanEnabled") ?? "true");

                    // DataRows/CommentRows読み込み
                    XmlReader subReader = reader.ReadSubtree();
                    while (subReader.Read())
                    {
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "DataRow")
                        {
                            var row = new DataRowConfig();
                            row.RowIndex = int.Parse(subReader.GetAttribute("RowIndex") ?? "0");
                            row.LabelText = subReader.GetAttribute("LabelText") ?? string.Empty;
                            row.UnitText = subReader.GetAttribute("UnitText") ?? string.Empty;
                            cfg.DataRows.Add(row);
                        }
                        else if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "CommentRow")
                        {
                            var row = new CommentRowConfig();
                            row.RowIndex = int.Parse(subReader.GetAttribute("RowIndex") ?? "0");
                            row.CommentText = subReader.GetAttribute("CommentText") ?? string.Empty;
                            cfg.CommentRows.Add(row);
                        }
                    }

                    config.ViewConfig = cfg;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "RamMonitorPane")
                {
                    break;
                }
            }

            return config;
        }

        private MultiLayoutGridPaneConfig ReadMultiLayoutGridPane(XmlReader reader)
        {
            var config = new MultiLayoutGridPaneConfig();
            config.PaneName = reader.GetAttribute("PaneName") ?? string.Empty;

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element && reader.Name == "GridConfig")
                {
                    var cfg = new MultiLayoutGridConfig();
                    cfg.ShowGridLines = bool.Parse(reader.GetAttribute("ShowGridLines") ?? "true");
                    cfg.GridLineColorArgb = int.Parse(reader.GetAttribute("GridLineColorArgb") ?? Color.Gray.ToArgb().ToString());
                    cfg.BackColorArgb = int.Parse(reader.GetAttribute("BackColorArgb") ?? Color.Black.ToArgb().ToString());
                    cfg.ForeColorArgb = int.Parse(reader.GetAttribute("ForeColorArgb") ?? Color.White.ToArgb().ToString());

                    XmlReader subReader = reader.ReadSubtree();
                    MultiLayoutGridRowConfig? currentRow = null;
                    while (subReader.Read())
                    {
                        if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "Row")
                        {
                            currentRow = new MultiLayoutGridRowConfig
                            {
                                RowIndex = int.Parse(subReader.GetAttribute("RowIndex") ?? "0"),
                                Height = int.Parse(subReader.GetAttribute("Height") ?? "30")
                            };
                            cfg.Rows.Add(currentRow);
                        }
                        else if (subReader.NodeType == XmlNodeType.Element && subReader.Name == "Cell" && currentRow != null)
                        {
                            currentRow.Cells.Add(new MultiLayoutGridCellConfig
                            {
                                CellIndex = int.Parse(subReader.GetAttribute("CellIndex") ?? "0"),
                                Text = subReader.GetAttribute("Text") ?? string.Empty,
                                IsVariable = bool.Parse(subReader.GetAttribute("IsVariable") ?? "false"),
                                Width = int.Parse(subReader.GetAttribute("Width") ?? "100"),
                                HorizontalAlignment = int.Parse(subReader.GetAttribute("HorizontalAlignment") ?? ((int)System.Drawing.ContentAlignment.MiddleLeft).ToString()),
                                VerticalAlignment = int.Parse(subReader.GetAttribute("VerticalAlignment") ?? ((int)System.Drawing.ContentAlignment.MiddleCenter).ToString()),
                                HasForeColor = bool.Parse(subReader.GetAttribute("HasForeColor") ?? "false"),
                                ForeColorArgb = int.Parse(subReader.GetAttribute("ForeColorArgb") ?? Color.White.ToArgb().ToString())
                            });
                        }
                    }

                    config.GridConfig = cfg;
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.Name == "MultiLayoutGridPane")
                {
                    break;
                }
            }

            return config;
        }

        /// <summary>
        /// DockPanelのレイアウトを文字列として保存
        /// </summary>
        /// <param name="dockPanel">DockPanel</param>
        /// <returns>レイアウト文字列</returns>
        public string SaveDockPanelLayout(DockPanel dockPanel)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                dockPanel.SaveAsXml(ms, System.Text.Encoding.UTF8);
                ms.Position = 0;
                using (StreamReader reader = new StreamReader(ms, System.Text.Encoding.UTF8))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// DockPanelのレイアウトを文字列から復元
        /// </summary>
        /// <param name="dockPanel">DockPanel</param>
        /// <param name="layoutXml">レイアウトXML文字列</param>
        /// <param name="getPersistStringCallback">コンテンツ復元コールバック</param>
        public void LoadDockPanelLayout(DockPanel dockPanel, string layoutXml, 
            DeserializeDockContent getPersistStringCallback)
        {
            if (string.IsNullOrEmpty(layoutXml))
            {
                return;
            }

            using (MemoryStream ms = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(layoutXml)))
            {
                dockPanel.LoadFromXml(ms, getPersistStringCallback);
            }
        }
    }
}
