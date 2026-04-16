using System;
using System.Collections.Generic;
using System.Drawing;

namespace RamMonitorEx.Controls.LineGraph
{
    /// <summary>
    /// 1本分の折れ線データを保持する系列クラス
    /// </summary>
    public class GraphSeries
    {
        private List<float> values = new List<float>();
        private int maxPoints = 1000;

        public GraphSeries(string name, Color color)
        {
            Name = name;
            Color = color;
            Visible = true;
        }

        /// <summary>
        /// 系列識別名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 系列データ（読み取り専用）
        /// </summary>
        public IReadOnlyList<float> Values => values;

        /// <summary>
        /// 折れ線色
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// 系列表示有無
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// データポイントを追加
        /// </summary>
        public void AddValue(float value)
        {
            values.Add(value);
            
            // 最大点数を超えたら古いデータを削除
            if (values.Count > maxPoints)
            {
                values.RemoveAt(0);
            }
        }

        /// <summary>
        /// データをクリア
        /// </summary>
        public void Clear()
        {
            values.Clear();
        }

        /// <summary>
        /// 最大保持点数を設定
        /// </summary>
        internal void SetMaxPoints(int max)
        {
            maxPoints = max;
            
            // 既存データが最大点数を超えている場合は古いデータを削除
            while (values.Count > maxPoints)
            {
                values.RemoveAt(0);
            }
        }
    }
}