# RamMonitorEx - プロダクト仕様書

## 1. 概要

RamMonitorEx は、リアルタイムデータの可視化とモニタリングを行う Windows デスクトップアプリケーションです。  
DockPanel を使用したマルチウィンドウ環境で、以下の表示を組み合わせて利用できます。

- 折れ線グラフ（LineGraph）
- RAMモニタ表示（RamMonitorView）
- マルチレイアウトグリッド（MultiLayoutGrid）
- ELFシンボル専用一覧（ElfSymbolPane）

---

## 2. アーキテクチャ

### 2.1 技術スタック
- .NET 8.0 (Windows Forms)
- DockPanelSuite 3.1.0
- C# 12.0

### 2.2 プロジェクト構成
```
RamMonitorEx/
├── Controls/
│   ├── LineGraphControl/
│   ├── RamMonitorView/
│   └── MultiLayoutGridControl/
├── Docking/
│   ├── GraphPane.cs
│   ├── RamMonitorViewPane.cs
│   ├── MultiLayoutGridPane.cs
│   ├── ElfSymbolPane.cs
│   └── PaneNameManager.cs
├── Forms/
│   ├── Form1.cs                 # クラス名は MainForm
│   ├── Form1.Designer.cs        # partial MainForm
│   ├── Form2.cs
│   ├── PaneNameDialog.cs
│   ├── GraphPropertiesForm.cs
│   ├── MultiLayoutGridPropertiesForm.cs
│   ├── RowPropertiesForm.cs
│   ├── CellPropertiesForm.cs
│   └── ElfSymbolSelectionForm.cs
├── ReadElf/
│   ├── ReadElfParser.cs
│   ├── ReadElfParserSmokeTest.cs
│   └── sample_elf_data.elf
├── Serialization/
│   ├── WorkspaceConfig.cs
│   └── WorkspaceSerializer.cs
└── spec/
    ├── product.md
    ├── LineGraphControl.md
    ├── RamMonitorView.md
    ├── MultiLayoutGridControl .md
    ├── ReadElf.md
    └── ToDo.md
```

---

## 3. メインフォーム（MainForm）

### 3.1 概要
- ファイル配置は `Form1.cs` だが、フォームクラス名は `MainForm`
- デザイン時クラッシュ回避のため、実行時初期化は `OnLoad` で行う

### 3.2 UI構成
- MenuStrip（ファイルメニュー）
- CommandBar（ToolStrip）
- DockPanel

### 3.3 メニュー
**ファイルメニュー**
- 新規RAMモニタ(R)...（Ctrl+M）
- 新規グラフ(G)...（Ctrl+N）
- 新規マルチレイアウトグリッド(L)...
- ELFシンボル選択...
- 名前を付けて保存(S)...（Ctrl+Shift+S）
- 上書き保存(O)（Ctrl+S）
- 開く(L)...（Ctrl+O）
- 終了(X)

### 3.4 CommandBar
- 新規グラフ
- 新規RAMモニタ
- 新規マルチレイアウト
- ELFシンボル選択
- 開く
- 保存

---

## 4. ドッキングペイン

### 4.1 GraphPane
- `LineGraphControl` を表示
- デモ系列を自動更新
- PersistString: `GraphPane|{PaneName}`

### 4.2 RamMonitorViewPane
- `RamMonitorView` を表示
- デモ値を1秒ごと更新
- PersistString: `RamMonitorViewPane|{PaneName}`

### 4.3 MultiLayoutGridPane
- `MultiLayoutGridControl` を表示
- 初期背景: Black / 初期文字色: White / フォント: Regular
- グリッド線: 初期ON、初期色 Gray
- PersistString: `MultiLayoutGridPane|{PaneName}`

### 4.4 ElfSymbolPane
- ELFシンボル表示専用ペイン（DataGridView）
- 列: 選択, シンボル名, アドレス, サイズ, ソース
- シンボル名の絞り込み
- PersistString: `ElfSymbolPane|{PaneName}`

---

## 5. ELF機能

### 5.1 解析
- `ReadElfParser` による ELF 解析
- 対応: ELF32/ELF64, Little Endian
- 取得: Name / Address / Size / SourceTable

### 5.2 選択画面
- `ElfSymbolSelectionForm`
- OpenFileDialog で ELF を選択
- DataSet + DataView + DataGridView で一覧表示
- 先頭列チェックボックス「選択」
- LinkLabel: 全て選択 / 全て選択解除
- シンボル名フィルタ
- OK/キャンセル
- OK時に選択シンボルを呼び出し元へ返却

### 5.3 選択結果の反映
- MainForm で選択結果を受け取り、`ElfSymbolPane` を新規作成して表示

---

## 6. ワークスペース永続化

### 6.1 形式
- 拡張子: `.xml`
- UTF-8 XML

### 6.2 保存対象
- DockPanelレイアウト（CDATA）
- GraphPane 設定
- RamMonitorViewPane 設定
- MultiLayoutGridPane 設定

### 6.3 備考（ElfSymbolPane）
- PersistStringによるペイン復元分岐は実装済み
- ただし、シンボル内容の完全シリアライズ復元は未対応（現状は空データ復元）

---

## 7. ビルド・実行

### 7.1 ビルド
```powershell
dotnet build
```

### 7.2 実行
```powershell
dotnet run
```

---

## 8. 更新履歴
- 2024-XX-XX: 初版
- 2024-XX-XX: ワークスペース永続化追加
- 2024-XX-XX: MultiLayoutGrid 追加
- 2024-XX-XX: MultiLayoutGrid プロパティ編集追加
- 2024-XX-XX: MultiLayoutGrid グリッド線ON/OFF・色設定追加
- 2024-XX-XX: MainForm化、CommandBar追加
- 2024-XX-XX: ELF解析・シンボル選択画面・専用ペイン追加
