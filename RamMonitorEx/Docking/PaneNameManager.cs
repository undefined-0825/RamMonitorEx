using System;
using System.Collections.Generic;
using System.Linq;

namespace WindowsFormsApp1
{
    /// <summary>
    /// ドッキングペインの名前を管理するクラス
    /// </summary>
    public class PaneNameManager
    {
        private static PaneNameManager? _instance;
        private readonly HashSet<string> _registeredNames = new HashSet<string>();
        private readonly Dictionary<string, int> _sequenceCounters = new Dictionary<string, int>();
        private readonly object _lockObject = new object();

        private PaneNameManager()
        {
        }

        /// <summary>
        /// シングルトンインスタンス
        /// </summary>
        public static PaneNameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new PaneNameManager();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 指定したベース名で新しいパネル名を生成して登録する
        /// </summary>
        /// <param name="baseName">ベース名（例: "折れ線グラフパネル"）</param>
        /// <returns>登録された一意のパネル名</returns>
        public string RegisterNewName(string baseName)
        {
            lock (_lockObject)
            {
                if (!_sequenceCounters.ContainsKey(baseName))
                {
                    _sequenceCounters[baseName] = 0;
                }

                string name;
                do
                {
                    _sequenceCounters[baseName]++;
                    name = $"{baseName}{_sequenceCounters[baseName]}";
                }
                while (_registeredNames.Contains(name));

                _registeredNames.Add(name);
                return name;
            }
        }

        /// <summary>
        /// カスタム名を登録する（重複チェック付き）
        /// </summary>
        /// <param name="customName">登録したいカスタム名</param>
        /// <returns>登録に成功した場合true、既に存在する場合false</returns>
        public bool RegisterCustomName(string customName)
        {
            lock (_lockObject)
            {
                if (string.IsNullOrWhiteSpace(customName))
                {
                    throw new ArgumentException("パネル名を空にすることはできません。", nameof(customName));
                }

                if (_registeredNames.Contains(customName))
                {
                    return false;
                }

                _registeredNames.Add(customName);
                return true;
            }
        }

        /// <summary>
        /// 名前が既に登録されているかチェック
        /// </summary>
        /// <param name="name">チェックする名前</param>
        /// <returns>既に存在する場合true</returns>
        public bool IsNameRegistered(string name)
        {
            lock (_lockObject)
            {
                return _registeredNames.Contains(name);
            }
        }

        /// <summary>
        /// 登録された名前を削除（パネルが閉じられた時に呼ぶ）
        /// </summary>
        /// <param name="name">削除する名前</param>
        public void UnregisterName(string name)
        {
            lock (_lockObject)
            {
                _registeredNames.Remove(name);
            }
        }

        /// <summary>
        /// 登録されている全ての名前を取得
        /// </summary>
        public IEnumerable<string> GetAllRegisteredNames()
        {
            lock (_lockObject)
            {
                return _registeredNames.ToList();
            }
        }

        /// <summary>
        /// 全ての登録をクリア（テスト用）
        /// </summary>
        public void ClearAll()
        {
            lock (_lockObject)
            {
                _registeredNames.Clear();
                _sequenceCounters.Clear();
            }
        }
    }
}
