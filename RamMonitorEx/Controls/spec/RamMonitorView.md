# RamMonitoerView(値表示ビュー)仕様案

## 1. 目的
シリアルポート等から取得した最新値を高速表示するための専用ビューを提供する。  
汎用グリッドではなく、表示専用の軽量コントロールとして実装する。

---

## 2. 表示構造

### 2.1 通常行
通常行は以下の3列で構成する。

- ラベル列
- 値列
- 単位列

例:
| Label | Value | Unit |
|-------|-------|------|
| 回転数 | 1200 | rpm |

### 2.2 コメント行
コメント行は3列結合の1行として扱う。  
固定文字列を横一列で表示する。

例:
| コメント: センサ未接続時は値が 0 になります |

---

## 3. 行種別

### 3.1 行種別定義
行は以下のいずれかとする。

- DataRow
  - ラベル列、値列、単位列を表示する通常行
- CommentRow
  - 3列結合のコメント文字列を表示する行

---

## 4. 列仕様

### 4.1 列構成
列は固定で以下の3列とする。

- Label
- Value
- Unit

列の追加・削除は初期対象外とする。

### 4.2 列幅変更
各列の幅はユーザーが変更可能とする。

#### 対応内容
- ラベル列幅変更可能（マウスドラッグで変更）
- 値列幅変更可能（マウスドラッグで変更）
- 単位列幅変更可能（マウスドラッグで変更）

#### 初期値
- ラベル列: 150px
- 値列: 100px
- 単位列: 60px

#### 制約
- 最小幅を持つ（各列30px程度）
- コメント行は列結合表示のため、内部的には3列幅合計で描画する

---

## 5. 列の配置

### 5.1 列の水平配置
各列は以下の水平方向揃えを持つ。

- ラベル列: Left（左揃え）
- 値列: Right（右揃え）※数値表示のため
- 単位列: Left（左揃え）

### 5.2 設定
- 値列の揃え方向は設定可能（Left / Center / Right）
- コントロール全体の既定値として設定可能
- 必要であれば将来的に行単位 override 可能な設計にしておく

---

## 6. コメント行仕様

### 6.1 表示内容
コメント行は固定文字列を表示する。

### 6.2 レイアウト
- 3列結合表示
- ラベル列・値列・単位列の区切り線は表示しない
- 行全幅に対して1つの文字列を描画する

### 6.3 編集
- 初期実装では表示専用
- コメント文字列は設定データから与える

---

## 7. 描画仕様

### 7.1 基本方針
- Owner Draw / 自前描画
- ダブルバッファ有効
- 可視行のみ描画
- 値変更時は必要最小範囲のみ再描画

### 7.2 通常行描画
通常行は以下を描画する。

- 背景
- ラベル文字列
- 値文字列
- 単位文字列
- 列境界線
- 行境界線

### 7.3 コメント行描画
コメント行は以下を描画する。

- 背景
- コメント文字列
- 行境界線

※ 列境界線は描画しない

---

## 8. データモデル

### 8.1 共通インターフェース
全行を同一コレクションで扱えるようにする。

例:
- IValueViewRow

### 8.2 通常行モデル
保持項目:
- RowType = Data
- LabelText
- ValueText
- UnitText

### 8.3 コメント行モデル
保持項目:
- RowType = Comment
- CommentText

---

## 9. コントロール設定項目

### 9.1 レイアウト系
- LabelColumnWidth（初期値: 150）
- ValueColumnWidth（初期値: 100）
- UnitColumnWidth（初期値: 60）
- RowHeight（初期値: 25、将来的にフォント変更で可変）
- HeaderVisible（初期値: true、"Label", "Value", "Unit" という列名を表示）
- GridLineVisible（初期値: true）

### 9.2 表示系
- ValueAlignment（値列の揃え、初期値: Right）
  - Left
  - Center
  - Right
- Font
- ForeColor
- BackColor
- CommentForeColor
- CommentBackColor
- GridLineColor（初期値: Gray）

### 9.3 動作系
- ColumnResizeEnabled（初期値: true、マウスドラッグで列幅変更）
- PanEnabled（初期値: true、マウスドラッグで表示領域移動、スクロールバーは表示しない）
  - ヘッダー行もパン操作に追従して移動する
- DoubleBufferedEnabled（初期値: true）

---

## 10. コンテキストメニュー（右クリックメニュー）

### 10.1 メニュー項目
コントロール上で右クリックすると、以下のメニューが表示される。

- **グリッド線を表示**
  - チェック可能項目
  - グリッド線の表示/非表示を切り替え
  - 初期状態: ON（表示）

- **ヘッダーを表示**
  - チェック可能項目
  - ヘッダー行の表示/非表示を切り替え
  - 初期状態: ON（表示）

