# RamMonitorEx - プロダクト仕様書

## 1. 概要

RamMonitorEx は、リアルタイムデータの可視化とモニタリングを行うための Windows デスクトップアプリケーションです。  
DockPanel を使用したマルチウィンドウ環境で、グラフ表示・数値表示・マルチレイアウトグリッド表示を組み合わせた柔軟なモニタリングが可能です。

### 1.1 主な機能
- 折れ線グラフによるリアルタイムデータ可視化
- 数値テーブルによる詳細データ表示（RamMonitorView）
- マルチレイアウトグリッド表示（MultiLayoutGridControl）
- ドッキング可能なペイン（複数ペイン、タブ化、分割表示対応）
- カスタマイズ可能な表示設定
- ワークスペースの XML 保存・復元

---

## 2. アーキテクチャ

### 2.1 技術スタック
- **フレームワーク**: .NET 8.0 (Windows Forms)
- **ドッキングライブラリ**: DockPanelSuite 3.1.0
- **言語**: C# 12.0
- **ターゲット**: Windows Desktop

### 2.2 プロジェクト構成
```
RamMonitorEx/
├── Controls/
│   ├── LineGraphControl/
│   │   ├── LineGraphControl.cs
│   │   └── GraphSeries.cs
│   ├── RamMonitorView/
│   │   ├── RamMonitorView.cs
│   │   ├── IValueViewRow.cs
│   │   ├── ValueDataRow.cs
│   │   ├── ValueCommentRow.cs
│   │   ├── ValueViewColumnLayout.cs
│   │   ├── ValueTextAlignment.cs
│   │   └── ValueRowType.cs
│   └── MultiLayoutGridControl/
│       ├── MultiLayoutGridControl.cs
│       ├── GridRow.cs
│       └── GridCell.cs
├── Docking/
│   ├── GraphPane.cs
│   ├── RamMonitorViewPane.cs
│   ├── MultiLayoutGridPane.cs
│   └── PaneNameManager.cs
├── Forms/
│   ├── Form1.cs
│   ├── Form2.cs
│   ├── PaneNameDialog.cs
│   ├── GraphPropertiesForm.cs
│   ├── MultiLayoutGridPropertiesForm.cs
│   ├── RowPropertiesForm.cs
│   └── CellPropertiesForm.cs
├── Serialization/
│   ├── WorkspaceConfig.cs
│   └── WorkspaceSerializer.cs
└── spec/
    ├── product.md
    ├── LineGraphControl.md
    ├── RamMonitorView.md
    └── MultiLayoutGridControl .md
```

---

## 3. メインフォーム (Form1)

### 3.1 概要
アプリケーションのメインウィンドウ。DockPanel を使用して各ペインを管理します。

### 3.2 構成要素

#### 3.2.1 DockPanel
- **ライブラリ**: DockPanelSuite 3.1.0
- **テーマ**: VS2015LightTheme
- **スタイル**: DocumentStyle.DockingWindow

#### 3.2.2 メニューバー
**ファイルメニュー**
- **新規RAMモニタ(R)...** - Ctrl+M
- **新規グラフ(G)...** - Ctrl+N
- **新規マルチレイアウトグリッド(L)...**
- **名前を付けて保存(S)...** - Ctrl+Shift+S
- **上書き保存(O)** - Ctrl+S
- **開く(L)...** - Ctrl+O
- **終了(X)**

※ 各「新規～」は PaneNameDialog 経由で名前指定可能。

### 3.3 初期表示
- メインフォーム上にサンプル LineGraphControl を1つ表示
- 0.1秒ごとにサンプルデータ更新

---

## 4. ドッキングペイン

### 4.1 GraphPane
- `DockContent` ベース
- `LineGraphControl` を内包
- 動的更新あり
- 永続化キー: `GraphPane|{PaneName}`

### 4.2 RamMonitorViewPane
- `DockContent` ベース
- `RamMonitorView` を内包
- 1秒更新デモあり
- 永続化キー: `RamMonitorViewPane|{PaneName}`

### 4.3 MultiLayoutGridPane
- `DockContent` ベース
- `MultiLayoutGridControl` を内包
- 初期背景色: Black
- 初期文字色: White
- フォント: Regular（太字なし）
- グリッド線: 初期表示 ON、初期色 Gray
- 永続化キー: `MultiLayoutGridPane|{PaneName}`

---

## 5. MultiLayoutGridControl 機能

### 5.1 基本
- 完全カスタム描画（DataGridView 不使用）
- ダブルバッファリング
- 行・セルの独立レイアウト
- `ValueProvider` 対応（可変表示）

### 5.2 操作
- セル右端ドラッグで列幅変更
- 右クリックメニュー
  - プロパティ...
  - グリッド線を表示（ON/OFF）
  - 背景色...
  - 文字色...
  - グリッド線の色...

### 5.3 プロパティ画面
- グリッド単位: 行の追加・削除・順序変更・高さ変更
- 行単位: 列の追加・削除・順序変更
- 列単位: 固定/可変、テキスト、幅、配置、文字色

---

## 6. ワークスペース永続化

### 6.1 ファイル形式
- 拡張子: `.xml`
- UTF-8 XML

### 6.2 保存対象
- DockPanel レイアウト（CDATA）
- GraphPane 設定
- RamMonitorViewPane 設定
- MultiLayoutGridPane 設定

### 6.3 MultiLayoutGrid の保存内容
- 表示設定: BackColor / ForeColor / ShowGridLines / GridLineColor
- 行: RowIndex / Height
- セル: CellIndex / Text / IsVariable / Width / HorizontalAlignment / VerticalAlignment / ForeColor

### 6.4 復元
- `GetContentFromPersistString` でペイン生成
- 各 `Restore*Config` で個別設定適用

---

## 7. ドッキング機能
- Document / DockLeft / DockRight / DockTop / DockBottom / Float
- タブ化、分割、フロートをサポート
- `HideOnClose = false` のため閉じると破棄

---

## 8. ビルド・実行

### 8.1 環境
- Visual Studio 2022 以降
- .NET 8.0 SDK

### 8.2 ビルド
```powershell
dotnet build
```

### 8.3 実行
```powershell
dotnet run
```

---

## 9. 更新履歴
- 2024-XX-XX: 初版
- 2024-XX-XX: ワークスペース永続化追加
- 2024-XX-XX: メニュー整理
- 2024-XX-XX: MultiLayoutGridControl / MultiLayoutGridPane 追加
- 2024-XX-XX: MultiLayoutGrid プロパティ編集画面追加
- 2024-XX-XX: MultiLayoutGrid グリッド線表示ON/OFF・色設定・XMLシリアライズ対応
