using System;
using System.Drawing;

namespace RamMonitorEx.Controls.MultiLayoutGrid
{
    /// <summary>
    /// グリッドセルの定義
    /// </summary>
    public class GridCell
    {
        /// <summary>
        /// 固定テキスト
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 動的値取得用のプロバイダー（nullの場合はTextを使用）
        /// </summary>
        public Func<string>? ValueProvider { get; set; }

        /// <summary>
        /// セルの幅
        /// </summary>
        public int Width { get; set; } = 100;

        /// <summary>
        /// 水平方向の配置
        /// </summary>
        public ContentAlignment HorizontalAlignment { get; set; } = ContentAlignment.MiddleLeft;

        /// <summary>
        /// 垂直方向の配置
        /// </summary>
        public ContentAlignment VerticalAlignment { get; set; } = ContentAlignment.MiddleCenter;

        /// <summary>
        /// セル固有の文字色（nullの場合は親の色を使用）
        /// </summary>
        public Color? ForeColor { get; set; }

        /// <summary>
        /// セルの描画領域（レイアウト計算時に設定）
        /// </summary>
        internal Rectangle Bounds { get; set; }

        /// <summary>
        /// 表示するテキストを取得
        /// </summary>
        public string GetDisplayText()
        {
            return ValueProvider?.Invoke() ?? Text;
        }
    }
}