### 10.2 実装
- ContextMenuStrip を使用
- メニュー項目のチェック状態と実際の表示状態が同期
- 項目クリックで即座に反映

---

## 11. イベント

- ColumnWidthChanged - 列幅が変更された時
- RowClicked - 行がクリックされた時（行インデックスを返す）
- RowDoubleClicked - 行がダブルクリックされた時（行インデックスを返す）
- PanPositionChanged - パン操作時の表示位置が変更された時

---

## 12. データAPI

### 12.1 行の追加
```csharp
// データ行の追加
valueView.AddDataRow("回転数", "1200", "rpm");

// コメント行の追加
valueView.AddCommentRow("センサ未接続時は値が 0 になります");
```

### 12.2 値の更新
```csharp
// 行インデックスで値を更新
valueView.UpdateValue(rowIndex, "1250");

// 行全体の更新
valueView.UpdateDataRow(rowIndex, "回転数", "1250", "rpm");
```

### 12.3 行の削除
```csharp
// 指定行を削除
valueView.RemoveRow(rowIndex);

// 全行削除
valueView.ClearRows();
```

---

## 13. 実装方針

### 13.1 採用方針
DataGridView は使わず、専用 UserControl とする。

### 13.2 理由
- コメント行の3列結合が素直
- 列幅変更を独自制御しやすい
- 値列の揃え制御が単純
- 表示専用のためグリッド機能を削ぎ落として軽量化できる
- 高頻度更新に向く

---

## 14. クラス構成（実装済み）

### 14.1 コアクラス
- **RamMonitorView** (UserControl)
  - メイン表示コントロール
  - コンテキストメニュー統合
  - パン操作、列幅変更、描画処理

- **ValueViewColumnLayout**
  - 3列の幅管理
  - 列境界検出
  - リサイズ処理

### 14.2 データモデル
- **IValueViewRow**
  - 行の共通インターフェース
  - RowType プロパティ

- **ValueDataRow**
  - 通常行の実装
  - LabelText, ValueText, UnitText

- **ValueCommentRow**
  - コメント行の実装
  - CommentText

### 14.3 列挙型
- **ValueTextAlignment**
  - Left / Center / Right

- **ValueRowType**
  - Data / Comment

---

## 15. サンプル実装（Form2）

### 15.1 実装内容
Form2 にて RamMonitorView の動作サンプルを実装。

```csharp
// RamMonitorView の初期化
ramMonitorView = new RamMonitorView
{
    Location = new Point(10, 10),
    Size = new Size(400, 400),
    BackColor = Color.White,
    ForeColor = Color.Black,
    GridLineVisible = true,
    GridLineColor = Color.Gray,
    HeaderVisible = true,
    ValueAlignment = ValueTextAlignment.Right,
    ColumnResizeEnabled = true,
    PanEnabled = true
};
```

### 15.2 サンプルデータ
- 9つのデータ行（CPU、メモリ、ディスク、温度、回転数、電圧、ネットワーク送受信）
- 2つのコメント行（セクション区切り、注意書き）
- 1秒ごとにランダム値で自動更新

### 15.3 確認可能な機能
- リアルタイムデータ更新
- 列幅のドラッグ変更
- パン操作（マウスドラッグで表示領域移動）
- 行クリック/ダブルクリックイベント
- コンテキストメニュー（グリッド線、ヘッダーの表示切り替え）

---

## 16. 実装完了機能

### 16.1 表示機能 ✅
- [x] データ行表示（3列: Label, Value, Unit）
- [x] コメント行表示（3列結合）
- [x] ヘッダー行表示
- [x] グリッド線表示
- [x] テキスト揃え（Left/Center/Right）
- [x] カスタムカラー設定

### 16.2 操作機能 ✅
- [x] 列幅ドラッグ変更
- [x] パン操作（ヘッダー追従）
- [x] 右クリックメニュー
  - グリッド線表示/非表示
  - ヘッダー表示/非表示

### 16.3 データ操作 ✅
- [x] 行追加（データ行/コメント行）
- [x] 値更新
- [x] 行削除
- [x] 全行クリア

### 16.4 イベント ✅
- [x] ColumnWidthChanged
- [x] RowClicked
- [x] RowDoubleClicked
- [x] PanPositionChanged

### 16.5 パフォーマンス ✅
- [x] ダブルバッファリング
- [x] 可視行のみ描画
- [x] 部分再描画対応

---

## 17. 今回の判断

この仕様は DataGridView の適用範囲を超えつつあるため、  
**最初から専用 UserControl で作るのが正解**。

### 17.1 実装結果
✅ 仕様通りに実装完了  
✅ 高速描画・軽量動作を実現  
✅ 直感的な操作性（右クリックメニュー、パン操作）  
✅ サンプル動作確認済み（Form2）