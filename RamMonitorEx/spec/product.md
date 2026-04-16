# RamMonitorEx - プロダクト仕様書

## 1. 概要

RamMonitorEx は、リアルタイムデータの可視化とモニタリングを行うための Windows デスクトップアプリケーションです。  
DockPanel を使用したマルチウィンドウ環境で、グラフ表示と数値表示を組み合わせた柔軟なモニタリングが可能です。

### 1.1 主な機能
- 折れ線グラフによるリアルタイムデータ可視化
- 数値テーブルによる詳細データ表示
- ドッキング可能なペイン（複数ペイン、タブ化、分割表示対応）
- カスタマイズ可能な表示設定

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
├── Controls/              # カスタムコントロール
│   ├── LineGraphControl.cs         # 折れ線グラフコントロール
│   ├── RamMonitorView.cs           # 数値表示コントロール
│   └── (その他関連クラス)
├── Docking/               # ドッキングペイン
│   ├── GraphPane.cs                # グラフペイン
│   └── RamMonitorViewPane.cs       # RAMモニタペイン
├── Forms/                 # その他フォーム
│   ├── Form1.cs                    # メインフォーム
│   └── Form2.cs                    # サンプルフォーム（独立ウィンドウ）
└── spec/                  # 仕様書
    ├── product.md                  # 本ドキュメント
    ├── LineGraphControl.md         # グラフコントロール仕様
    └── RamMonitorView.md           # 数値表示コントロール仕様
```

---

## 3. メインフォーム (Form1)

### 3.1 概要
アプリケーションのメインウィンドウ。DockPanel を使用してペインを管理します。

### 3.2 構成要素

#### 3.2.1 DockPanel
- **ライブラリ**: DockPanelSuite 3.1.0
- **テーマ**: VS2015LightTheme
- **スタイル**: DocumentStyle.DockingWindow

#### 3.2.2 メニューバー
以下のメニュー項目を提供：

**ファイルメニュー**
- **新規グラフ(N)** - Ctrl+N
  - 新しいグラフペインを追加
  - タイトル: "グラフ 1", "グラフ 2", ...（自動採番）
  
- **新規RAMモニタ(M)** - Ctrl+M
  - 新しいRAMモニタペインを追加
  - タイトル: "RAMモニタ 1", "RAMモニタ 2", ...（自動採番）
  
- **RamMonitorView サンプル(R)**
  - 独立したサンプルウィンドウ（Form2）を表示
  - DockPanel の外で動作確認用
  
- **終了(X)**
  - アプリケーションを終了

### 3.3 初期表示
- メインフォームには、サンプルとして LineGraphControl が1つ配置されています
- リアルタイムデータ更新（0.1秒ごと）
- 3つのサンプル系列（CPU、メモリ、ディスク）

### 3.4 実装詳細

#### シーケンス管理
```csharp
private int _graphPaneSequence = 0;        // グラフペインの連番
private int _ramMonitorPaneSequence = 0;   // RAMモニタペインの連番
```

#### ペイン追加処理
```csharp
// グラフペイン追加
private void AddNewGraphPane()
{
    _graphPaneSequence++;
    string title = $"グラフ {_graphPaneSequence}";
    GraphPane pane = new GraphPane(title);
    pane.Show(_dockPanel, DockState.Document);
}

