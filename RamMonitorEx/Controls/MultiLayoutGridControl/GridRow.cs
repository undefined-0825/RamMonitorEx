using System;
using System.Collections.Generic;
using System.Drawing;

namespace RamMonitorEx.Controls.MultiLayoutGrid
{
    /// <summary>
    /// グリッド行の定義
    /// </summary>
    public class GridRow
    {
        /// <summary>
        /// 行の高さ
        /// </summary>
        public int Height { get; set; } = 30;

        /// <summary>
        /// この行に含まれるセルのリスト
        /// </summary>
        public List<GridCell> Cells { get; set; } = new List<GridCell>();

        /// <summary>
        /// 行の描画領域（レイアウト計算時に設定）
        /// </summary>
        internal Rectangle Bounds { get; set; }
    }
}
