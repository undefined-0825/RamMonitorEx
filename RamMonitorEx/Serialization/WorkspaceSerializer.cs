using System;
using System.IO;
using System.Xml.Serialization;
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

                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                using (StreamWriter writer = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    _serializer.Serialize(writer, config);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ワークスペースの保存に失敗しました: {ex.Message}", ex);
            }
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

                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                using (StreamReader reader = new StreamReader(fs, System.Text.Encoding.UTF8))
                {
                    object? obj = _serializer.Deserialize(reader);
                    if (obj is WorkspaceConfig config)
                    {
                        return config;
                    }
                    throw new InvalidOperationException("ワークスペース設定の読み込みに失敗しました。");
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"ワークスペースの読み込みに失敗しました: {ex.Message}", ex);
            }
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