// RAMモニタペイン追加
private void AddNewRamMonitorPane()
{
    _ramMonitorPaneSequence++;
    string title = $"RAMモニタ {_ramMonitorPaneSequence}";
    RamMonitorViewPane pane = new RamMonitorViewPane(title);
    pane.Show(_dockPanel, DockState.Document);
}
```

---

## 4. ドッキングペイン

### 4.1 GraphPane（グラフペイン）

#### 概要
折れ線グラフを表示するためのドッキング可能なペイン。

#### 構成
- **基底クラス**: `DockContent`
- **コントロール**: `LineGraphControl`
- **配置**: `Panel` でラップ（Padding: 5px）

#### 初期設定
```csharp
_graphControl = new LineGraphControl
{
    Dock = DockStyle.Fill,
    BackColor = Color.FromArgb(13, 13, 13),  // ダークグレー
    MaxPoints = 60,                           // 60ポイント表示
    AutoScaleY = false,
    YMin = 0,
    YMax = 100,
    LineWidth = 1.5f,
    ShowGrid = true,
    GridColor = Color.FromArgb(40, 40, 40),
    RedrawIntervalMs = 100,                   // 0.1秒ごとに再描画
    FillArea = false,                         // 線のみ（塗りつぶし無し）
    PlotPadding = new Padding(5, 20, 5, 25)
};
```

#### デモデータ
- 2つのサンプル系列を自動生成
- 60ポイントのランダムデータ

#### 外部アクセス
```csharp
public LineGraphControl GraphControl => _graphControl;
```

### 4.2 RamMonitorViewPane（RAMモニタペイン）

#### 概要
数値データをテーブル形式で表示するためのドッキング可能なペイン。

#### 構成
- **基底クラス**: `DockContent`
- **コントロール**: `RamMonitorView`
- **配置**: `Panel` でラップ（Padding: 5px）

#### 初期設定
```csharp
_ramMonitorView = new RamMonitorView
{
    Dock = DockStyle.Fill,
    GridLineVisible = true,
    GridLineColor = Color.Gray,
    HeaderVisible = true,
    ValueAlignment = ValueTextAlignment.Right,
    ColumnResizeEnabled = true,
    PanEnabled = true
};
```

#### サンプルデータ
**データ行（9行）**:
1. CPU使用率（%）
2. メモリ使用率（%）
3. ディスク使用率（%）
4. 温度（℃）
5. 回転数（rpm）
6. 電圧（V）
7. ネットワーク送信（MB/s）
8. ネットワーク受信（MB/s）

**コメント行（2行）**:
- "--- システム情報 ---"（4行目）
- "※ 値は1秒ごとに更新されます"（8行目）

#### 自動更新
- **更新間隔**: 1秒（1000ms）
- **更新内容**: ランダム値でシミュレーション

#### 外部アクセス
```csharp
public RamMonitorView? RamMonitorView => _ramMonitorView;
```

---

## 5. ドッキング機能

### 5.1 DockPanelSuite の機能

#### ドッキングモード
- **Document**: ドキュメント領域にタブ表示
- **DockLeft**: 左側にドッキング
- **DockRight**: 右側にドッキング
- **DockTop**: 上側にドッキング
- **DockBottom**: 下側にドッキング
- **Float**: フローティングウィンドウ

#### 操作方法
1. **ペインの移動**: タイトルバーをドラッグ
2. **タブ化**: 既存ペインの上にドロップ
3. **分割表示**: ペインの端にドロップ
4. **フロート**: デスクトップ上の任意の場所にドロップ
5. **閉じる**: タイトルバーの✕ボタン（HideOnClose = false なので実際に閉じる）

### 5.2 レイアウト保存（未実装）
将来的な拡張として、以下の機能を追加可能：
- レイアウト設定の保存
- アプリケーション起動時の復元
- プリセットレイアウト

---

## 6. サンプルフォーム (Form2)

### 6.1 概要
RamMonitorView の単体動作確認用の独立ウィンドウ。  
DockPanel とは独立して動作します。

### 6.2 起動方法
メニュー: **ファイル → RamMonitorView サンプル(R)**

### 6.3 機能
- RamMonitorView の全機能を確認可能
- 1秒ごとの自動データ更新
- 右クリックメニューによる表示設定
- 列幅変更、パン操作のデモ

---

## 7. ビルド・実行

### 7.1 ビルド環境
- Visual Studio 2022 以降
- .NET 8.0 SDK

### 7.2 ビルドコマンド
```powershell
dotnet build
```

### 7.3 実行
```powershell
dotnet run
```

または Visual Studio から F5 でデバッグ実行

---

## 8. 使用例

### 8.1 複数ペインの配置例

**シナリオ1: 2つのグラフを左右に分割**
1. Ctrl+N でグラフ1を追加
2. Ctrl+N でグラフ2を追加
3. グラフ2のタイトルバーをドラッグして、グラフ1の右端にドロップ

**シナリオ2: グラフとRAMモニタを上下に分割**
1. Ctrl+N でグラフを追加
2. Ctrl+M でRAMモニタを追加
3. RAMモニタのタイトルバーをドラッグして、グラフの下端にドロップ

**シナリオ3: タブ化**
1. Ctrl+N でグラフ1を追加
2. Ctrl+N でグラフ2を追加
3. グラフ2のタイトルバーをドラッグして、グラフ1の中央にドロップ

### 8.2 カスタマイズ例

**グラフの色設定**
- 右クリック → プロパティ → 背景色・グリッド線色を変更

**RAMモニタの色設定**
- 右クリック → 全体 → 背景色・文字色を変更
- 右クリック → セル文字色 → 各列の色を個別に設定

---

## 9. 今後の拡張予定

### 9.1 データ入力
- [ ] シリアルポート対応
- [ ] TCP/UDP ソケット対応
- [ ] ファイル読み込み対応

### 9.2 データ出力
- [ ] CSV エクスポート
- [ ] スクリーンショット保存
- [ ] レポート生成

### 9.3 UI拡張
- [ ] レイアウト保存・復元
- [ ] テーマ切り替え
- [ ] カスタムカラーパレット

### 9.4 パフォーマンス
- [ ] 大量データの最適化
- [ ] メモリ使用量の削減
- [ ] GPU アクセラレーション

---

## 10. ライセンス

プロジェクトのライセンスは未定です。

---

## 11. 参考資料

### 11.1 関連ドキュメント
- [LineGraphControl.md](./LineGraphControl.md) - 折れ線グラフコントロール仕様
- [RamMonitorView.md](./RamMonitorView.md) - 数値表示コントロール仕様

### 11.2 使用ライブラリ
- [DockPanelSuite](https://github.com/dockpanelsuite/dockpanelsuite) - MIT License
- [.NET](https://dotnet.microsoft.com/) - MIT License

---

**更新履歴**
- 2024-XX-XX: 初版作成
